// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IOracleConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        public OracleDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] IOracleConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken);
        }

        protected override bool HasTables()
            => Dependencies.ExecutionStrategyFactory.Create().Execute(
                _connection, connection
                    => Convert.ToInt32(CreateHasTablesCommand().ExecuteScalar(connection)) > 0);

        protected override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                _connection,
                async (connection, ct)
                    => Convert.ToInt32(await CreateHasTablesCommand().ExecuteScalarAsync(connection, cancellationToken: ct)) > 0, cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build("SELECT COUNT(*) FROM user_tables");

        private IEnumerable<MigrationCommand> CreateCreateOperations()
        {
            var builder = new OracleConnectionStringBuilder(_connection.DbConnection.ConnectionString);
            return Dependencies.MigrationsSqlGenerator.Generate(
                new[]
                {
                    new OracleCreateDatabaseOperation
                    {
                        Name = builder.UserID
                    }
                });
        }

        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategyFactory.Create().Execute(
                DateTime.UtcNow + RetryTimeout, giveUp =>
                    {
                        while (true)
                        {
                            try
                            {
                                _connection.Open(errorsExpected: true);
                                _connection.Close();
                                return true;
                            }
                            catch (OracleException e)
                            {
                                if (!retryOnNotExists
                                    && IsDoesNotExist(e))
                                {
                                    return false;
                                }

                                if (DateTime.UtcNow > giveUp
                                    || !RetryOnExistsFailure(e))
                                {
                                    throw;
                                }

                                Thread.Sleep(RetryDelay);
                            }
                        }
                    });

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                    {
                        while (true)
                        {
                            try
                            {
                                await _connection.OpenAsync(ct, errorsExpected: true);

                                _connection.Close();
                                return true;
                            }
                            catch (OracleException e)
                            {
                                if (!retryOnNotExists
                                    && IsDoesNotExist(e))
                                {
                                    return false;
                                }

                                if (DateTime.UtcNow > giveUp
                                    || !RetryOnExistsFailure(e))
                                {
                                    throw;
                                }

                                await Task.Delay(RetryDelay, ct);
                            }
                        }
                    }, cancellationToken);

        // Login failed is thrown when database does not exist (See Issue #776)
        // Unable to attach database file is thrown when file does not exist (See Issue #2810)
        // Unable to open the physical file is thrown when file does not exist (See Issue #2810)
        private static bool IsDoesNotExist(OracleException exception) =>
            exception.Number == 4060 || exception.Number == 1832 || exception.Number == 5120;

        // See Issue #985
        private bool RetryOnExistsFailure(OracleException exception)
        {
            // This is to handle the case where Open throws (Number 233):
            //   OracleException: A connection was successfully established with the
            //   server, but then an error occurred during the login process. (provider: Named Pipes
            //   Provider, error: 0 - No process is on the other end of the pipe.)
            // It appears that this happens when the database has just been created but has not yet finished
            // opening or is auto-closing when using the AUTO_CLOSE option. The workaround is to flush the pool
            // for the connection and then retry the Open call.
            // Also handling (Number -2):
            //   OracleException: Connection Timeout Expired.  The timeout period elapsed while
            //   attempting to consume the pre-login handshake acknowledgment.  This could be because the pre-login
            //   handshake failed or the server was unable to respond back in time.
            // And (Number 4060):
            //   OracleException: Cannot open database "X" requested by the login. The
            //   login failed.
            // And (Number 1832)
            //   OracleException: Unable to Attach database file as database xxxxxxx.
            // And (Number 5120)
            //   OracleException: Unable to open the physical file xxxxxxx.
            if (exception.Number == 233
                || exception.Number == -2
                || exception.Number == 4060
                || exception.Number == 1832
                || exception.Number == 5120)
            {
                ClearPool();
                return true;
            }
            return false;
        }

        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken);
            }
        }

        private IEnumerable<MigrationCommand> CreateDropCommands()
        {
            var databaseName = _connection.DbConnection.Database;
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException(OracleStrings.NoInitialCatalog);
            }

            var operations = new MigrationOperation[]
            {
                new OracleDropDatabaseOperation { Name = databaseName }
            };

            var masterCommands = Dependencies.MigrationsSqlGenerator.Generate(operations);
            
            return masterCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => OracleConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => OracleConnection.ClearPool((OracleConnection)_connection.DbConnection);
    }
}
