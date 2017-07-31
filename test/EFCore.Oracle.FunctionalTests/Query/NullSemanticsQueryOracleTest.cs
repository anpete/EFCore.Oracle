// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQueryOracleTest : NullSemanticsQueryTestBase<OracleTestStore, NullSemanticsQueryOracleFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public NullSemanticsQueryOracleTest(NullSemanticsQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Compare_bool_with_bool_equal()
        {
            base.Compare_bool_with_bool_equal();
        }
    }
}
