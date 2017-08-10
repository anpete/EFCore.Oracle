// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQueryOracleTest
        : InheritanceRelationshipsQueryTestBase<OracleTestStore, InheritanceRelationshipsQueryOracleFixture>
    {
        public InheritanceRelationshipsQueryOracleTest(
            InheritanceRelationshipsQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }
    }
}
