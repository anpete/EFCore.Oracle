// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleOptionsExtensionTest
    {
        [Fact]
        public void ApplyServices_adds_SQL_server_services()
        {
            var services = new ServiceCollection();

            new OracleOptionsExtension().ApplyServices(services);

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IOracleConnection)));
        }
    }
}
