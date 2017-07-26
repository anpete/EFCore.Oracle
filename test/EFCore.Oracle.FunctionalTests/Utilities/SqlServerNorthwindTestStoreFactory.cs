// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class OracleNorthwindTestStoreFactory : OracleTestStoreFactory
    {
        public new static OracleNorthwindTestStoreFactory Instance { get; } = new OracleNorthwindTestStoreFactory();

        protected OracleNorthwindTestStoreFactory()
        {
        }

        public override OracleTestStore CreateShared(string storeName)
            => OracleTestStore.GetOrCreateShared("Northwind",
                Path.Combine(Path.GetDirectoryName(typeof(OracleTestStore).GetTypeInfo().Assembly.Location), "Northwind.sql"));
    }
}
