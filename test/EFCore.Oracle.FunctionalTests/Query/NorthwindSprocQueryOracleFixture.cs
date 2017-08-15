// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSprocQueryOracleFixture : NorthwindSprocQueryRelationalFixture, IDisposable
    {
        private readonly OracleTestStore _testStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public NorthwindSprocQueryOracleFixture() => _testStore = OracleTestStore.GetNorthwindStore();

        public OracleTestStore TestStore => _testStore;

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(
                    (additionalServices ?? new ServiceCollection())
                    .AddEntityFrameworkOracle()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider(validateScopes: true))
                .UseOracle(_testStore.ConnectionString, b => b.ApplyConfiguration())
                .Options;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");
        }

        public void Dispose() => _testStore.Dispose();
    }
}
