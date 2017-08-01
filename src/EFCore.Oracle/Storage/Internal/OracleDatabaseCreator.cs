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

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor.ExecuteNonQuery(CreateCreateOperations(), masterConnection);
            }
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken);
            }
        }

        protected override bool HasTables()
            => Dependencies.ExecutionStrategyFactory.Create().Execute(
                _connection, connection
                    => Convert.ToInt32(CreateHasTablesCommand().ExecuteScalar(connection)) > 0);

        protected override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                _connection,
                async (connection, ct)
                    => Convert.ToInt32(await CreateHasTablesCommand().ExecuteScalarAsync(connection, ct)) > 0, cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder.Build("SELECT COUNT(*) FROM user_tables");

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
        {
            try
            {
                _connection.Open(errorsExpected: true);
                _connection.Close();
                
                return true;
            }
            catch (OracleException)
            {
                return false;
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _connection.OpenAsync(cancellationToken, errorsExpected: true);
                
                _connection.Close();
                
                return true;
            }
            catch (OracleException)
            {
                return false;
            }
        }

        public override void Delete()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor.ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken);
            }
        }

        private IEnumerable<MigrationCommand> CreateDropCommands()
        {
            var userName = new OracleConnectionStringBuilder(_connection.DbConnection.ConnectionString).UserID;
            
            if (string.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException(OracleStrings.NoUserId);
            }

            var operations = new MigrationOperation[]
            {
                new OracleDropUserOperation { UserName = userName }
            };

            var masterCommands = Dependencies.MigrationsSqlGenerator.Generate(operations);

            return masterCommands;
        }
    }
}
