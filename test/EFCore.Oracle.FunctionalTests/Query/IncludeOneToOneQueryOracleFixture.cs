// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneQueryOracleFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly OracleTestStore _testStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public IncludeOneToOneQueryOracleFixture()
        {
            _testStore = OracleTestStore.Create("OneToOneQueryTest");

            _options = new DbContextOptionsBuilder()
                .UseOracle(_testStore.ConnectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkOracle()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider(validateScopes: true))
                .Options;

            using (var context = new DbContext(_options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public void Debug()
        {
            using (var context = new DbContext(_options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
