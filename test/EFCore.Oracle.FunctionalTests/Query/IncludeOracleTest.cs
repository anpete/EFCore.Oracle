// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOracleTest : IncludeTestBase<NorthwindQueryOracleFixture>
    {
        public IncludeOracleTest(NorthwindQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Include_duplicate_reference(bool useString)
        {
            // TODO: Investigate
        }
    }
}
