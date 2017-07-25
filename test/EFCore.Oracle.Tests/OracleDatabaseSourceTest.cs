// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleDatabaseSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(OracleRelationalConnection).GetTypeInfo().Assembly.GetName().Name,
                new DatabaseProvider<OracleOptionsExtension>(new DatabaseProviderDependencies()).Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseOracle("Database=Crunchie");

            Assert.True(new DatabaseProvider<OracleOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new DatabaseProvider<OracleOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }
    }
}
