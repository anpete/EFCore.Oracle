// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncIncludeOracleTest : IncludeAsyncTestBase<NorthwindQueryOracleFixture>
    {
        public AsyncIncludeOracleTest(NorthwindQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override Task Include_duplicate_reference()
        {
            // TODO: Investigate
            return null;
        }
    }
}
