// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using Oracle.ManagedDataAccess.Client;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlSprocQueryOracleTest : FromSqlSprocQueryTestBase<NorthwindSprocQueryOracleFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FromSqlSprocQueryOracleTest(
            NorthwindSprocQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            _testOutputHelper = testOutputHelper;
            fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void From_sql_queryable_stored_procedure()
        {
            base.From_sql_queryable_stored_procedure();

//
//            using (var connection = new OracleConnection(Fixture.TestStore.ConnectionString))
//            {
//                connection.Open();
//
//                using (var command = connection.CreateCommand())
//                {
//                    //command.CommandText = TenMostExpensiveProductsSproc;
//                    command.CommandText = TenMostExpensiveProductsSproc2;
//                    command.BindByName = true;
//                    //command.CommandType = CommandType.StoredProcedure;
//                    
//                    var oracleParameter = new OracleParameter
//                    {
//                        ParameterName = "cur",
//                        OracleDbType = OracleDbType.RefCursor,
//                        Direction = ParameterDirection.Output
//                    };
//
//                    command.Parameters.Add(oracleParameter);
//                    
//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            _testOutputHelper.WriteLine(reader.GetValue(0).ToString());
//                        }
//                    }
//                    
//                    
//
//                }
//
//            }

        }

        protected override string TenMostExpensiveProductsSproc 
            => "BEGIN \"Ten Most Expensive Products\"(:cur); END;";
        
        protected override string CustomerOrderHistorySproc => "BEGIN \"CustOrderHist](:CustomerID)\"; END;";
    }
}
