// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class DbSetAsTableNameOracleTest : DbSetAsTableNameTest
    {
        protected override string GetTableName<TEntity>(DbContext context)
            => context.Model.FindEntityType(typeof(TEntity)).Oracle().TableName;

        protected override SetsContext CreateContext() => new OracleSetsContext();

        protected override SetsContext CreateNamedTablesContext() => new OracleNamedTablesContextContext();

        protected class OracleSetsContext : SetsContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle("Database = Dummy");
        }

        protected class OracleNamedTablesContextContext : NamedTablesContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle("Database = Dummy");
        }
    }
}
