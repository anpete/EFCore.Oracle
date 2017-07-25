// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerConnectionTest
    {
        [Fact]
        public void Creates_SQL_Server_connection_string()
        {
            using (var connection = new SqlServerConnection(CreateDependencies()))
            {
                Assert.IsType<SqlConnection>(connection.DbConnection);
            }
        }

        [Fact]
        public void Can_create_master_connection()
        {
            using (var connection = new SqlServerConnection(CreateDependencies()))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
                    Assert.Equal(SqlServerConnection.DefaultMasterConnectionCommandTimeout, master.CommandTimeout);
                }
            }
        }

        [Fact]
        public void Master_connection_string_contains_filename()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest;AttachDBFilename=C:\Narf.mdf",
                    b => b.CommandTimeout(55))
                .Options;

            using (var connection = new SqlServerConnection(CreateDependencies(options)))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
                }
            }
        }

        [Fact]
        public void Master_connection_string_none_default_command_timeout()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest",
                    b => b.CommandTimeout(55))
                .Options;

            using (var connection = new SqlServerConnection(CreateDependencies(options)))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(55, master.CommandTimeout);
                }
            }
        }

        public static RelationalConnectionDependencies CreateDependencies(DbContextOptions options = null)
        {
            options = options
                      ?? new DbContextOptionsBuilder()
                          .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest")
                          .Options;

            return new RelationalConnectionDependencies(
                options,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener")),
                new DiagnosticsLogger<DbLoggerCategory.Database.Connection>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener")),
                new NamedConnectionStringResolver(options));
        }
    }
}
