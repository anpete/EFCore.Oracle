// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersInheritanceOracleTest : FiltersInheritanceTestBase<OracleTestStore, FiltersInheritanceOracleFixture>
    {
        public FiltersInheritanceOracleTest(FiltersInheritanceOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();
        }
    }
}
