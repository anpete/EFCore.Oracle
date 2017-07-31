// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class OracleSqlGeneratorTest : SqlGeneratorTestBase
    {
        [Fact]
        public override void BatchSeparator_returns_separator()
        {
            Assert.Equal("GO" + Environment.NewLine + Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);
        }
        
        [Fact]
        public override void GenerateParameterName_returns_parameter_name()
        {
            var name = CreateSqlGenerationHelper().GenerateParameterName("_2_name");
            
            Assert.Equal(":name", name);
        }

        protected override ISqlGenerationHelper CreateSqlGenerationHelper()
            => new OracleSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
