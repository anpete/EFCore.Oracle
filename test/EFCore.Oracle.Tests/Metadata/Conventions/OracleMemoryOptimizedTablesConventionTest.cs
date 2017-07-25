// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class OracleMemoryOptimizedTablesConventionTest
    {
        [Fact]
        public void Keys_and_indexes_are_nonclustered_for_memory_optimized_tables()
        {
            var modelBuilder = OracleTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Order>();

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.Oracle().IsClustered == null));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.Oracle().IsClustered == null));

            modelBuilder.Entity<Order>().ForOracleIsMemoryOptimized();
            modelBuilder.Entity<Order>().HasKey(o => new { o.Id, o.CustomerId });
            modelBuilder.Entity<Order>().HasIndex(o => o.CustomerId);

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.Oracle().IsClustered == false));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.Oracle().IsClustered == false));

            modelBuilder.Entity<Order>().ForOracleIsMemoryOptimized(false);

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.Oracle().IsClustered == null));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.Oracle().IsClustered == null));
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
