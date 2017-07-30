// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindRowNumberPagingQueryOracleFixture : NorthwindQueryOracleFixture
    {
        protected override void ConfigureOptions(OracleDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
            => sqlServerDbContextOptionsBuilder.UseRowNumberForPaging();
    }
}
