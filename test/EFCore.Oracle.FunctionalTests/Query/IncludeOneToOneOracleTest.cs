// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneOracleTest : IncludeOneToOneTestBase, IClassFixture<IncludeOneToOneQueryOracleFixture>
    {
        private readonly IncludeOneToOneQueryOracleFixture _fixture;

        public IncludeOneToOneOracleTest(IncludeOneToOneQueryOracleFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();
    }
}
