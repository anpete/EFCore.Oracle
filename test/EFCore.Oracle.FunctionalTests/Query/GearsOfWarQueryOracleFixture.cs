// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryOracleFixture : GearsOfWarQueryRelationalFixture<OracleTestStore>
    {
        public const string DatabaseName = "GearsOfWarQueryTest";

        private readonly DbContextOptions _options;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public GearsOfWarQueryOracleFixture()
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
                    using (var context = new GearsOfWarContext(
                        new DbContextOptionsBuilder(_options)
                            .UseOracle(OracleTestStore.CreateConnectionString(DatabaseName),
                                b => b.ApplyConfiguration())
                            .Options))
                    {
                        //context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        GearsOfWarModelInitializer.Seed(context);
                    }
                });
        }

        public override GearsOfWarContext CreateContext(OracleTestStore testStore)
        {
            var context = new GearsOfWarContext(
                new DbContextOptionsBuilder(_options)
                    .UseOracle(testStore.Connection, b => b.ApplyConfiguration())
                    .Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
