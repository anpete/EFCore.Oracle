// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQueryOracleFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly OracleTestStore _testDatabase;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public MappingQueryOracleFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _testDatabase = OracleTestStore.GetNorthwindStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseOracle(_testDatabase.ConnectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public DbContext CreateContext()
        {
            var context = new DbContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testDatabase.Dispose();

        protected override string DatabaseSchema { get; } = null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.Oracle().ColumnName = "CompanyName";
                    e.Metadata.Oracle().TableName = "Customers";
                    e.Metadata.Oracle().Schema = null;
                });
        }
    }
}
