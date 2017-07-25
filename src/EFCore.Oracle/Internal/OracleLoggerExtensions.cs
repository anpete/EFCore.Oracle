// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class OracleLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DecimalTypeDefaultWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = OracleStrings.LogDefaultDecimalTypeColumn;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        DecimalTypeDefaultWarning,
                        property));
            }
        }

        private static string DecimalTypeDefaultWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ByteIdentityColumnWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = OracleStrings.LogByteIdentityColumn;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        ByteIdentityColumnWarning,
                        property));
            }
        }

        private static string ByteIdentityColumnWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName,
            [CanBeNull] string dataTypeName,
            int? ordinal,
            bool? nullable,
            int? primaryKeyOrdinal,
            [CanBeNull] string defaultValue,
            [CanBeNull] string computedValue,
            int? precision,
            int? scale,
            int? maxLength,
            [CanBeNull] bool? identity,
            [CanBeNull] bool? computed)
        {
            // No DiagnosticsSource events because these are purely design-time messages

            var definition = OracleStrings.LogFoundColumn;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        columnName,
                        dataTypeName,
                        ordinal,
                        nullable,
                        primaryKeyOrdinal,
                        defaultValue,
                        computedValue,
                        precision,
                        scale,
                        maxLength,
                        identity,
                        computed));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string principalTableName,
            [CanBeNull] string columnName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string updateAction,
            [CanBeNull] string deleteAction,
            int? ordinal)
        {
            // No DiagnosticsSource events because these are purely design-time messages

            var definition = OracleStrings.LogFoundForeignKeyColumn;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        foreignKeyName,
                        principalTableName,
                        columnName,
                        principalColumnName,
                        updateAction,
                        deleteAction,
                        ordinal));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DefaultSchemaFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogFoundDefaultSchema.Log(diagnostics, schemaName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TypeAliasFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string typeAliasName,
            [CanBeNull] string systemTypeName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogFoundTypeAlias.Log(diagnostics, typeAliasName, systemTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogColumnNameEmptyOnIndex.Log(diagnostics, indexName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogColumnNotInSelectionSet.Log(diagnostics, columnName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogForeignKeyColumnNotInSelectionSet.Log(diagnostics, columnName, foreignKeyName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogForeignKeyNameEmpty.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [NotNull] IList<string> unmappedColumnNames)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogForeignKeyScaffoldErrorPropertyNotFound.Log(
                diagnostics,
                foreignKeyName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogPrincipalTableNotInSelectionSet.Log(diagnostics, foreignKeyName, tableName, principalTableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            bool? unique,
            [CanBeNull] string columnName,
            int? ordinal)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogFoundIndexColumn.Log(diagnostics, indexName, tableName, columnName, ordinal);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            [CanBeNull] string columnName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogIndexColumnNotInSelectionSet.Log(diagnostics, columnName, indexName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [NotNull] IList<string> unmappedColumnNames)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogUnableToScaffoldIndexMissingProperty.Log(
                diagnostics,
                indexName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogIndexNameEmpty.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexTableMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogUnableToFindTableForIndex.Log(diagnostics, indexName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingSchemaWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string schemaName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogMissingSchema.Log(diagnostics, schemaName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogMissingTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string sequenceName,
            [CanBeNull] string sequenceTypeName,
            bool? cyclic,
            int? increment,
            long? start,
            long? min,
            long? max)
        {
            // No DiagnosticsSource events because these are purely design-time messages
            var definition = OracleStrings.LogFoundSequence;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogSequencesRequireName.Log(diagnostics);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogFoundTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogTableNotInSelectionSet.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogColumnNameEmptyOnTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => OracleStrings.LogColumnNameEmptyOnForeignKey.Log(diagnostics, tableName, foreignKeyName);
    }
}
