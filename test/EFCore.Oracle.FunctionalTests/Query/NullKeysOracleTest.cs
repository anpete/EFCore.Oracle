// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysOracleTest : NullKeysTestBase<NullKeysOracleTest.NullKeysOracleFixture>
    {
        public NullKeysOracleTest(NullKeysOracleFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysOracleFixture : NullKeysFixtureBase, IDisposable
        {
            private readonly DbContextOptions _options;
            private readonly OracleTestStore _testStore;

            public NullKeysOracleFixture()
            {
                var name = "StringsContext";
                var connectionString = OracleTestStore.CreateConnectionString(name);

                _options = new DbContextOptionsBuilder()
                    .UseOracle(connectionString, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkOracle()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider(validateScopes: true))
                    .Options;

                _testStore = OracleTestStore.GetOrCreateShared(name, EnsureCreated);
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public void Dispose() => _testStore.Dispose();
        }
    }
}
