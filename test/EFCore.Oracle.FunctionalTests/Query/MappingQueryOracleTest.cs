// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQueryOracleTest : MappingQueryTestBase, IClassFixture<MappingQueryOracleFixture>
    {
        private readonly MappingQueryOracleFixture _fixture;

        public MappingQueryOracleTest(MappingQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            //_fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();
    }
}
