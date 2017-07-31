// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable SuggestBaseTypeForParameter

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class OracleTestStore : RelationalTestStore
    {
        private const string Northwind = "Northwind";

        public const int CommandTimeout = 600;

        private static string BaseDirectory => AppContext.BaseDirectory;

        public static readonly string NorthwindConnectionString = CreateConnectionString(Northwind);

        public static OracleTestStore GetNorthwindStore()
            => GetOrCreateShared(
                Northwind,
                () => ExecuteScript(
                    Northwind,
                    Path.Combine(
                        // ReSharper disable once AssignNullToNotNullAttribute
                        Path.GetDirectoryName(typeof(OracleTestStore).GetTypeInfo().Assembly.Location),
                        "Northwind.sql")),
                cleanDatabase: false);

        public static OracleTestStore GetOrCreateShared(string name, Action initializeDatabase, bool cleanDatabase = true)
            => new OracleTestStore(name, cleanDatabase).CreateShared(initializeDatabase);

        public static OracleTestStore Create(string name, bool deleteDatabase = false)
            => new OracleTestStore(name).CreateTransient(createDatabase: true, deleteDatabase: deleteDatabase);

        public static OracleTestStore CreateScratch(bool createDatabase = true, bool useFileName = false)
            => new OracleTestStore(GetScratchDbName(), useFileName).CreateTransient(createDatabase, deleteDatabase: true);

        private OracleConnection _connection;

        private readonly bool _cleanDatabase;
        private string _connectionString;
        private bool _deleteDatabase;

        public string Name { get; }
        public override string ConnectionString => _connectionString;

        private OracleTestStore(string name, bool cleanDatabase = true)
        {
            Name = name;

            _cleanDatabase = cleanDatabase;
        }

        private static string GetScratchDbName()
        {
            string name;
            do
            {
                name = "Scratch_" + Guid.NewGuid();
            }
            while (DatabaseExists(name));

            return name;
        }

        private OracleTestStore CreateShared(Action initializeDatabase)
        {
            _connectionString = CreateConnectionString(Name);
            _connection = new OracleConnection(_connectionString);

            CreateShared(
                typeof(OracleTestStore).Name + Name,
                () =>
                    {
                        if (CreateDatabase())
                        {
                            initializeDatabase?.Invoke();
                        }
                    });

            return this;
        }

        private bool CreateDatabase()
        {
            if (DatabaseExists(Name))
            {
                if (!_cleanDatabase)
                {
                    return false;
                }

                Clean(Name);
            }
            else
            {
                using (var master = new OracleConnection(CreateConnectionString()))
                {
                    ExecuteNonQuery(
                        master,
                        $@"CREATE PLUGGABLE DATABASE {Name}
                        ADMIN USER pdb_{Name} IDENTIFIED BY pdb_{Name}
                        ROLES = (DBA)
                        FILE_NAME_CONVERT = ('\pdbseed\', '\pdb{Name}\')");

                    ExecuteNonQuery(
                        master,
                        $"ALTER PLUGGABLE DATABASE {Name} OPEN");
                }

                using (var pdb = new OracleConnection(CreateConnectionString(Name, $"pdb_{Name}")))
                {
                    ExecuteNonQuery(
                        pdb,
                        $@"CREATE USER {Name} IDENTIFIED BY {Name}");

                    ExecuteNonQuery(
                        pdb,
                        $@"GRANT DBA TO {Name}");
                }
            }

            return true;
        }

        public static void ExecuteScript(string databaseName, string scriptPath)
        {
            // HACK: Probe for script file as current dir
            // is different between k build and VS run.
            if (File.Exists(@"..\..\" + scriptPath))
            {
                //executing in VS - so path is relative to bin\<config> dir
                scriptPath = @"..\..\" + scriptPath;
            }
            else
            {
                scriptPath = Path.Combine(BaseDirectory, scriptPath);
            }

            var script = File.ReadAllText(scriptPath);

            using (var connection = new OracleConnection(CreateConnectionString(databaseName)))
            {
                Execute(
                    connection, command =>
                        {
                            var statements = Regex.Split(script, @";[\r?\n]\s+", RegexOptions.Multiline);

                            foreach (var statement in statements)
                            {
                                if (string.IsNullOrWhiteSpace(statement)
                                    || statement.StartsWith("SET ", StringComparison.Ordinal))
                                {
                                    continue;
                                }

                                command.CommandText = statement;
                                command.ExecuteNonQuery();
                            }

                            return 0;
                        }, "");
            }
        }

        private OracleTestStore CreateTransient(bool createDatabase, bool deleteDatabase)
        {
            _connectionString = CreateConnectionString(Name);
            _connection = new OracleConnection(_connectionString);

            if (createDatabase)
            {
                CreateDatabase();

                OpenConnection();
            }
            else if (DatabaseExists(Name))
            {
                DeleteDatabase(Name);
            }

            _deleteDatabase = deleteDatabase;

            return this;
        }

        private static void Clean(string name)
        {
            var options = new DbContextOptionsBuilder()
                .UseOracle(CreateConnectionString(name), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkOracle()
                        .BuildServiceProvider())
                .Options;

            using (var context = new DbContext(options))
            {
                //context.Database.EnsureClean();
                DeleteDatabase(name);

            }
        }

        private static bool DatabaseExists(string name)
        {
            using (var master = new OracleConnection(CreateConnectionString()))
            {
                return ExecuteScalar<int>(master, $@"SELECT COUNT(*) FROM v$pdbs WHERE name = '{name.ToUpperInvariant()}'") > 0;
            }
        }

        private static void DeleteDatabase(string name)
        {
            using (var master = new OracleConnection(CreateConnectionString()))
            {
                ExecuteNonQuery(
                    master,
                    $"ALTER PLUGGABLE DATABASE {name} CLOSE");
                
                ExecuteNonQuery(
                        master,
                        $@"DROP PLUGGABLE DATABASE {name} INCLUDING DATAFILES");

                OracleConnection.ClearAllPools();
            }
        }
        
        public override DbConnection Connection => _connection;
        public override DbTransaction Transaction => null;

        public override void OpenConnection()
        {
            _connection.Open();
        }

        public Task OpenConnectionAsync()
            => _connection.OpenAsync();

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(_connection, sql, parameters);

        private static T ExecuteScalar<T>(OracleConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T)), sql, useTransaction: false, parameters: parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(_connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(OracleConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, useTransaction: false, parameters: parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(_connection, sql, parameters);

        private static int ExecuteNonQuery(OracleConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, useTransaction: false, parameters: parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(_connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(OracleConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, useTransaction: false, parameters: parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(_connection, sql, parameters);

        private static IEnumerable<T> Query<T>(OracleConnection connection, string sql, object[] parameters = null)
            => Execute(
                connection, command =>
                    {
                        using (var dataReader = command.ExecuteReader())
                        {
                            var results = Enumerable.Empty<T>();
                            while (dataReader.Read())
                            {
                                results = results.Concat(new[] { dataReader.GetFieldValue<T>(ordinal: 0) });
                            }
                            return results;
                        }
                    },
                sql,
                useTransaction: false,
                parameters: parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(_connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(OracleConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(
                connection, async command =>
                    {
                        using (var dataReader = await command.ExecuteReaderAsync())
                        {
                            var results = Enumerable.Empty<T>();
                            while (await dataReader.ReadAsync())
                            {
                                results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(ordinal: 0) });
                            }
                            return results;
                        }
                    },
                sql,
                useTransaction: false,
                parameters: parameters);

        private static T Execute<T>(
            OracleConnection connection, Func<DbCommand, T> execute, string sql,
            bool useTransaction = false, object[] parameters = null)
            => ExecuteCommand(connection, execute, sql, useTransaction, parameters);

        private static T ExecuteCommand<T>(
            OracleConnection connection, Func<DbCommand, T> execute, string sql, bool useTransaction, object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Open();

            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        command.Transaction = transaction;
                        result = execute(command);
                    }

                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Closed
                    && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static Task<T> ExecuteAsync<T>(
            OracleConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql,
            bool useTransaction = false, IReadOnlyList<object> parameters = null)
            => ExecuteCommandAsync(connection, executeAsync, sql, useTransaction, parameters);

        private static async Task<T> ExecuteCommandAsync<T>(
            OracleConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql, bool useTransaction, IReadOnlyList<object> parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            await connection.OpenAsync();
            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        result = await executeAsync(command);
                    }
                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Closed
                    && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static DbCommand CreateCommand(
            OracleConnection connection, string commandText, IReadOnlyList<object> parameters = null)
        {
            var command = connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    command.Parameters.Add("p" + i, parameters[i]);
                }
            }

            return command;
        }

        public override void Dispose()
        {
            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(Name);
            }
        }

        public static string CreateConnectionString(string name = null, string user = null)
        {
            var oracleConnectionStringBuilder = new OracleConnectionStringBuilder();

            if (!string.IsNullOrEmpty(name))
            {
                user = user ?? name;

                oracleConnectionStringBuilder.DataSource = $"//localhost:1521/{name}.redmond.corp.microsoft.com";
                //oracleConnectionStringBuilder.DataSource = $"//localhost:1521/{name}";
                oracleConnectionStringBuilder.UserID = user;
                oracleConnectionStringBuilder.Password = user;
            }
            else
            {
                oracleConnectionStringBuilder.DataSource = "//localhost:1521/orcl.redmond.corp.microsoft.com";
                //oracleConnectionStringBuilder.DataSource = "//localhost:1521/orcl";
                oracleConnectionStringBuilder.UserID = "sys";
                oracleConnectionStringBuilder.Password = "oracle";
                oracleConnectionStringBuilder.DBAPrivilege = "SYSDBA";
            }

            return oracleConnectionStringBuilder.ToString();
        }
    }
}
