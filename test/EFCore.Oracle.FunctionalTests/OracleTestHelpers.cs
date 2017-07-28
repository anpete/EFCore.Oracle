// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleTestHelpers : TestHelpers
    {
        public const string TestConnectionString
            = "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))"
              + "(CONNECT_DATA=(SID=orcl)));USER ID=sys;PASSWORD=oracle;DBA PRIVILEGE=SYSDBA;";

        protected OracleTestHelpers()
        {
        }

        public static OracleTestHelpers Instance { get; } = new OracleTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkOracle();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseOracle(new OracleConnection(TestConnectionString));
    }
}
