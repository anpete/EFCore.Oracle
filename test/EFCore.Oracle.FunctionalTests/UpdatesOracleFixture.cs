// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesOracleFixture : UpdatesFixtureBase<OracleTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdatesOracleFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);
        }

        protected virtual string DatabaseName => "PartialUpdateOracleTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override OracleTestStore CreateTestStore()
            => OracleTestStore.GetOrCreateShared(
                DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseOracle(OracleTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new UpdatesContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureCreated();
                            UpdatesModelInitializer.Seed(context);
                        }
                    });

        public override UpdatesContext CreateContext(OracleTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseOracle(testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

            var context = new UpdatesContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasColumnType("decimal(18,2)");
        }
    }
}
