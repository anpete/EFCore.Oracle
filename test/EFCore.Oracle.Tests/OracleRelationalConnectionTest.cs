// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleRelationalConnectionTest
    {
        [Fact]
        public void Creates_Oracle_connection_string()
        {
            using (var connection = new OracleRelationalConnection(CreateDependencies()))
            {
                Assert.IsType<OracleConnection>(connection.DbConnection);
            }
        }

        public static RelationalConnectionDependencies CreateDependencies(DbContextOptions options = null)
        {
            options = options
                      ?? new DbContextOptionsBuilder()
                          .UseOracle(OracleTestHelpers.TestConnectionString)
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
