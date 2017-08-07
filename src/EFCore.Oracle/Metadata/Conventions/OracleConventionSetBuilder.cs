// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class OracleConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public OracleConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new OracleValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);

            ValueGeneratorConvention valueGeneratorConvention = new OracleValueGeneratorConvention();
            ReplaceConvention(conventionSet.BaseEntityTypeChangedConventions, valueGeneratorConvention);

            var oracleInMemoryTablesConvention = new OracleMemoryOptimizedTablesConvention();
            conventionSet.EntityTypeAnnotationChangedConventions.Add(oracleInMemoryTablesConvention);

            ReplaceConvention(conventionSet.PrimaryKeyChangedConventions, valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(oracleInMemoryTablesConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            var oracleIndexConvention = new OracleIndexConvention(_sqlGenerationHelper);
            conventionSet.IndexAddedConventions.Add(oracleInMemoryTablesConvention);
            conventionSet.IndexAddedConventions.Add(oracleIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(oracleIndexConvention);

            conventionSet.IndexAnnotationChangedConventions.Add(oracleIndexConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(oracleIndexConvention);

            conventionSet.PropertyAnnotationChangedConventions.Add(oracleIndexConvention);
            conventionSet.PropertyAnnotationChangedConventions.Add((OracleValueGeneratorConvention)valueGeneratorConvention);

            return conventionSet;
        }

        public static ConventionSet Build()
        {
            var oracleTypeMapper = new OracleTypeMapper(new RelationalTypeMapperDependencies());

            return new OracleConventionSetBuilder(
                    new RelationalConventionSetBuilderDependencies(oracleTypeMapper, null, null),
                    new OracleSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                .AddConventions(
                    new CoreConventionSetBuilder(
                            new CoreConventionSetBuilderDependencies(oracleTypeMapper))
                        .CreateConventionSet());
        }
    }
}
