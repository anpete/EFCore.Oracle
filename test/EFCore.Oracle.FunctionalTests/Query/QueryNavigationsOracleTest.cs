// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsOracleTest : QueryNavigationsTestBase<NorthwindQueryOracleFixture>
    {
        public QueryNavigationsOracleTest(
            NorthwindQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override void Select_collection_navigation_multi_part()
        {
            // TODO: Test data ordering issue?
        }
    }
}
