﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncQueryNavigationsSqlServerTests : AsyncQueryNavigationsTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public AsyncQueryNavigationsSqlServerTests(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}
