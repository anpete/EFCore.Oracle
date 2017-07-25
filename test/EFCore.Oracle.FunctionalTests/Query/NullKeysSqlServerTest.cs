// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysSqlServerTest : NullKeysTestBase<NullKeysSqlServerTest.NullKeysSqlServerFixture>
    {
        public NullKeysSqlServerTest(NullKeysSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysSqlServerFixture : NullKeysFixtureBase, IDisposable
        {
            private readonly DbContextOptions _options;
            private readonly SqlServerTestStore _testStore;

            public NullKeysSqlServerFixture()
            {
                var name = "StringsContext";
                var connectionString = SqlServerTestStore.CreateConnectionString(name);

                _options = new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider(validateScopes: true))
                    .Options;

                _testStore = SqlServerTestStore.GetOrCreateShared(name, EnsureCreated);
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public void Dispose() => _testStore.Dispose();
        }
    }
}
