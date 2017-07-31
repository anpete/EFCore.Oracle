// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleRelationalConnection : RelationalConnection, IOracleConnection
    {
        // Compensate for slow SQL Server database creation
        internal const int DefaultMasterConnectionCommandTimeout = 60;

        public OracleRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new OracleConnection(ConnectionString);

        public override bool IsMultipleActiveResultSetsEnabled => true;

        // TODO use clone connection method once implemented see #1406

        public virtual IOracleConnection CreateMasterConnection()
        {
            var connectionStringBuilder = new OracleConnectionStringBuilder(ConnectionString);

            var contextOptions = new DbContextOptionsBuilder()
                .UseOracle(
                    connectionStringBuilder.ConnectionString,
                    b => b.CommandTimeout(CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
                .Options;

            return new OracleRelationalConnection(Dependencies.With(contextOptions));
        }
    }
}
