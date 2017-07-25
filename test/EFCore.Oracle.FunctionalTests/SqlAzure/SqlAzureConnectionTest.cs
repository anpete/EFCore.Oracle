// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    [SqlServerCondition(SqlServerCondition.IsSqlAzure)]
    public class SqlAzureConnectionTest : IClassFixture<SqlAzureFixture>
    {
        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Connect_with_encryption(bool encryptionEnabled)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(SqlServerTestStore.CreateConnectionString("adventureworks"))
            {
                Encrypt = encryptionEnabled
            };
            var options = new DbContextOptionsBuilder();
            options.UseSqlServer(connectionStringBuilder.ConnectionString, b => b.ApplyConfiguration());

            using (var context = new AdventureWorksContext(options.Options))
            {
                context.Database.OpenConnection();
                Assert.Equal(ConnectionState.Open, context.Database.GetDbConnection().State);
            }
        }

        public SqlAzureConnectionTest(SqlAzureFixture fixture)
        {
        }
    }
}
