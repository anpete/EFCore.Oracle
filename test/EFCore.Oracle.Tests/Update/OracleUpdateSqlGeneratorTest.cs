// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class OracleUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new OracleUpdateSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new OracleSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies())),
                new OracleTypeMapper(
                    new RelationalTypeMapperDependencies()));

        protected override TestHelpers TestHelpers => OracleTestHelpers.Instance;

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                @"INSERT INTO ""dbo"".""Ducks"" (""Id"", ""Name"", ""Quacks"", ""ConcurrencyToken"")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                @"SELECT ""Computed""" + Environment.NewLine +
                @"FROM ""dbo"".""Ducks""" + Environment.NewLine +
                @"WHERE @@ROWCOUNT = 1 AND ""Id"" = @p0;" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                @"INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                @"SELECT ""Id"", ""Computed""" + Environment.NewLine +
                @"FROM ""dbo"".""Ducks""" + Environment.NewLine +
                @"WHERE @@ROWCOUNT = 1 AND ""Id"" = scope_identity();" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES;
SELECT ""Id""
FROM ""dbo"".""Ducks""
WHERE @@ROWCOUNT = 1 AND ""Id"" = scope_identity();

",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (@p0, @p1, @p2);
SELECT ""Id""
FROM ""dbo"".""Ducks""
WHERE @@ROWCOUNT = 1 AND ""Id"" = scope_identity();

",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES;
SELECT ""Id"", ""Computed""
FROM ""dbo"".""Ducks""
WHERE @@ROWCOUNT = 1 AND ""Id"" = scope_identity();

",
                stringBuilder.ToString());
        }

//        [Fact]
//        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist()
//        {
//            var stringBuilder = new StringBuilder();
//            var command = CreateInsertCommand(identityKey: true, isComputed: true);
//
//            var sqlGenerator = (IOracleUpdateSqlGenerator)CreateSqlGenerator();
//            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);
//
//            AssertBaseline(
//                @"DECLARE @inserted0 TABLE (""Id"" NUMBER(10), ""_Position"" ""int"");
//MERGE ""dbo"".""Ducks"" USING (
//VALUES (@p0, @p1, @p2, 0),
//(@p0, @p1, @p2, 1)) AS i (""Name"", ""Quacks"", ""ConcurrencyToken"", _Position) ON 1=0
//WHEN NOT MATCHED THEN
//INSERT (""Name"", ""Quacks"", ""ConcurrencyToken"")
//VALUES (i.""Name"", i.""Quacks"", i.""ConcurrencyToken"")
//OUTPUT INSERTED.""Id"", i._Position
//INTO @inserted0;
//
//SELECT ""t"".""Id"", ""t"".""Computed"" FROM ""dbo"".""Ducks"" t
//INNER JOIN @inserted0 i ON (""t"".""Id"" = ""i"".""Id"")
//ORDER BY ""i"".""_Position"";
//
//",
//                stringBuilder.ToString());
//            
//            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
//        }
//
//        [Fact]
//        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
//        {
//            var stringBuilder = new StringBuilder();
//            var command = CreateInsertCommand(identityKey: false, isComputed: false);
//
//            var sqlGenerator = (IOracleUpdateSqlGenerator)CreateSqlGenerator();
//            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);
//
//            AssertBaseline(
//                @"INSERT INTO ""dbo"".""Ducks"" (""Id"", ""Name"", ""Quacks"", ""ConcurrencyToken"")
//VALUES (@p0, @p1, @p2, @p3),
//(@p0, @p1, @p2, @p3);
//",
//                stringBuilder.ToString());
//            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
//        }
//
//        [Fact]
//        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
//        {
//            var stringBuilder = new StringBuilder();
//            var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);
//
//            var sqlGenerator = (IOracleUpdateSqlGenerator)CreateSqlGenerator();
//            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);
//
//            AssertBaseline(
//                @"DECLARE @inserted0 TABLE (""Id"" NUMBER(10));
//INSERT INTO ""dbo"".""Ducks"" (""Id"")
//OUTPUT INSERTED.""Id""
//INTO @inserted0
//VALUES (DEFAULT),
//(DEFAULT);
//SELECT ""t"".""Id"", ""t"".""Computed"" FROM ""dbo"".""Ducks"" t
//INNER JOIN @inserted0 i ON (""t"".""Id"" = ""i"".""Id"");
//
//",
//                stringBuilder.ToString());
//            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
//        }
//
//        [Fact]
//        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
//        {
//            var stringBuilder = new StringBuilder();
//            var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);
//
//            var sqlGenerator = (IOracleUpdateSqlGenerator)CreateSqlGenerator();
//            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);
//
//            var expectedText = @"INSERT INTO ""dbo"".""Ducks""
//DEFAULT VALUES;
//";
//            AssertBaseline(expectedText + expectedText,
//                stringBuilder.ToString());
//            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
//        }

        protected override string RowsAffected => "@@ROWCOUNT";

        protected override string Identity => throw new NotImplementedException();

        private const string FileLineEnding = @"
";

        private static void AssertBaseline(string expected, string actual )
        {
            Assert.Equal(expected.Replace(FileLineEnding, Environment.NewLine), actual);
        }
    }
}
