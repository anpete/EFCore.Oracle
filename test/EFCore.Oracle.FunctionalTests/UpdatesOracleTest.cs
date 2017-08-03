// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesOracleTest : UpdatesRelationalTestBase<UpdatesOracleFixture, OracleTestStore>
    {
        public UpdatesOracleTest(UpdatesOracleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Test()
        {
            using (var connection = new OracleConnection("USER ID=PartialUpdateOracleTest;PASSWORD=PartialUpdateOracleTest;DATA SOURCE=//localhost:1521/ef.redmond.corp.microsoft.com"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText 
                        = @"DECLARE
                                rowcount INTEGER;
                            BEGIN

                                rowcount := 23;
                                OPEN :cur FOR SELECT rowcount FROM DUAL;
                            END;";

                    command.Parameters.Add(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output);
                    
                    var reader = command.ExecuteReader();

                    reader.Read();

                    var foo = reader.GetValue(0);

                }
            }
        }

        [Fact]
        public override void Save_partial_update()
        {
            base.Save_partial_update();
        }
    }
}
