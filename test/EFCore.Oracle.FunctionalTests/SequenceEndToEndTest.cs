// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.SupportsSequences)]
    public class SequenceEndToEndTest : IDisposable
    {
        [ConditionalFact] // TODO: See issue#7160
        [PlatformSkipCondition(TestPlatform.Linux, SkipReason = "Connection timeout error on Linux.")]
        public void Can_use_sequence_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreated();
            }

            AddEntities(serviceProvider, TestStore.Name);
            AddEntities(serviceProvider, TestStore.Name);

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            AddEntities(serviceProvider, TestStore.Name);

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                var pegasuses = context.Pegasuses.ToList();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private static void AddEntities(IServiceProvider serviceProvider, string name)
        {
            using (var context = new BronieContext(serviceProvider, name))
            {
                for (var i = 0; i < 10; i++)
                {
                    context.Add(new Pegasus { Name = "Rainbow Dash " + i });
                    context.Add(new Pegasus { Name = "Fluttershy " + i });
                }

                context.SaveChanges();
            }
        }

        [ConditionalFact]
        public async Task Can_use_sequence_end_to_end_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreated();
            }

            await AddEntitiesAsync(serviceProvider, TestStore.Name);
            await AddEntitiesAsync(serviceProvider, TestStore.Name);

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            await AddEntitiesAsync(serviceProvider, TestStore.Name);

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private static async Task AddEntitiesAsync(IServiceProvider serviceProvider, string databaseName)
        {
            using (var context = new BronieContext(serviceProvider, databaseName))
            {
                for (var i = 0; i < 10; i++)
                {
                    await context.AddAsync(new Pegasus { Name = "Rainbow Dash " + i });
                    await context.AddAsync(new Pegasus { Name = "Fluttershy " + i });
                }

                await context.SaveChangesAsync();
            }
        }

        [ConditionalFact] // TODO: See issue#7160
        [PlatformSkipCondition(TestPlatform.Linux, SkipReason = "Connection timeout error on Linux.")]
        public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreated();
            }

            const int threadCount = 50;

            var tests = new Func<Task>[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                var closureProvider = serviceProvider;
                tests[i] = () => AddEntitiesAsync(closureProvider, TestStore.Name);
            }

            var tasks = tests.Select(Task.Run).ToArray();

            foreach (var t in tasks)
            {
                await t;
            }

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        [ConditionalFact]
        public void Can_use_explicit_values()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreated();
            }

            AddEntitiesWithIds(serviceProvider, 0, TestStore.Name);
            AddEntitiesWithIds(serviceProvider, 2, TestStore.Name);

            // Use a different service provider so a different generator is used but with
            // the same server sequence.
            serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            AddEntitiesWithIds(serviceProvider, 4, TestStore.Name);

            using (var context = new BronieContext(serviceProvider, TestStore.Name))
            {
                var pegasuses = context.Pegasuses.ToList();

                for (var i = 1; i < 11; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Fluttershy " + i));

                    for (var j = 0; j < 6; j++)
                    {
                        pegasuses.Single(p => p.Identifier == i * 100 + j);
                    }
                }
            }
        }

        private static void AddEntitiesWithIds(IServiceProvider serviceProvider, int idOffset, string name)
        {
            using (var context = new BronieContext(serviceProvider, name))
            {
                for (var i = 1; i < 11; i++)
                {
                    context.Add(new Pegasus { Name = "Rainbow Dash " + i, Identifier = i * 100 + idOffset });
                    context.Add(new Pegasus { Name = "Fluttershy " + i, Identifier = i * 100 + idOffset + 1 });
                }

                context.SaveChanges();
            }
        }

        private class BronieContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _databaseName;

            public BronieContext(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
            }

            public DbSet<Pegasus> Pegasuses { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Pegasus>(b =>
                    {
                        b.HasKey(e => e.Identifier);
                        b.Property(e => e.Identifier).ForSqlServerUseSequenceHiLo();
                    });
            }
        }

        private class Pegasus
        {
            public int Identifier { get; set; }
            public string Name { get; set; }
        }

        [ConditionalFact] // Issue #478
        public void Can_use_sequence_with_nullable_key_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, true))
            {
                context.Database.EnsureCreated();
            }

            AddEntitiesNullable(serviceProvider, TestStore.Name, true);
            AddEntitiesNullable(serviceProvider, TestStore.Name, true);
            AddEntitiesNullable(serviceProvider, TestStore.Name, true);

            using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, true))
            {
                var pegasuses = context.Unicons.ToList();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Twilight Sparkle " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rarity " + i));
                }
            }
        }

        [ConditionalFact] // Issue #478
        public void Can_use_identity_with_nullable_key_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, false))
            {
                context.Database.EnsureCreated();
            }

            AddEntitiesNullable(serviceProvider, TestStore.Name, false);
            AddEntitiesNullable(serviceProvider, TestStore.Name, false);
            AddEntitiesNullable(serviceProvider, TestStore.Name, false);

            using (var context = new NullableBronieContext(serviceProvider, TestStore.Name, false))
            {
                var pegasuses = context.Unicons.ToList();

                for (var i = 0; i < 10; i++)
                {
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Twilight Sparkle " + i));
                    Assert.Equal(3, pegasuses.Count(p => p.Name == "Rarity " + i));
                }
            }
        }

        private static void AddEntitiesNullable(IServiceProvider serviceProvider, string databaseName, bool useSequence)
        {
            using (var context = new NullableBronieContext(serviceProvider, databaseName, useSequence))
            {
                for (var i = 0; i < 10; i++)
                {
                    context.Add(new Unicon { Name = "Twilight Sparkle " + i });
                    context.Add(new Unicon { Name = "Rarity " + i });
                }

                context.SaveChanges();
            }
        }

        private class NullableBronieContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _databaseName;
            private readonly bool _useSequence;

            public NullableBronieContext(IServiceProvider serviceProvider, string databaseName, bool useSequence)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
                _useSequence = useSequence;
            }

            public DbSet<Unicon> Unicons { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Unicon>(b =>
                    {
                        b.HasKey(e => e.Identifier);
                        if (_useSequence)
                        {
                            b.Property(e => e.Identifier).ForSqlServerUseSequenceHiLo();
                        }
                        else
                        {
                            b.Property(e => e.Identifier).UseSqlServerIdentityColumn();
                        }
                    });
            }
        }

        private class Unicon
        {
            public int? Identifier { get; set; }
            public string Name { get; set; }
        }

        public SequenceEndToEndTest()
        {
            TestStore = SqlServerTestStore.Create("SequenceEndToEndTest");
        }

        protected SqlServerTestStore TestStore { get; }

        public void Dispose() => TestStore.Dispose();
    }
}
