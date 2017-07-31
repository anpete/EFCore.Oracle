﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class OracleModelValidator : RelationalModelValidator
    {
        public OracleModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            ValidateDefaultDecimalMapping(model);
            ValidateByteIdentityMapping(model);
            ValidateNonKeyValueGeneration(model);
        }

        protected virtual void ValidateDefaultDecimalMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => p.ClrType.UnwrapNullableType() == typeof(decimal)
                         && p.Oracle().ColumnType == null))
            {
                Dependencies.Logger.DecimalTypeDefaultWarning(property);
            }
        }

        protected virtual void ValidateByteIdentityMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => p.ClrType.UnwrapNullableType() == typeof(byte)
                         && p.Oracle().ValueGenerationStrategy == OracleValueGenerationStrategy.IdentityColumn))
            {
                Dependencies.Logger.ByteIdentityColumnWarning(property);
            }
        }

        protected virtual void ValidateNonKeyValueGeneration([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p =>
                        ((OraclePropertyAnnotations)p.Oracle()).GetOracleValueGenerationStrategy(fallbackToModel: false) == OracleValueGenerationStrategy.SequenceHiLo
                        && !p.IsKey()))
            {
                throw new InvalidOperationException(
                    OracleStrings.NonKeyValueGeneration(property.Name, property.DeclaringEntityType.DisplayName()));
            }
        }

        protected override void ValidateSharedTableCompatibility(
            IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            var firstMappedType = mappedTypes[0];
            var isMemoryOptimized = firstMappedType.Oracle().IsMemoryOptimized;

            foreach (var otherMappedType in mappedTypes.Skip(1))
            {
                if (isMemoryOptimized != otherMappedType.Oracle().IsMemoryOptimized)
                {
                    throw new InvalidOperationException(
                        OracleStrings.IncompatibleTableMemoryOptimizedMismatch(
                            tableName, firstMappedType.DisplayName(), otherMappedType.DisplayName(),
                            isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName(),
                            !isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName()));
                }
            }

            base.ValidateSharedTableCompatibility(mappedTypes, tableName);
        }

        protected override void ValidateSharedColumnsCompatibility(IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            base.ValidateSharedColumnsCompatibility(mappedTypes, tableName);

            var identityColumns = mappedTypes.SelectMany(et => et.GetDeclaredProperties())
                .Where(p => p.Oracle().ValueGenerationStrategy == OracleValueGenerationStrategy.IdentityColumn)
                .Distinct((p1, p2) => p1.Name == p2.Name)
                .ToList();

            if (identityColumns.Count > 1)
            {
                var sb = new StringBuilder()
                    .AppendJoin(identityColumns.Select(p => "'" + p.DeclaringEntityType.DisplayName() + "." + p.Name + "'"));
                throw new InvalidOperationException(OracleStrings.MultipleIdentityColumns(sb, tableName));
            }
        }
    }
}
