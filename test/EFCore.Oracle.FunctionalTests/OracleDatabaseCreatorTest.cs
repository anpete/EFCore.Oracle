// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    // Tests are split into classes to enable parallel execution
    public class OracleDatabaseCreatorExistsTest
    {
        [Fact]
        public Task Returns_false_when_database_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: false, file: false);
        }

        [Fact]
        public Task Returns_false_when_database_with_filename_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: false, file: true);
        }

        [Fact]
        public Task Async_returns_false_when_database_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: true, file: false);
        }

        [Fact]
        public Task Async_returns_false_when_database_with_filename_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: true, file: true);
        }

        private static async Task Returns_false_when_database_does_not_exist_test(bool async, bool file)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public Task Returns_true_when_database_exists()
        {
            return Returns_true_when_database_exists_test(async: false, file: false);
        }

        [Fact]
        public Task Returns_true_when_database_with_filename_exists()
        {
            return Returns_true_when_database_exists_test(async: false, file: true);
        }

        [Fact]
        public Task Async_returns_true_when_database_exists()
        {
            return Returns_true_when_database_exists_test(async: true, file: false);
        }

        [Fact]
        public Task Async_returns_true_when_database_with_filename_exists()
        {
            return Returns_true_when_database_exists_test(async: true, file: true);
        }

        private static async Task Returns_true_when_database_exists_test(bool async, bool file)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }
    }

    public class OracleDatabaseCreatorEnsureDeletedTest
    {
        [Fact]
        public Task Deletes_database()
        {
            return Delete_database_test(async: false, open: false, file: false);
        }

        [Fact]
        public Task Deletes_database_with_filename()
        {
            return Delete_database_test(async: false, open: false, file: true);
        }

        [Fact]
        public Task Async_deletes_database()
        {
            return Delete_database_test(async: true, open: false, file: false);
        }

        [Fact]
        public Task Async_deletes_database_with_filename()
        {
            return Delete_database_test(async: true, open: false, file: true);
        }

        [Fact]
        public Task Deletes_database_with_opened_connections()
        {
            return Delete_database_test(async: false, open: true, file: false);
        }

        [Fact]
        public Task Deletes_database_with_filename_with_opened_connections()
        {
            return Delete_database_test(async: false, open: true, file: true);
        }

        [Fact]
        public Task Async_deletes_database_with_opened_connections()
        {
            return Delete_database_test(async: true, open: true, file: false);
        }

        [Fact]
        public Task Async_deletes_database_with_filename_with_opened_connections()
        {
            return Delete_database_test(async: true, open: true, file: true);
        }

        private static async Task Delete_database_test(bool async, bool open, bool file)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                if (!open)
                {
                    testDatabase.Connection.Close();
                }

                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.True(await context.Database.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.True(context.Database.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public Task Noop_when_database_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: false, file: false);
        }

        [Fact]
        public Task Noop_when_database_with_filename_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: false, file: true);
        }

        [Fact]
        public Task Async_is_noop_when_database_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: true, file: false);
        }

        [Fact]
        public Task Async_is_noop_when_database_with_filename_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: true, file: true);
        }

        private static async Task Noop_when_database_does_not_exist_test(bool async, bool file)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.False(await creator.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.False(creator.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }
    }

    public class OracleDatabaseCreatorEnsureCreatedTest
    {
        [Fact]
        public Task Creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: false, file: false);
        }

        [Fact]
        public Task Creates_schema_in_existing_database_with_filename()
        {
            return Creates_schema_in_existing_database_test(async: false, file: true);
        }

        [Fact]
        public Task Async_creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: true, file: false);
        }

        [Fact]
        public Task Async_creates_schema_in_existing_database_with_filename()
        {
            return Creates_schema_in_existing_database_test(async: true, file: true);
        }

        private static Task Creates_schema_in_existing_database_test(bool async, bool file)
            => Creates_physical_database_and_schema_test((true, async, file));

        [Fact]
        public Task Creates_physical_database_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: false, file: false);
        }

        [Fact]
        public Task Creates_physical_database_with_filename_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: false, file: true);
        }

        [Fact]
        public Task Async_creates_physical_database_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: true, file: false);
        }

        [Fact]
        public Task Async_creates_physical_database_with_filename_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: true, file: true);
        }

        private static Task Creates_new_physical_database_and_schema_test(bool async, bool file)
            => Creates_physical_database_and_schema_test((false, async, file));

        private static async Task Creates_physical_database_and_schema_test(
            (bool CreateDatabase, bool Async, bool File) options)
        {
            (bool createDatabase, bool async, bool file) = options;
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase, file))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    if (async)
                    {
                        Assert.True(await creator.EnsureCreatedAsync());
                    }
                    else
                    {
                        Assert.True(creator.EnsureCreated());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    if (testDatabase.Connection.State != ConnectionState.Open)
                    {
                        await testDatabase.Connection.OpenAsync();
                    }

                    var tables = (await testDatabase.QueryAsync<string>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).ToList();
                    Assert.Equal(1, tables.Count);
                    Assert.Equal("Blogs", tables.Single());

                    var columns = (await testDatabase.QueryAsync<string>(
                        "SELECT TABLE_NAME + '.' + COLUMN_NAME + ' (' + DATA_TYPE + ')' FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = 'Blogs' ORDER BY TABLE_NAME, COLUMN_NAME")).ToArray();
                    Assert.Equal(14, columns.Length);

                    Assert.Equal(
                        new[]
                        {
                            "Blogs.AndChew (varbinary)",
                            "Blogs.AndRow (timestamp)",
                            "Blogs.Cheese (nvarchar)",
                            "Blogs.ErMilan (int)",
                            "Blogs.Fuse (smallint)",
                            "Blogs.George (bit)",
                            "Blogs.Key1 (nvarchar)",
                            "Blogs.Key2 (varbinary)",
                            "Blogs.NotFigTime (datetime2)",
                            "Blogs.On (real)",
                            "Blogs.OrNothing (float)",
                            "Blogs.TheGu (uniqueidentifier)",
                            "Blogs.ToEat (tinyint)",
                            "Blogs.WayRound (bigint)"
                        },
                        columns);
                }
            }
        }

        [Fact]
        public Task Noop_when_database_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: false, file: false);
        }

        [Fact]
        public Task Noop_when_database_with_filename_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: false, file: true);
        }

        [Fact]
        public Task Async_is_noop_when_database_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: true, file: false);
        }

        [Fact]
        public Task Async_is_noop_when_database_with_filename_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: true, file: true);
        }

        private static async Task Noop_when_database_exists_and_has_schema_test(bool async, bool file)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    context.Database.EnsureCreated();

                    if (async)
                    {
                        Assert.False(await context.Database.EnsureCreatedAsync());
                    }
                    else
                    {
                        Assert.False(context.Database.EnsureCreated());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }
    }

    public class OracleDatabaseCreatorHasTablesTest
    {
        [Fact]
        public Task Throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: false);
        }

        [Fact]
        public Task Async_throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false))
            {
                var databaseCreator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                await databaseCreator.ExecutionStrategyFactory.Create().ExecuteAsync(
                    databaseCreator,
                    async creator =>
                        {
                            var errorNumber = async
                                ? (await Assert.ThrowsAsync<OracleException>(() => creator.HasTablesAsyncBase())).Number
                                : Assert.Throws<OracleException>(() => creator.HasTablesBase()).Number;

                            if (errorNumber != 233) // skip if no-process transient failure
                            {
                                Assert.Equal(
                                    4060, // Login failed error number
                                    errorNumber);
                            }
                        });
            }
        }

        [Fact]
        public Task Returns_false_when_database_exists_but_has_no_tables()
        {
            return Returns_false_when_database_exists_but_has_no_tables_test(async: false);
        }

        [Fact]
        public Task Async_returns_false_when_database_exists_but_has_no_tables()
        {
            return Returns_false_when_database_exists_but_has_no_tables_test(async: true);
        }

        private static async Task Returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch())
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                Assert.False(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
            }
        }

        [Fact]
        public Task Returns_true_when_database_exists_and_has_any_tables()
        {
            return Returns_true_when_database_exists_and_has_any_tables_test(async: false);
        }

        [Fact]
        public Task Async_returns_true_when_database_exists_and_has_any_tables()
        {
            return Returns_true_when_database_exists_and_has_any_tables_test(async: true);
        }

        private static async Task Returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch())
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE SomeTable (Id uniqueidentifier)");

                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                Assert.True(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
            }
        }
    }

    public class OracleDatabaseCreatorDeleteTest
    {
        [Fact]
        public async Task Deletes_database()
        {
            await Deletes_database_test(async: false);
        }

        [Fact]
        public async Task Async_deletes_database()
        {
            await Deletes_database_test(async: true);
        }

        private static async Task Deletes_database_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch())
            {
                testDatabase.Connection.Close();

                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public Task Throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: false);
        }

        [Fact]
        public Task Async_throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await Assert.ThrowsAsync<OracleException>(() => creator.DeleteAsync());
                }
                else
                {
                    Assert.Throws<OracleException>(() => creator.Delete());
                }
            }
        }

        [Fact]
        public void Throws_when_no_user_id()
        {
            var connectionStringBuilder = new OracleConnectionStringBuilder(TestEnvironment.DefaultConnection);
            connectionStringBuilder.Remove("User Id");

            var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(connectionStringBuilder.ToString());

            var ex = Assert.Throws<InvalidOperationException>(() => creator.Delete());

            Assert.Equal(OracleStrings.NoUserId, ex.Message);
        }
    }

    public class OracleDatabaseCreatorCreateTablesTest
    {
        [Fact]
        public Task Creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public Task Async_creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: true);
        }

        private static async Task Creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch())
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    if (async)
                    {
                        await creator.CreateTablesAsync();
                    }
                    else
                    {
                        creator.CreateTables();
                    }

                    if (testDatabase.Connection.State != ConnectionState.Open)
                    {
                        await testDatabase.Connection.OpenAsync();
                    }

                    var tables = (await testDatabase.QueryAsync<string>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).ToList();
                    Assert.Equal(1, tables.Count);
                    Assert.Equal("Blogs", tables.Single());

                    var columns = (await testDatabase.QueryAsync<string>(
                        "SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs'")).ToList();
                    Assert.Equal(14, columns.Count);
                    Assert.True(columns.Any(c => c == "Blogs.Key1"));
                    Assert.True(columns.Any(c => c == "Blogs.Key2"));
                    Assert.True(columns.Any(c => c == "Blogs.Cheese"));
                    Assert.True(columns.Any(c => c == "Blogs.ErMilan"));
                    Assert.True(columns.Any(c => c == "Blogs.George"));
                    Assert.True(columns.Any(c => c == "Blogs.TheGu"));
                    Assert.True(columns.Any(c => c == "Blogs.NotFigTime"));
                    Assert.True(columns.Any(c => c == "Blogs.ToEat"));
                    Assert.True(columns.Any(c => c == "Blogs.OrNothing"));
                    Assert.True(columns.Any(c => c == "Blogs.Fuse"));
                    Assert.True(columns.Any(c => c == "Blogs.WayRound"));
                    Assert.True(columns.Any(c => c == "Blogs.On"));
                    Assert.True(columns.Any(c => c == "Blogs.AndChew"));
                    Assert.True(columns.Any(c => c == "Blogs.AndRow"));
                }
            }
        }

        [Fact]
        public Task Throws_if_database_does_not_exist()
        {
            return Throws_if_database_does_not_exist_test(async: false);
        }

        [Fact]
        public Task Async_throws_if_database_does_not_exist()
        {
            return Throws_if_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_if_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                var errorNumber
                    = async
                        ? (await Assert.ThrowsAsync<OracleException>(() => creator.CreateTablesAsync())).Number
                        : Assert.Throws<OracleException>(() => creator.CreateTables()).Number;

                if (errorNumber != 233) // skip if no-process transient failure
                {
                    Assert.Equal(
                        4060, // Login failed error number
                        errorNumber);
                }
            }
        }
    }

    public class OracleDatabaseCreatorCreateTest
    {
        [Fact]
        public Task Creates_physical_database_but_not_tables()
        {
            return Creates_physical_database_but_not_tables_test(async: false);
        }

        [Fact]
        public Task Async_creates_physical_database_but_not_tables()
        {
            return Creates_physical_database_but_not_tables_test(async: true);
        }

        private static async Task Creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch(createDatabase: false))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                Assert.False(creator.Exists());

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                Assert.Equal(
                    0, (await testDatabase.QueryAsync<string>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).Count());

                Assert.True(
                    await testDatabase.ExecuteScalarAsync<bool>(
                        string.Concat(
                            "SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name='",
                            testDatabase.Connection.Database,
                            "'")));
            }
        }

        [Fact]
        public Task Throws_if_database_already_exists()
        {
            return Throws_if_database_already_exists_test(async: false);
        }

        [Fact]
        public Task Async_throws_if_database_already_exists()
        {
            return Throws_if_database_already_exists_test(async: true);
        }

        private static async Task Throws_if_database_already_exists_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateScratch())
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<OracleException>(() => creator.CreateAsync())
                    : Assert.Throws<OracleException>(() => creator.Create());
                Assert.Equal(
                    1801, // Database with given name already exists
                    ex.Number);
            }
        }
    }

    public class OracleDatabaseCreatorTest
    {
        public static TestDatabaseCreator GetDatabaseCreator(OracleTestStore testStore)
            => GetDatabaseCreator(testStore.ConnectionString);

        public static TestDatabaseCreator GetDatabaseCreator(string connectionString)
            => GetDatabaseCreator(new BloggingContext(connectionString));

        public static TestDatabaseCreator GetDatabaseCreator(BloggingContext context)
            => (TestDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();

        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
                .BuildServiceProvider();

        public class BloggingContext : DbContext
        {
            private readonly string _connectionString;

            public BloggingContext(OracleTestStore testStore)
                : this(testStore.ConnectionString)
            {
            }

            public BloggingContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseOracle(_connectionString, b => b.ApplyConfiguration().CommandTimeout(600))
                    .UseInternalServiceProvider(CreateServiceProvider());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(
                    b =>
                        {
                            b.HasKey(e => new { e.Key1, e.Key2 });
                            b.Property(e => e.AndRow).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                        });
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public string Key1 { get; set; }
            public byte[] Key2 { get; set; }
            public string Cheese { get; set; }
            public int ErMilan { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public float On { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }

        public class TestDatabaseCreator : OracleDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                IOracleConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default(CancellationToken))
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;
        }
    }
}
