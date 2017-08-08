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

        public override void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(false);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
BEGIN
DELETE FROM ""dbo"".""Ducks""
WHERE ""Id"" = :p0;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR SELECT v_RowCount FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        public override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(concurrencyToken: true);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
BEGIN
DELETE FROM ""dbo"".""Ducks""
WHERE ""Id"" = :p0 AND ""ConcurrencyToken"" IS NULL;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR SELECT v_RowCount FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_Id NUMBER(10);
v_Computed RAW(16);
v_RowCount INTEGER;
BEGIN
INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (:p0, :p1, :p2)
RETURN ""Id"", ""Computed"" INTO v_Id, v_Computed;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Id, v_Computed FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_Computed RAW(16);
BEGIN
INSERT INTO ""dbo"".""Ducks"" (""Id"", ""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (:p0, :p1, :p2, :p3)
RETURN ""Computed"" INTO v_Computed;
OPEN :cur FOR
SELECT v_Computed FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_Id NUMBER(10);
v_RowCount INTEGER;
BEGIN
INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (:p0, :p1, :p2)
RETURN ""Id"" INTO v_Id;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Id FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_Id NUMBER(10);
v_Computed RAW(16);
v_RowCount INTEGER;
BEGIN
INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES
RETURN ""Id"", ""Computed"" INTO v_Id, v_Computed;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Id, v_Computed FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_Id NUMBER(10);
v_RowCount INTEGER;
BEGIN
INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES
RETURN ""Id"" INTO v_Id;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Id FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
v_Computed RAW(16);
BEGIN
UPDATE ""dbo"".""Ducks"" SET ""Name"" = :p0, ""Quacks"" = :p1, ""ConcurrencyToken"" = :p2
WHERE ""Id"" = :p3 AND ""ConcurrencyToken"" IS NULL
RETURN ""Computed"" INTO v_Computed;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Computed
FROM DUAL
WHERE v_RowCount = 1;
END;",
                stringBuilder.ToString());
        }

        public override void AppendUpdateOperation_appends_update_and_select_rowcount_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
BEGIN
UPDATE ""dbo"".""Ducks"" SET ""Name"" = :p0, ""Quacks"" = :p1, ""ConcurrencyToken"" = :p2
WHERE ""Id"" = :p3;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR SELECT v_RowCount FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        public override void AppendUpdateOperation_appends_where_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
BEGIN
UPDATE ""dbo"".""Ducks"" SET ""Name"" = :p0, ""Quacks"" = :p1, ""ConcurrencyToken"" = :p2
WHERE ""Id"" = :p3 AND ""ConcurrencyToken"" IS NULL;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR SELECT v_RowCount FROM DUAL;
END;",
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            AssertBaseline(
                @"DECLARE
v_RowCount INTEGER;
v_Computed RAW(16);
BEGIN
UPDATE ""dbo"".""Ducks"" SET ""Name"" = :p0, ""Quacks"" = :p1, ""ConcurrencyToken"" = :p2
WHERE ""Id"" = :p3
RETURN ""Computed"" INTO v_Computed;
v_RowCount := SQL%ROWCOUNT;
OPEN :cur FOR
SELECT v_Computed
FROM DUAL
WHERE v_RowCount = 1;
END;",
                stringBuilder.ToString());
        }

        protected override string RowsAffected => "SQL%ROWCOUNT";

        protected override string Identity => throw new NotImplementedException();

        private const string FileLineEnding = @"
";

        private static void AssertBaseline(string expected, string actual)
        {
            Assert.Equal(expected.Replace(FileLineEnding, Environment.NewLine), actual);
        }
    }
}
