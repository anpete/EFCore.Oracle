// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public SqlServerUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(dependencies)
        {
            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            if (modificationCommands.Count == 1
                && modificationCommands[0].ColumnModifications.All(o =>
                    !o.IsKey
                    || !o.IsRead
                    || o.Property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn))
            {
                return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
            var keyOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList();

            var defaultValuesOnly = writeOperations.Count == 0;
            var nonIdentityOperations = modificationCommands[0].ColumnModifications
                .Where(o => o.Property.SqlServer().ValueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
                .ToList();

            if (defaultValuesOnly)
            {
                if (nonIdentityOperations.Count == 0
                    || readOperations.Count == 0)
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperation(commandStringBuilder, modification, commandPosition);
                    }

                    return readOperations.Count == 0
                        ? ResultSetMapping.NoResultSet
                        : ResultSetMapping.LastInResultSet;
                }

                if (nonIdentityOperations.Count > 1)
                {
                    nonIdentityOperations = new List<ColumnModification> { nonIdentityOperations.First() };
                }
            }

            if (readOperations.Count == 0)
            {
                return AppendBulkInsertWithoutServerValues(commandStringBuilder, modificationCommands, writeOperations);
            }

            if (defaultValuesOnly)
            {
                return AppendBulkInsertWithServerValuesOnly(commandStringBuilder, modificationCommands, commandPosition, nonIdentityOperations, keyOperations, readOperations);
            }

            if (modificationCommands[0].Entries.SelectMany(e => e.EntityType.GetAllBaseTypesInclusive())
                .Any(e => e.SqlServer().IsMemoryOptimized))
            {
                if (!nonIdentityOperations.Any(o => o.IsRead && o.IsKey))
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperation(commandStringBuilder, modification, commandPosition++);
                    }
                }
                else
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperationWithServerKeys(commandStringBuilder, modification, keyOperations, readOperations, commandPosition++);
                    }
                }

                return ResultSetMapping.LastInResultSet;
            }

            return AppendBulkInsertWithServerValues(commandStringBuilder, modificationCommands, commandPosition, writeOperations, keyOperations, readOperations);
        }

        private ResultSetMapping AppendBulkInsertWithoutServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            List<ColumnModification> writeOperations)
        {
            Debug.Assert(writeOperations.Count > 0);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.NoResultSet;
        }

        private const string InsertedTableBaseName = "@inserted";
        private const string ToInsertTableAlias = "i";
        private const string PositionColumnName = "_Position";
        private const string PositionColumnDeclaration = "[" + PositionColumnName + "] [int]";
        private const string FullPositionColumnName = ToInsertTableAlias + "." + PositionColumnName;

        private ResultSetMapping AppendBulkInsertWithServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> writeOperations,
            List<ColumnModification> keyOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(
                commandStringBuilder,
                InsertedTableBaseName,
                commandPosition,
                keyOperations,
                PositionColumnDeclaration);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendMergeCommandHeader(
                commandStringBuilder,
                name,
                schema,
                ToInsertTableAlias,
                modificationCommands,
                writeOperations,
                PositionColumnName);
            AppendOutputClause(
                commandStringBuilder,
                keyOperations,
                InsertedTableBaseName,
                commandPosition,
                FullPositionColumnName);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema, orderColumn: PositionColumnName);

            return ResultSetMapping.NotLastInResultSet;
        }

        private ResultSetMapping AppendBulkInsertWithServerValuesOnly(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> nonIdentityOperations,
            List<ColumnModification> keyOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;
            AppendInsertCommandHeader(commandStringBuilder, name, schema, nonIdentityOperations);
            AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
            AppendValuesHeader(commandStringBuilder, nonIdentityOperations);
            AppendValues(commandStringBuilder, nonIdentityOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, nonIdentityOperations);
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);

            return ResultSetMapping.NotLastInResultSet;
        }

        private void AppendMergeCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string toInsertTableAlias,
            [NotNull] IReadOnlyList<ModificationCommand> modificationCommands,
            [NotNull] IReadOnlyList<ColumnModification> writeOperations,
            string additionalColumns = null)
        {
            commandStringBuilder.Append("MERGE ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

            commandStringBuilder
                .Append(" USING (");

            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations, "0");
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(
                    commandStringBuilder,
                    modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                    i.ToString(CultureInfo.InvariantCulture));
            }

            commandStringBuilder
                .Append(") AS ").Append(toInsertTableAlias)
                .Append(" (")
                .AppendJoin(
                    writeOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));
            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ")
                    .Append(additionalColumns);
            }

            commandStringBuilder
                .Append(")")
                .AppendLine(" ON 1=0")
                .AppendLine("WHEN NOT MATCHED THEN");

            commandStringBuilder
                .Append("INSERT ")
                .Append("(")
                .AppendJoin(
                    writeOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
                .Append(")");

            AppendValuesHeader(commandStringBuilder, writeOperations);
            commandStringBuilder
                .Append("(")
                .AppendJoin(
                    writeOperations,
                    toInsertTableAlias,
                    SqlGenerationHelper,
                    (sb, o, alias, helper) =>
                        {
                            sb.Append(alias).Append(".");
                            helper.DelimitIdentifier(sb, o.ColumnName);
                        })
                .Append(")");
        }

        private void AppendValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            string additionalLiteral)
        {
            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("(")
                    .AppendJoin(
                        operations,
                        SqlGenerationHelper,
                        (sb, o, helper) =>
                            {
                                if (o.IsWrite)
                                {
                                    helper.GenerateParameterName(sb, o.ParameterName);
                                }
                                else
                                {
                                    sb.Append("DEFAULT");
                                }
                            })
                    .Append(", ")
                    .Append(additionalLiteral)
                    .Append(")");
            }
        }

        private void AppendDeclareTable(
            StringBuilder commandStringBuilder,
            string name,
            int index,
            IReadOnlyList<ColumnModification> operations,
            string additionalColumns = null)
        {
            commandStringBuilder
                .Append("DECLARE ")
                .Append(name)
                .Append(index)
                .Append(" TABLE (")
                .AppendJoin(
                    operations,
                    this,
                    (sb, o, generator) =>
                        {
                            generator.SqlGenerationHelper.DelimitIdentifier(sb, o.ColumnName);
                            sb.Append(" ").Append(generator.GetTypeNameForCopy(o.Property));
                        });

            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ")
                    .Append(additionalColumns);
            }
            commandStringBuilder
                .Append(")")
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();
        }

        private string GetTypeNameForCopy(IProperty property)
        {
            var typeName = property.SqlServer().ColumnType;
            if (typeName == null)
            {
                var principalProperty = property.FindPrincipal();
                typeName = principalProperty?.SqlServer().ColumnType;
                if (typeName == null)
                {
                    if (property.ClrType == typeof(string))
                    {
                        typeName = _typeMapper.StringMapper?.FindMapping(
                            property.IsUnicode() ?? principalProperty?.IsUnicode() ?? true,
                            keyOrIndex: false,
                            maxLength: null).StoreType;
                    }
                    else if (property.ClrType == typeof(byte[]))
                    {
                        typeName = _typeMapper.ByteArrayMapper?.FindMapping(
                            rowVersion: false,
                            keyOrIndex: false,
                            size: null).StoreType;
                    }
                    else
                    {
                        typeName = _typeMapper.FindMapping(property.ClrType).StoreType;
                    }
                }
            }

            if (property.ClrType == typeof(byte[])
                && typeName != null
                && (typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                    || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase)))
            {
                return property.IsNullable ? "varbinary(8)" : "binary(8)";
            }

            return typeName;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendOutputClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            string tableName,
            int tableIndex,
            string additionalColumns = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                        {
                            sb.Append("INSERTED.");
                            helper.DelimitIdentifier(sb, o.ColumnName);
                        });

            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ").Append(additionalColumns);
            }

            commandStringBuilder.AppendLine()
                .Append("INTO ").Append(tableName).Append(tableIndex);
        }

        private ResultSetMapping AppendInsertOperationWithServerKeys(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            IReadOnlyList<ColumnModification> keyOperations,
            IReadOnlyList<ColumnModification> readOperations,
            int commandPosition)
        {
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();

            AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            return AppendSelectCommand(commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);
        }

        private ResultSetMapping AppendSelectCommand(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> readOperations,
            IReadOnlyList<ColumnModification> keyOperations,
            string insertedTableName,
            int insertedTableIndex,
            string tableName,
            string schema,
            string orderColumn = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "t"))
                .Append(" FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, tableName, schema);
            commandStringBuilder
                .Append(" t")
                .AppendLine()
                .Append("INNER JOIN ")
                .Append(insertedTableName).Append(insertedTableIndex)
                .Append(" i")
                .Append(" ON ")
                .AppendJoin(keyOperations, (sb, c) =>
                    {
                        sb.Append("(");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "t");
                        sb.Append(" = ");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "i");
                        sb.Append(")");
                    }, " AND ");

            if (orderColumn != null)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("ORDER BY ");
                SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, orderColumn, "i");
            }

            commandStringBuilder
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine()
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            commandStringBuilder
                .Append("SELECT @@ROWCOUNT")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine()
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => commandStringBuilder
                .Append("SET NOCOUNT ON")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ");

            commandStringBuilder.Append("scope_identity()");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("@@ROWCOUNT = ")
                .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));
    }
}
