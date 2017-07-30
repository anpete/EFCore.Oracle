// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsOwnedQueryOracleFixture
        : ComplexNavigationsOwnedQueryRelationalFixtureBase<OracleTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigationsOwned";

        private readonly DbContextOptions _options;
        private readonly string _connectionString = OracleTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public ComplexNavigationsOwnedQueryOracleFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseOracle(_connectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(serviceProvider).Options;
        }

        public override OracleTestStore CreateTestStore()
        {
            return OracleTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new ComplexNavigationsContext(_options))
                    {
                        context.Database.EnsureCreated();
                        ComplexNavigationsModelInitializer.Seed(context, tableSplitting: true);
                    }
                });
        }

        public override ComplexNavigationsContext CreateContext(OracleTestStore testStore)
        {
            var context = new ComplexNavigationsContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
