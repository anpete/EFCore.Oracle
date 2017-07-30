// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQueryOracleFixture : InheritanceRelationshipsQueryRelationalFixture<OracleTestStore>
    {
        public static readonly string DatabaseName = "InheritanceRelationships";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = OracleTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public InheritanceRelationshipsQueryOracleFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);
        }

        public override OracleTestStore CreateTestStore()
        {
            return OracleTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseOracle(_connectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new InheritanceRelationshipsContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        InheritanceRelationshipsModelInitializer.Seed(context);
                    }
                });
        }

        public override InheritanceRelationshipsContext CreateContext(OracleTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseOracle(testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

            var context = new InheritanceRelationshipsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
