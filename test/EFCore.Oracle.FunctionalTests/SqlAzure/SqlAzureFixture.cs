// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    public class SqlAzureFixture
    {
        protected DbContextOptions Options { get; }
        protected IServiceProvider Services { get; }

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public SqlAzureFixture()
        {
            SqlServerTestStore.GetOrCreateShared(
                "adventureworks",
                () => SqlServerTestStore.ExecuteScript(
                    "adventureworks",
                    Path.Combine(
                        Path.GetDirectoryName(typeof(SqlAzureFixture).GetTypeInfo().Assembly.Location),
                        "SqlAzure",
                        "adventureworks.sql")),
                cleanDatabase: false);

            Services = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory).BuildServiceProvider();

            Options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(Services)
                .EnableSensitiveDataLogging()
                .UseSqlServer(SqlServerTestStore.CreateConnectionString("adventureworks"), b => b.ApplyConfiguration()).Options;
        }

        public virtual AdventureWorksContext CreateContext() => new AdventureWorksContext(Options);
    }
}
