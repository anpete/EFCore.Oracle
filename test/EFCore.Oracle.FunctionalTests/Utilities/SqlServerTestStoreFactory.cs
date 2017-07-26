// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class OracleTestStoreFactory : ITestStoreFactory<OracleTestStore>
    {
        public static OracleTestStoreFactory Instance { get; } = new OracleTestStoreFactory();

        protected OracleTestStoreFactory()
        {
        }

        public virtual OracleTestStore CreateShared(string storeName)
            => OracleTestStore.GetOrCreateShared(storeName);
    }
}
