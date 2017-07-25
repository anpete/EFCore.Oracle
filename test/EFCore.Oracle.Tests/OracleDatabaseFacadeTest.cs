// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleDatabaseFacadeTest
    {
        [Fact]
        public void IsOracle_when_using_OnConfguring()
        {
            using (var context = new OracleOnConfiguringContext())
            {
                Assert.True(context.Database.IsOracle());
            }
        }

        [Fact]
        public void IsOracle_in_OnModelCreating_when_using_OnConfguring()
        {
            using (var context = new OracleOnModelContext())
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsOracleSet);
            }
        }

        [Fact]
        public void IsOracle_in_constructor_when_using_OnConfguring()
        {
            using (var context = new OracleConstructorContext())
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsOracleSet);
            }
        }

        [Fact]
        public void Cannot_use_IsOracle_in_OnConfguring()
        {
            using (var context = new OracleUseInOnConfiguringContext())
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            {
                                var _ = context.Model; // Trigger context initialization
                            }).Message);
            }
        }

        [Fact]
        public void IsOracle_when_using_constructor()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseOracle("Database=Maltesers").Options))
            {
                Assert.True(context.Database.IsOracle());
            }
        }

        [Fact]
        public void IsOracle_in_OnModelCreating_when_using_constructor()
        {
            using (var context = new ProviderOnModelContext(
                new DbContextOptionsBuilder().UseOracle("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsOracleSet);
            }
        }

        [Fact]
        public void IsOracle_in_constructor_when_using_constructor()
        {
            using (var context = new ProviderConstructorContext(
                new DbContextOptionsBuilder().UseOracle("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsOracleSet);
            }
        }

        [Fact]
        public void Cannot_use_IsOracle_in_OnConfguring_with_constructor()
        {
            using (var context = new ProviderUseInOnConfiguringContext(
                new DbContextOptionsBuilder().UseOracle("Database=Maltesers").Options))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            {
                                var _ = context.Model; // Trigger context initialization
                            }).Message);
            }
        }

        [Fact]
        public void Not_IsOracle_when_using_different_provider()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseInMemoryDatabase("Maltesers").Options))
            {
                Assert.False(context.Database.IsOracle());
            }
        }

        private class ProviderContext : DbContext
        {
            protected ProviderContext()
            {
            }

            public ProviderContext(DbContextOptions options)
                : base(options)
            {
            }

            public bool? IsOracleSet { get; protected set; }
        }

        private class OracleOnConfiguringContext : ProviderContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle("Database=Maltesers");
        }

        private class OracleOnModelContext : OracleOnConfiguringContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsOracleSet = Database.IsOracle();
        }

        private class OracleConstructorContext : OracleOnConfiguringContext
        {
            public OracleConstructorContext()
                => IsOracleSet = Database.IsOracle();
        }

        private class OracleUseInOnConfiguringContext : OracleOnConfiguringContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                IsOracleSet = Database.IsOracle();
            }
        }

        private class ProviderOnModelContext : ProviderContext
        {
            public ProviderOnModelContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsOracleSet = Database.IsOracle();
        }

        private class ProviderConstructorContext : ProviderContext
        {
            public ProviderConstructorContext(DbContextOptions options)
                : base(options)
                => IsOracleSet = Database.IsOracle();
        }

        private class ProviderUseInOnConfiguringContext : ProviderContext
        {
            public ProviderUseInOnConfiguringContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => IsOracleSet = Database.IsOracle();
        }
    }
}
