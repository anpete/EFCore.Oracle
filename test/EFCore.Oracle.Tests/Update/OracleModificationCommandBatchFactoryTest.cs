// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class OracleModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_OracleOptionsExtension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseOracle("Database=Crunchie", b => b.MaxBatchSize(1));

            var factory = new OracleModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    new OracleTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new OracleSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new OracleUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new OracleSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies())),
                    new OracleTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new UntypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies()),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }

        [Fact(Skip = "Batching not implemented")]
        public void MaxBatchSize_is_optional()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseOracle("Database=Crunchie");

            var factory = new OracleModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    new OracleTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new OracleSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new OracleUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new OracleSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies())),
                    new OracleTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new UntypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies()),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }
    }
}
