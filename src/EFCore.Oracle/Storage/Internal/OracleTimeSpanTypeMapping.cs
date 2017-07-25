// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleTimeSpanTypeMapping : TimeSpanTypeMapping
    {
        public OracleTimeSpanTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
            : base(storeType, dbType)
        {
        }
    }
}
