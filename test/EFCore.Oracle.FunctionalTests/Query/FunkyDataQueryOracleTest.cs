// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQueryOracleTest : FunkyDataQueryTestBase<OracleTestStore, FunkyDataQueryOracleFixture>
    {
        public FunkyDataQueryOracleTest(FunkyDataQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void String_ends_with_inside_conditional_negated()
        {
            base.String_ends_with_inside_conditional_negated();
        }
    }
}
