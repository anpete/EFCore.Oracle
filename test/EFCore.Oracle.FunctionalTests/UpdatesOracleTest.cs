// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesOracleTest : UpdatesRelationalTestBase<UpdatesOracleFixture, OracleTestStore>
    {
        public UpdatesOracleTest(UpdatesOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public override void SaveChanges_processes_all_tracked_entities()
        {
            base.SaveChanges_processes_all_tracked_entities();
        }
    }
}
