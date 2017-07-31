// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class OracleOptions : IOracleOptions
    {
        public virtual void Initialize(IDbContextOptions options)
        {
        }

        public virtual void Validate(IDbContextOptions options)
        {
        }
    }
}
