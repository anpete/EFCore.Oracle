// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class OracleModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var batch = new OracleModificationCommandBatch(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    new OracleTypeMapper(new RelationalTypeMapperDependencies())),
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
                1);

            Assert.True(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.False(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }
    }
}
