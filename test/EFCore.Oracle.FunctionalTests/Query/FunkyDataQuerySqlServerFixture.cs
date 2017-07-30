// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQueryOracleFixture : FunkyDataQueryFixtureBase<OracleTestStore>
    {
        public const string DatabaseName = "FunkyDataQueryTest";

        private readonly DbContextOptions _options;

        private readonly string _connectionString = OracleTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public FunkyDataQueryOracleFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override OracleTestStore CreateTestStore()
        {
            return OracleTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder(_options)
                        .UseOracle(_connectionString, b => b.ApplyConfiguration());

                    using (var context = new FunkyDataContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        FunkyDataModelInitializer.Seed(context);
                    }
                });
        }

        public override FunkyDataContext CreateContext(OracleTestStore testStore)
        {
            var options = new DbContextOptionsBuilder(_options)
                .UseOracle(testStore.Connection, b => b.ApplyConfiguration())
                .Options;

            var context = new FunkyDataContext(options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
