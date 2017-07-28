// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class QueryOracleTest : QueryTestBase<NorthwindQueryOracleFixture>
    {
        public QueryOracleTest(NorthwindQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
        
//        [Fact]
//        public void Test()
//        {
//            using (var connection 
//                = new OracleConnection("USER ID=Northwind;PASSWORD=Northwind;DATA SOURCE=//localhost:1521/Northwind.redmond.corp.microsoft.com"))
//            {
//                connection.Open();
//
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText
//                        = @"SELECT ""c"".""CustomerID""
//FROM ""Customers"" ""c""
//ORDER BY ""c"".""CustomerID""
//OFFSET :p_0 ROWS";
//                    command.BindByName = true;
//                    
//                    var parameter = command.CreateParameter();
//                    
//                    parameter.ParameterName = "p_0";
//                    parameter.Value = 5;
//                    parameter.DbType = DbType.Int32;
//                    
//                    command.Parameters.Add(parameter);
//                    
//                    var reader = command.ExecuteReader();
//
//
//                }
//            }
//
//        }

        public override void Shaper_command_caching_when_parameter_names_different()
        {
            base.Shaper_command_caching_when_parameter_names_different();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""e""
WHERE ""e"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT COUNT(*)
FROM ""Customers"" ""e""
WHERE ""e"".""CustomerID"" = N'ALFKI'");
        }

        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();

            AssertSql(
                @":p_0='2'

SELECT ""c1_Orders"".""OrderID"", ""c1_Orders"".""CustomerID"", ""c1_Orders"".""EmployeeID"", ""c1_Orders"".""OrderDate"", ""t0"".""CustomerID""
FROM ""Orders"" ""c1_Orders""
INNER JOIN (
    SELECT DISTINCT ""t"".""CustomerID""
    FROM (
        SELECT ""c"".*
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""CustomerID""
    ) ""t""
    CROSS JOIN ""Customers"" ""c2""
) ""t0"" ON ""c1_Orders"".""CustomerID"" = ""t0"".""CustomerID""");
        }

        public override void Lifting_when_subquery_nested_order_by_simple()
        {
            base.Lifting_when_subquery_nested_order_by_simple();

            // TODO: Avoid unnecessary pushdown of subquery. See Issue#8094
            AssertContainsSql(
                @":p_0='2'

SELECT ""t0"".""CustomerID""
FROM (
    SELECT DISTINCT ""t"".""CustomerID""
    FROM (
        SELECT ""c"".*
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""CustomerID""
    ) ""t""
    CROSS JOIN ""Customers"" ""c2""
) ""t0""",
                //
                @"SELECT ""c1_Orders"".""OrderID"", ""c1_Orders"".""CustomerID"", ""c1_Orders"".""EmployeeID"", ""c1_Orders"".""OrderDate""
FROM ""Orders"" ""c1_Orders""");
        }

        [Fact]
        public virtual void Cache_key_contexts_are_detached()
        {
            var weakRef = Scoper(
                () =>
                    {
                        var context = CreateContext();

                        var wr = new WeakReference(context);

                        using (context)
                        {
                            var orderDetails = context.OrderDetails;

                            Func<NorthwindContext, Customer> query
                                = param
                                    => (from c in context.Customers
                                        from o in context.Set<Order>()
                                        from od in orderDetails
                                        from e1 in param.Employees
                                        from e2 in param.Set<Order>()
                                        select c).First();

                            query(context);

                            Assert.True(wr.IsAlive);

                            return wr;
                        }
                    });

            GC.Collect();

            Assert.False(weakRef.IsAlive);
        }

        private static T Scoper<T>(Func<T> getter)
        {
            return getter();
        }

        public override void Local_array()
        {
            base.Local_array();

            AssertSql(
                @":get_Item_0='ALFKI' (Size = 5)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = :get_Item_0");
        }

        public override void Entity_equality_self()
        {
            base.Entity_equality_self();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = ""c"".""CustomerID""");
        }

        public override void Entity_equality_local()
        {
            base.Entity_equality_local();

            AssertSql(
                @":local_0_CustomerID='ANATR' (Nullable = false) (Size = 5)

SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = :local_0_CustomerID");
        }

        public override void Entity_equality_local_inline()
        {
            base.Entity_equality_local_inline();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = N'ANATR'");
        }

        public override void Entity_equality_null()
        {
            base.Entity_equality_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NULL");
        }

        public override void Entity_equality_not_null()
        {
            base.Entity_equality_not_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NOT NULL");
        }

        public override void Queryable_reprojection()
        {
            base.Queryable_reprojection();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void Default_if_empty_top_level()
        {
            base.Default_if_empty_top_level();

            AssertSql(
                @"SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT NULL ""empty"" FROM DUAL
) ""empty""
LEFT JOIN (
    SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
    FROM ""Employees"" ""c""
    WHERE ""c"".""EmployeeID"" = -1
) ""t"" ON 1 = 1");
        }

        public override void Default_if_empty_top_level_positive()
        {
            base.Default_if_empty_top_level_positive();

            AssertSql(
                @"SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT NULL ""empty"" FROM DUAL
) ""empty""
LEFT JOIN (
    SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
    FROM ""Employees"" ""c""
    WHERE ""c"".""EmployeeID"" > 0
) ""t"" ON 1 = 1");
        }

        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();

            AssertSql(
                @"SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
FROM ""Employees"" ""c""
WHERE ""c"".""EmployeeID"" = -1");
        }

        public override void Default_if_empty_top_level_projection()
        {
            base.Default_if_empty_top_level_projection();

            AssertSql(
                @"SELECT ""t"".""EmployeeID""
FROM (
    SELECT NULL ""empty""
) ""empty""
LEFT JOIN (
    SELECT ""e"".""EmployeeID""
    FROM ""Employees"" ""e""
    WHERE ""e"".""EmployeeID"" = -1
) ""t"" ON 1 = 1");
        }

        public override void Where_query_composition()
        {
            base.Where_query_composition();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE ""e1"".""FirstName"" = (
    SELECT ""e"".""FirstName""
    FROM ""Employees"" ""e""
    ORDER BY ""e"".""EmployeeID""
)");
        }

        public override void Where_query_composition_is_null()
        {
            base.Where_query_composition_is_null();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" = :outer_ReportsTo",
                //
                @"SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" IS NULL",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" = :outer_ReportsTo");
        }

        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_null();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" = :outer_ReportsTo",
                //
                @"SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" IS NULL",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e2""
WHERE ""e2"".""EmployeeID"" = :outer_ReportsTo");
        }

        public override void Where_query_composition_entity_equality_one_element_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_SingleOrDefault();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" = :outer_ReportsTo",
                //
                @"SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" IS NULL",
                //
                @":outer_ReportsTo='2' (Nullable = true)

SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" = :outer_ReportsTo");
        }

        public override void Where_query_composition_entity_equality_one_element_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_FirstOrDefault();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE (
    SELECT ""e2"".""EmployeeID""
    FROM ""Employees"" ""e2""
    WHERE ""e2"".""EmployeeID"" = ""e1"".""ReportsTo""
) = 0");
        }

        public override void Where_query_composition_entity_equality_no_elements_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_SingleOrDefault();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @"SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" = 42",
                //
                @"SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" = 42",
                //
                @"SELECT ""e20"".""EmployeeID""
FROM ""Employees"" ""e20""
WHERE ""e20"".""EmployeeID"" = 42");
        }

        public override void Where_query_composition_entity_equality_no_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_FirstOrDefault();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE (
    SELECT ""e2"".""EmployeeID""
    FROM ""Employees"" ""e2""
    WHERE ""e2"".""EmployeeID"" = 42
) = 0");
        }

        public override void Where_query_composition_entity_equality_multiple_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE (
    SELECT ""e2"".""EmployeeID""
    FROM ""Employees"" ""e2""
    WHERE (""e2"".""EmployeeID"" <> ""e1"".""ReportsTo"") OR ""e1"".""ReportsTo"" IS NULL
) = 0");
        }

        public override void Where_query_composition2()
        {
            base.Where_query_composition2();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""");
        }

        public override void Where_query_composition2_FirstOrDefault()
        {
            base.Where_query_composition2_FirstOrDefault();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""
WHERE ""t"".""FirstName"" = (
    SELECT ""e0"".""FirstName""
    FROM ""Employees"" ""e0""
    ORDER BY ""e0"".""EmployeeID""
)");
        }

        public override void Where_query_composition2_FirstOrDefault_with_anonymous()
        {
            base.Where_query_composition2_FirstOrDefault_with_anonymous();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""",
                //
                @"SELECT ""e0"".""EmployeeID"", ""e0"".""City"", ""e0"".""Country"", ""e0"".""FirstName"", ""e0"".""ReportsTo"", ""e0"".""Title""
FROM ""Employees"" ""e0""
ORDER BY ""e0"".""EmployeeID""");
        }

        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            AssertSql(
                @":p_0='2'

SELECT ""od"".""OrderID""
FROM ""Order Details"" ""od""
ORDER BY ""od"".""ProductID"", ""od"".""OrderID""",
                //
                @":outer_OrderID='10285'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE :outer_OrderID = ""o"".""OrderID""
ORDER BY ""o"".""OrderID""",
                //
                @":outer_OrderID='10294'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE :outer_OrderID = ""o"".""OrderID""
ORDER BY ""o"".""OrderID""");
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            AssertSql(
                @"SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" ""od""
WHERE ""od"".""OrderID"" = 10344",
                //
                @":outer_OrderID='10344'

SELECT ""o0"".""CustomerID""
FROM ""Orders"" ""o0""
WHERE :outer_OrderID = ""o0"".""OrderID""",
                //
                @":outer_CustomerID1='WHITC' (Size = 5)

SELECT ""c2"".""City""
FROM ""Customers"" ""c2""
WHERE :outer_CustomerID1 = ""c2"".""CustomerID""",
                //
                @":outer_OrderID='10344'

SELECT ""o0"".""CustomerID""
FROM ""Orders"" ""o0""
WHERE :outer_OrderID = ""o0"".""OrderID""",
                //
                @":outer_CustomerID1='WHITC' (Size = 5)

SELECT ""c2"".""City""
FROM ""Customers"" ""c2""
WHERE :outer_CustomerID1 = ""c2"".""CustomerID""");
        }

        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            AssertSql(
                @":p_0='2'

SELECT ""od"".""OrderID"", ""od"".""ProductID"", ""od"".""Discount"", ""od"".""Quantity"", ""od"".""UnitPrice""
FROM ""Order Details"" ""od""
WHERE (
    SELECT (
        SELECT ""c"".""City""
        FROM ""Customers"" ""c""
        WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
    )
    FROM ""Orders"" ""o""
    WHERE ""od"".""OrderID"" = ""o"".""OrderID""
) = N'Seattle'");
        }

        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            AssertSql(
                @":p_0='1'

SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate""
FROM (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
) ""t""
ORDER BY ""t"".""OrderID""",
                //
                @"SELECT ""t1"".""OrderID""
FROM (
    SELECT ""od0"".*
    FROM ""Order Details"" ""od0""
    ORDER BY ""od0"".""OrderID""
) ""t1""",
                //
                @":outer_CustomerID2='VINET' (Size = 5)

SELECT ""c3"".""Country""
FROM ""Customers"" ""c3""
WHERE ""c3"".""CustomerID"" = :outer_CustomerID2
ORDER BY ""c3"".""CustomerID""",
                //
                @":outer_OrderID1='10248'

SELECT ""c4"".""Country""
FROM ""Orders"" ""o20""
INNER JOIN ""Customers"" ""c4"" ON ""o20"".""CustomerID"" = ""c4"".""CustomerID""
WHERE ""o20"".""OrderID"" = :outer_OrderID1
ORDER BY ""o20"".""OrderID"", ""c4"".""CustomerID""",
                //
                @":outer_CustomerID2='VINET' (Size = 5)

SELECT ""c3"".""Country""
FROM ""Customers"" ""c3""
WHERE ""c3"".""CustomerID"" = :outer_CustomerID2
ORDER BY ""c3"".""CustomerID""",
                //
                @":outer_OrderID1='10248'

SELECT ""c4"".""Country""
FROM ""Orders"" ""o20""
INNER JOIN ""Customers"" ""c4"" ON ""o20"".""CustomerID"" = ""c4"".""CustomerID""
WHERE ""o20"".""OrderID"" = :outer_OrderID1
ORDER BY ""o20"".""OrderID"", ""c4"".""CustomerID""");
        }

        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""
CROSS JOIN (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
) ""t0""");
        }

        public override void Where_subquery_anon_nested()
        {
            base.Where_subquery_anon_nested();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""t1"".""CustomerID"", ""t1"".""Address"", ""t1"".""City"", ""t1"".""CompanyName"", ""t1"".""ContactName"", ""t1"".""ContactTitle"", ""t1"".""Country"", ""t1"".""Fax"", ""t1"".""Phone"", ""t1"".""PostalCode"", ""t1"".""Region""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""
CROSS JOIN (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
) ""t0""
CROSS JOIN (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
) ""t1""
WHERE ""t"".""City"" = N'London'");
        }

        public override void OrderBy_SelectMany()
        {
            base.OrderBy_SelectMany();

            AssertSql(
                @"SELECT ""c"".""ContactName"", ""t"".""OrderID""
FROM ""Customers"" ""c""
CROSS JOIN (
    SELECT ""o"".*
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""
WHERE ""c"".""CustomerID"" = ""t"".""CustomerID""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Let_any_subquery_anonymous()
        {
            base.Let_any_subquery_anonymous();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID""",
                //
                @":outer_CustomerID='ALFKI' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='ANATR' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='ANTON' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='AROUT' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void GroupBy_anonymous()
        {
            base.GroupBy_anonymous();

            AssertSql(
                @"SELECT ""c"".""City"", ""c"".""CustomerID""
FROM ""Customers"" ""c""
ORDER BY ""c"".""City""");
        }

        public override void GroupBy_anonymous_with_where()
        {
            base.GroupBy_anonymous_with_where();

            AssertSql(
                @"SELECT ""c"".""City"", ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""Country"" IN (N'Argentina', N'Austria', N'Brazil', N'France', N'Germany', N'USA')
ORDER BY ""c"".""City""");
        }

        public override void GroupBy_nested_order_by_enumerable()
        {
            base.GroupBy_nested_order_by_enumerable();

            AssertSql(
                @"SELECT ""c0"".""Country"", ""c0"".""CustomerID""
FROM ""Customers"" ""c0""
ORDER BY ""c0"".""Country""");
        }

        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();

            AssertSql(
                @"SELECT ""order0"".""OrderID"", ""order0"".""CustomerID"", ""order0"".""EmployeeID"", ""order0"".""OrderDate"", ""orderDetail0"".""OrderID"", ""orderDetail0"".""ProductID"", ""orderDetail0"".""Discount"", ""orderDetail0"".""Quantity"", ""orderDetail0"".""UnitPrice""
FROM ""Orders"" ""order0""
LEFT JOIN ""Order Details"" ""orderDetail0"" ON ""order0"".""OrderID"" = ""orderDetail0"".""OrderID""
ORDER BY ""order0"".""OrderID""");
        }

        public override void OrderBy_arithmetic()
        {
            base.OrderBy_arithmetic();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" ""e""
ORDER BY ""e"".""EmployeeID"" - ""e"".""EmployeeID""");
        }

        public override void OrderBy_condition_comparison()
        {
            base.OrderBy_condition_comparison();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
ORDER BY CASE
    WHEN ""p"".""UnitsInStock"" > 0
    THEN 1 ELSE 0
END, ""p"".""ProductID""");
        }

        public override void OrderBy_ternary_conditions()
        {
            base.OrderBy_ternary_conditions();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
ORDER BY CASE
    WHEN ((""p"".""UnitsInStock"" > 10) AND (""p"".""ProductID"" > 40)) OR ((""p"".""UnitsInStock"" <= 10) AND (""p"".""ProductID"" <= 40))
    THEN 1 ELSE 0
END, ""p"".""ProductID""");
        }

        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(
                @"SELECT ""p"".""CustomerID"", ""p"".""Address"", ""p"".""City"", ""p"".""CompanyName"", ""p"".""ContactName"", ""p"".""ContactTitle"", ""p"".""Country"", ""p"".""Fax"", ""p"".""Phone"", ""p"".""PostalCode"", ""p"".""Region""
FROM ""Customers"" ""p""
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM ""Orders"" ""o""
            WHERE (""o"".""OrderID"" > 11000) AND (""p"".""CustomerID"" = ""o"".""CustomerID""))
        THEN 1 ELSE 0
    END FROM DUAL
), ""p"".""CustomerID""");
        }

        public override void Skip()
        {
            base.Skip();

            AssertSql(
                @":p_0='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""
OFFSET :p_0 ROWS");
        }

        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            AssertSql(
                @":p_0='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
OFFSET :p_0 ROWS");
        }

        public override void Skip_Take()
        {
            base.Skip_Take();

            AssertSql(
                @":p_0='5'
:p_1='10'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""ContactName""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""c"".""ContactName"", ""o"".""OrderID""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
ORDER BY ""o"".""OrderID""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT (""c"".""ContactName"" + N' ') + ""c"".""ContactTitle"" ""Contact"", ""o"".""OrderID""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
ORDER BY ""o"".""OrderID""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""o"".""OrderID"", ""ca"".""CustomerID"" ""CustomerIDA"", ""cb"".""CustomerID"" ""CustomerIDB"", ""ca"".""ContactName"" ""ContactNameA"", ""cb"".""ContactName"" ""ContactNameB""
FROM ""Orders"" ""o""
INNER JOIN ""Customers"" ""ca"" ON ""o"".""CustomerID"" = ""ca"".""CustomerID""
INNER JOIN ""Customers"" ""cb"" ON ""o"".""CustomerID"" = ""cb"".""CustomerID""
ORDER BY ""o"".""OrderID""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""ContactName""
) ""t""
ORDER BY ""t"".""ContactName""
OFFSET :p_1 ROWS");
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName""
    ) ""t""
    ORDER BY ""t"".""ContactName""
    OFFSET :p_1 ROWS
) ""t0""");
        }

        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName""
        FETCH FIRST :p_0 ROWS ONLY
    ) ""t""
    ORDER BY ""t"".""ContactName""
    OFFSET :p_1 ROWS
) ""t0""",
                //
                @":p_0='15'
:p_1='10'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName""
        FETCH FIRST :p_0 ROWS ONLY
    ) ""t""
    ORDER BY ""t"".""ContactName""
    OFFSET :p_1 ROWS
) ""t0""");
        }

        public void Skip_when_no_OrderBy()
        {
            Assert.Throws<Exception>(() => CreateContext().Set<Customer>().Skip(5).Take(10).ToList());
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            AssertSql(
                @":p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""t"".*
    FROM (
        SELECT ""o"".*
        FROM ""Orders"" ""o""
    ) ""t""
) ""t0""");
        }

        public override void Take_Where_Distinct_Count()
        {
            base.Take_Where_Distinct_Count();

            AssertSql(
                @":p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""t"".*
    FROM (
        SELECT ""o"".*
        FROM ""Orders"" ""o""
        WHERE ""o"".""CustomerID"" = N'FRANK'
    ) ""t""
) ""t0""");
        }

        public override void Null_conditional_simple()
        {
            base.Null_conditional_simple();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Null_conditional_deep()
        {
            base.Null_conditional_deep();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE CAST(LENGTH(""c"".""CustomerID"") int) = 5");
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            AssertSql(
                @"SELECT ""c3"".""CustomerID"", ""c3"".""Address"", ""c3"".""City"", ""c3"".""CompanyName"", ""c3"".""ContactName"", ""c3"".""ContactTitle"", ""c3"".""Country"", ""c3"".""Fax"", ""c3"".""Phone"", ""c3"".""PostalCode"", ""c3"".""Region""
FROM ""Customers"" ""c3""");
        }

        public override void Queryable_simple_anonymous_projection_subquery()
        {
            base.Queryable_simple_anonymous_projection_subquery();

            AssertSql(
                @":p_0='91'

SELECT ""c"".""City""
FROM ""Customers"" ""c""");
        }

        public override void Queryable_simple_anonymous_subquery()
        {
            base.Queryable_simple_anonymous_subquery();

            AssertSql(
                @":p_0='91'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void Take_simple()
        {
            base.Take_simple();

            AssertSql(
                @":p_0='10'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();

            AssertSql(
                @":p_0='10'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            AssertSql(
                @":p_0='10'

SELECT ""c"".""City""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();

            AssertSql(
                @":p_0='2'

SELECT ""c"".""City""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();

            AssertSql(
                @":p_0='5'

SELECT COUNT(*)
FROM (
    SELECT ""o"".*
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();

            AssertSql(
                @":p_0='5'

SELECT COUNT(*)
FROM (
    SELECT ""o"".*
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Any_simple()
        {
            base.Any_simple();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE ""c"".""ContactName"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""ContactName"", 1, LENGTH(N'A')) = N'A'))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Any_nested_negated()
        {
            base.Any_nested_negated();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL) AND NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL)");
        }

        public override void Any_nested()
        {
            base.Any_nested();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override void Any_nested2()
        {
            base.Any_nested2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL) AND EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override void Any_nested3()
        {
            base.Any_nested3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL)");
        }

        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (""c"".""City"" = N'London') AND EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE (""o"".""EmployeeID"" = 1) AND (""c"".""CustomerID"" = ""o"".""CustomerID""))");
        }

        public override void All_top_level()
        {
            base.All_top_level();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE NOT (""c"".""ContactName"" LIKE N'A' || N'%') OR (SUBSTR(""c"".""ContactName"", 1, LENGTH(N'A')) <> N'A'))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void All_top_level_column()
        {
            base.All_top_level_column();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE (NOT (""c"".""ContactName"" LIKE ""c"".""ContactName"" || N'%') OR (SUBSTR(""c"".""ContactName"", 1, LENGTH(""c"".""ContactName"")) <> ""c"".""ContactName"")) AND ((""c"".""ContactName"" <> N'') OR ""c"".""ContactName"" IS NULL))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void All_top_level_subquery()
        {
            base.All_top_level_subquery();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c1""
        WHERE NOT ((
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM ""Customers"" ""c2""
                    WHERE EXISTS (
                        SELECT 1
                        FROM ""Customers"" ""c3""
                        WHERE ""c1"".""CustomerID"" = ""c3"".""CustomerID""))
                THEN 1 ELSE 0
            END
        ) = 1))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void All_top_level_subquery_ef_property()
        {
            base.All_top_level_subquery_ef_property();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c1""
        WHERE NOT ((
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM ""Customers"" ""c2""
                    WHERE EXISTS (
                        SELECT 1
                        FROM ""Customers"" ""c3""
                        WHERE ""c1"".""CustomerID"" = ""c3"".""CustomerID""))
                THEN 1 ELSE 0
            END
        ) = 1))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE (""c"".""City"" = N'London') OR (""e"".""City"" = N'London')");
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin')");
        }

        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin', N'Seattle')");
        }

        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();

            AssertSql(
                @":london_0='London' (Size = 4000)
:lisboa_1='Lisboa' (Size = 4000)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (:london_0, N'Berlin', N'Seattle', :lisboa_1)");
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            AssertSql(
                @":p_0='2'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
    ORDER BY ""e"".""EmployeeID""
) ""t""",
                //
                @"SELECT ""t0"".""CustomerID"", ""t0"".""Address"", ""t0"".""City"", ""t0"".""CompanyName"", ""t0"".""ContactName"", ""t0"".""ContactTitle"", ""t0"".""Country"", ""t0"".""Fax"", ""t0"".""Phone"", ""t0"".""PostalCode"", ""t0"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t0""",
                //
                @"SELECT ""t0"".""CustomerID"", ""t0"".""Address"", ""t0"".""City"", ""t0"".""CompanyName"", ""t0"".""ContactName"", ""t0"".""ContactTitle"", ""t0"".""Country"", ""t0"".""Fax"", ""t0"".""Phone"", ""t0"".""PostalCode"", ""t0"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t0""",
                //
                @"SELECT ""t0"".""CustomerID"", ""t0"".""Address"", ""t0"".""City"", ""t0"".""CompanyName"", ""t0"".""ContactName"", ""t0"".""ContactTitle"", ""t0"".""Country"", ""t0"".""Fax"", ""t0"".""Phone"", ""t0"".""PostalCode"", ""t0"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t0""",
                //
                @"SELECT ""t0"".""CustomerID"", ""t0"".""Address"", ""t0"".""City"", ""t0"".""CompanyName"", ""t0"".""ContactName"", ""t0"".""ContactTitle"", ""t0"".""Country"", ""t0"".""Fax"", ""t0"".""Phone"", ""t0"".""PostalCode"", ""t0"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t0""");
        }

        public override void SelectMany_simple_subquery()
        {
            base.SelectMany_simple_subquery();

            AssertSql(
                @":p_0='9'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""
CROSS JOIN ""Customers"" ""c""");
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Employees"" ""e""
CROSS JOIN ""Customers"" ""c""");
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e2"".""FirstName"" ""FirstName0""
FROM ""Employees"" ""e1""
CROSS JOIN ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e2""");
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title"", ""e3"".""EmployeeID"", ""e3"".""City"", ""e3"".""Country"", ""e3"".""FirstName"", ""e3"".""ReportsTo"", ""e3"".""Title"", ""e4"".""EmployeeID"", ""e4"".""City"", ""e4"".""Country"", ""e4"".""FirstName"", ""e4"".""ReportsTo"", ""e4"".""Title""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""
CROSS JOIN ""Employees"" ""e3""
CROSS JOIN ""Employees"" ""e4""");
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            AssertSql(
                @"SELECT ""e1"".""City"", ""e2"".""Country""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""");
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            AssertSql(
                @"SELECT ""e1"".""City"", ""e2"".""Country"", ""e3"".""FirstName""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""
CROSS JOIN ""Employees"" ""e3""");
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""");
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""");
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        CROSS JOIN ""Orders"" ""o"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""");
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        INNER JOIN ""Orders"" ""or"" ON ""c"".""CustomerID"" = ""or"".""CustomerID""
        INNER JOIN ""Order Details"" ""od"" ON ""or"".""OrderID"" = ""od"".""OrderID""
        WHERE ""c"".""City"" = N'London')
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Where_join_select()
        {
            base.Where_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Where_orderby_join_select()
        {
            base.Where_orderby_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" <> N'ALFKI'
ORDER BY ""c"".""CustomerID""");
        }

        public override void Where_join_orderby_join_select()
        {
            base.Where_join_orderby_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
INNER JOIN ""Order Details"" ""od"" ON ""o"".""OrderID"" = ""od"".""OrderID""
WHERE ""c"".""CustomerID"" <> N'ALFKI'
ORDER BY ""c"".""CustomerID""");
        }

        public override void Where_select_many()
        {
            base.Where_select_many();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Where_orderby_select_many()
        {
            base.Where_orderby_select_many();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE ""c"".""CustomerID"" = N'ALFKI'
ORDER BY ""c"".""CustomerID""");
        }

        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
ORDER BY ""o"".""CustomerID""");
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();

            AssertSql(
                @"SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" ""o0""
ORDER BY ""o0"".""CustomerID""");
        }

        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();

            AssertSql(
                @"SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" ""o0""
ORDER BY ""o0"".""CustomerID""");
        }

        public override void GroupBy_DateTimeOffset_Property()
        {
            base.GroupBy_DateTimeOffset_Property();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL
ORDER BY DATEPART(month, ""o"".""OrderDate"")");
        }

        public override void GroupBy_with_orderby()
        {
            base.GroupBy_with_orderby();

            AssertSql(
                @"SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" ""o0""
ORDER BY ""o0"".""CustomerID"", ""o0"".""OrderID""");
        }

        public override void GroupBy_with_orderby_and_anonymous_projection()
        {
            base.GroupBy_with_orderby_and_anonymous_projection();

            AssertSql(
                @"SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" ""o0""
ORDER BY ""o0"".""CustomerID""");
        }

        public override void GroupBy_with_orderby_take_skip_distinct()
        {
            base.GroupBy_with_orderby_take_skip_distinct();

            AssertSql(
                @"SELECT ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" ""o0""
ORDER BY ""o0"".""CustomerID""");
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""City"" ""City0""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE (""c"".""City"" = ""e"".""City"") OR (""c"".""City"" IS NULL AND ""e"".""City"" IS NULL)
ORDER BY ""City0"", ""c"".""CustomerID"" DESC");
        }

        public override void SelectMany_Joined_DefaultIfEmpty()
        {
            base.SelectMany_Joined_DefaultIfEmpty();

            AssertSql(
                @"SELECT ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""c"".""ContactName""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate""
    FROM (
        SELECT NULL ""empty"" FROM DUAL
    ) ""empty""
    LEFT JOIN (
        SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
        FROM ""Orders"" ""o""
        WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
    ) ""t"" ON 1 = 1
) ""t0""");
        }

        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();

            AssertSql(
                @"SELECT ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate""
    FROM (
        SELECT NULL ""empty""
    ) ""empty""
    LEFT JOIN (
        SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
        FROM ""Orders"" ""o""
        WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
    ) ""t"" ON 1 = 1
) ""t0""");
        }

        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            AssertSql(
                @"SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate"", ""c"".""ContactName""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
) ""t""");
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            AssertSql(
                @":p_0='1'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t""
ORDER BY ""t"".""CustomerID""");
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            AssertSql(
                @":p_0='1'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"" ""CustomerID0"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Customers"" ""c""
    CROSS JOIN ""Orders"" ""o""
    ORDER BY ""c"".""CustomerID"", ""o"".""OrderID""
) ""t""
ORDER BY ""t"".""CustomerID"", ""t"".""OrderID""");
        }

        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();

            AssertSql(
                @":p_0='5'
:p_1='10'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName""
        OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY)
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Skip_Take_All()
        {
            base.Skip_Take_All();

            AssertSql(
                @":p_0='5'
:p_1='10'

SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE CAST(LENGTH(""c"".""CustomerID"") int) <> 5
        ORDER BY ""c"".""ContactName""
        OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY)
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void OrderBy()
        {
            base.OrderBy();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void OrderBy_true()
        {
            base.OrderBy_true();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_integer()
        {
            base.OrderBy_integer();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_parameter()
        {
            base.OrderBy_parameter();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY (SELECT 1)");
        }

        public override void OrderBy_anon()
        {
            base.OrderBy_anon();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void OrderBy_anon2()
        {
            base.OrderBy_anon2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            AssertContainsSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""",
                //
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override void Take_Distinct()
        {
            base.Take_Distinct();

            AssertSql(
                @":p_0='5'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Distinct_Take()
        {
            base.Distinct_Take();

            AssertSql(
                @":p_0='5'

SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate""
FROM (
    SELECT DISTINCT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
) ""t""
ORDER BY ""t"".""OrderID""");
        }

        public override void Distinct_Take_Count()
        {
            base.Distinct_Take_Count();

            AssertSql(
                @":p_0='5'

SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""o"".*
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" ""e""
ORDER BY ""e"".""Title"", ""e"".""EmployeeID""");
        }

        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            AssertSql(
                @"SELECT ""c"".""City""
FROM ""Customers"" ""c""
ORDER BY ""c"".""Country"", ""c"".""CustomerID""");
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void OrderBy_correlated_subquery1()
        {
            base.OrderBy_correlated_subquery1();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM ""Customers"" ""c2""
            WHERE ""c2"".""CustomerID"" = ""c"".""CustomerID"")
        THEN 1 ELSE 0
    END
)");
        }

        public override void OrderBy_correlated_subquery2()
        {
            base.OrderBy_correlated_subquery2();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" <= 10250) AND ((
    SELECT ""c"".""City""
    FROM ""Customers"" ""c""
    ORDER BY (
        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM ""Customers"" ""c2""
                WHERE ""c2"".""CustomerID"" = N'ALFKI')
            THEN 1 ELSE 0
        END
    )
) <> N'Nowhere')");
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE EXISTS (
    SELECT 1
    FROM ""Employees"" ""e2""
    WHERE EXISTS (
        SELECT 1
        FROM ""Employees"" ""e3""))
ORDER BY ""e1"".""EmployeeID""");
        }

        public override void Where_query_composition4()
        {
            base.Where_query_composition4();

            AssertSql(
                @":p_0='2'

SELECT ""t"".""CustomerID"", ""t"".""Address"", ""t"".""City"", ""t"".""CompanyName"", ""t"".""ContactName"", ""t"".""ContactTitle"", ""t"".""Country"", ""t"".""Fax"", ""t"".""Phone"", ""t"".""PostalCode"", ""t"".""Region""
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""CustomerID""
) ""t""",
                //
                @"SELECT 1
FROM ""Customers"" ""c0""
ORDER BY ""c0"".""CustomerID""",
                //
                @"SELECT ""c2"".""CustomerID"", ""c2"".""Address"", ""c2"".""City"", ""c2"".""CompanyName"", ""c2"".""ContactName"", ""c2"".""ContactTitle"", ""c2"".""Country"", ""c2"".""Fax"", ""c2"".""Phone"", ""c2"".""PostalCode"", ""c2"".""Region""
FROM ""Customers"" ""c2""",
                //
                @"SELECT 1
FROM ""Customers"" ""c0""
ORDER BY ""c0"".""CustomerID""",
                //
                @"SELECT ""c2"".""CustomerID"", ""c2"".""Address"", ""c2"".""City"", ""c2"".""CompanyName"", ""c2"".""ContactName"", ""c2"".""ContactTitle"", ""c2"".""Country"", ""c2"".""Fax"", ""c2"".""Phone"", ""c2"".""PostalCode"", ""c2"".""Region""
FROM ""Customers"" ""c2""");
        }

        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT 1
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT ""o"".""CustomerID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server();

            AssertSql(
                @"SELECT ""t"".""Id"", ""t"".""Count"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderID"" < 10300
) ""t""
CROSS JOIN ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = ""t"".""Id""");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""t"".""Id"", ""t"".""Count""
FROM ""Customers"" ""c""
CROSS JOIN (
    SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderID"" < 10300
) ""t""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')");
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""CustomerID""
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""CustomerID""",
                //
                @":outer_CustomerID='ALFKI' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANATR' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANTON' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID");
        }

        public override void Select_correlated_subquery_filtered()
        {
            base.Select_correlated_subquery_filtered();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID""",
                //
                @":outer_CustomerID='ALFKI' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANATR' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANTON' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='AROUT' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID");
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            AssertSql(
                @":p_0='3'

SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Where_subquery_on_bool()
        {
            base.Where_subquery_on_bool();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
WHERE N'Chai' IN (
    SELECT ""p2"".""ProductName""
    FROM ""Products"" ""p2""
)");
        }

        public override void Where_subquery_on_collection()
        {
            base.Where_subquery_on_collection();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
WHERE 5 IN (
    SELECT ""o"".""Quantity""
    FROM ""Order Details"" ""o""
    WHERE ""o"".""ProductID"" = ""p"".""ProductID""
)");
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            AssertSql(
                @"SELECT ""c0"".""CustomerID"", ""c0"".""Address"", ""c0"".""City"", ""c0"".""CompanyName"", ""c0"".""ContactName"", ""c0"".""ContactTitle"", ""c0"".""Country"", ""c0"".""Fax"", ""c0"".""Phone"", ""c0"".""PostalCode"", ""c0"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Customers"" ""c0""");
        }

        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY COALESCE(""c"".""Region"", N'ZZ')");
        }

        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
FROM ""Customers"" ""c""
ORDER BY ""Region""");
        }

        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY CASE
    WHEN ""c"".""Region"" IS NULL
    THEN N'ZZ' ELSE ""c"".""Region""
END FROM DUAL");
        }

        public override void OrderBy_conditional_operator_where_condition_null()
        {
            base.OrderBy_conditional_operator_where_condition_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY CASE
    WHEN 0 = 1
    THEN N'ZZ' ELSE ""c"".""City""
END FROM DUAL");
        }

        public override void OrderBy_comparison_operator()
        {
            base.OrderBy_comparison_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY CASE
    WHEN ""c"".""Region"" = N'ASK'
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
FROM ""Customers"" ""c""");
        }

        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE COALESCE(""c"".""CompanyName"", ""c"".""ContactName"") = N'The Big Cheese'");
        }

        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", COALESCE(""c"".""Region"", N'ZZ') ""c""
        FROM ""Customers"" ""c""
        ORDER BY ""c""
    ) ""t""
    ORDER BY ""t"".""c""
    OFFSET :p_1 ROWS
) ""t0""");
        }

        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            AssertSql(
                @":p_0='5'

SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
FROM ""Customers"" ""c""
ORDER BY ""Region""");
        }

        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""Region""
) ""t""
ORDER BY ""t"".""Region""
OFFSET :p_1 ROWS");
        }

        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();

              AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", ""c"".""Region"", COALESCE(""c"".""Region"", N'ZZ') ""c""
    FROM ""Customers"" ""c""
    ORDER BY ""c""
    FETCH FIRST :p_0 ROWS ONLY
) ""t""
ORDER BY ""t"".""c""
OFFSET :p_1 ROWS");
        }

        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", COALESCE(""c"".""Region"", N'ZZ') ""c""
    FROM ""Customers"" ""c""
    ORDER BY ""c""
) ""t""
ORDER BY ""t"".""c""
OFFSET :p_1 ROWS");
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY COALESCE(""c"".""Region"", N'ZZ')");
        }

        public override void Does_not_change_ordering_of_projection_with_complex_projections()
        {
            base.Does_not_change_ordering_of_projection_with_complex_projections();

            AssertSql(
                @"SELECT ""e"".""CustomerID"" ""Id"", (
    SELECT COUNT(*)
    FROM ""Orders"" ""o0""
    WHERE ""e"".""CustomerID"" = ""o0"".""CustomerID""
) ""TotalOrders""
FROM ""Customers"" ""e""
WHERE (""e"".""ContactTitle"" = N'Owner') AND ((
    SELECT COUNT(*)
    FROM ""Orders"" ""o""
    WHERE ""e"".""CustomerID"" = ""o"".""CustomerID""
) > 2)
ORDER BY ""Id""");
        }

        public override void DateTime_parse_is_parameterized()
        {
            base.DateTime_parse_is_parameterized();

            AssertSql(
                @":Parse_0='01/01/1998 12:00:00'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" > :Parse_0");
        }

        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Environment_newline_is_funcletized()
        {
            base.Environment_newline_is_funcletized();

              AssertSql(
                @":NewLine_0='
' (Size = 4000)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (INSTR(""c"".""CustomerID"", :NewLine_0) > 0) OR (:NewLine_0 = N'')");
        }

        public override void String_concat_with_navigation1()
        {
            base.String_concat_with_navigation1();

            AssertSql(
                @"SELECT (""o"".""CustomerID"" || N' ') || ""o.Customer"".""City""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""");
        }

        public override void String_concat_with_navigation2()
        {
            base.String_concat_with_navigation2();

            AssertSql(
                @"SELECT (""o.Customer"".""City"" || N' ') || ""o.Customer"".""City""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""");
        }

        public override void Where_bitwise_or()
        {
            base.Where_bitwise_or();

              AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END - BITAND(CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END, CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) + CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1");
        }

        public override void Where_bitwise_and()
        {
            base.Where_bitwise_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (BITAND(CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END, CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END)) = 1");
        }

        public override void Select_bitwise_or()
        {
            base.Select_bitwise_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END | CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Select_bitwise_or_multiple()
        {
            base.Select_bitwise_or_multiple();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", (CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END | CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) | CASE
    WHEN ""c"".""CustomerID"" = N'ANTON'
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Select_bitwise_and()
        {
            base.Select_bitwise_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END & CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Select_bitwise_and_or()
        {
            base.Select_bitwise_and_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", (CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END & CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) | CASE
    WHEN ""c"".""CustomerID"" = N'ANTON'
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Where_bitwise_or_with_logical_or()
        {
            base.Where_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END | CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1) OR (""c"".""CustomerID"" = N'ANTON')");
        }

        public override void Where_bitwise_and_with_logical_and()
        {
            base.Where_bitwise_and_with_logical_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END & CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1) AND (""c"".""CustomerID"" = N'ANTON')");
        }

        public override void Where_bitwise_or_with_logical_and()
        {
            base.Where_bitwise_or_with_logical_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END | CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1) AND (""c"".""Country"" = N'Germany')");
        }

        public override void Where_bitwise_and_with_logical_or()
        {
            base.Where_bitwise_and_with_logical_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END & CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1) OR (""c"".""CustomerID"" = N'ANTON')");
        }

        public override void Select_bitwise_or_with_logical_or()
        {
            base.Select_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", CASE
    WHEN ((CASE
        WHEN ""c"".""CustomerID"" = N'ALFKI'
        THEN 1 ELSE 0
    END | CASE
        WHEN ""c"".""CustomerID"" = N'ANATR'
        THEN 1 ELSE 0
    END) = 1) OR (""c"".""CustomerID"" = N'ANTON')
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Select_bitwise_and_with_logical_and()
        {
            base.Select_bitwise_and_with_logical_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", CASE
    WHEN ((BITAND(CASE
        WHEN ""c"".""CustomerID"" = N'ALFKI'
        THEN 1 ELSE 0
    END, CASE
        WHEN ""c"".""CustomerID"" = N'ANATR'
        THEN 1 ELSE 0
    END)) = 1) AND (""c"".""CustomerID"" = N'ANTON')
    THEN 1 ELSE 0
END ""Value""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Handle_materialization_properly_when_more_than_two_query_sources_are_involved()
        {
            base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
CROSS JOIN ""Employees"" ""e""
ORDER BY ""c"".""CustomerID""");
        }

        public override void Parameter_extraction_short_circuits_1()
        {
            base.Parameter_extraction_short_circuits_1();

            AssertSql(
                @":dateFilter_Value_Month_0='7'
:dateFilter_Value_Year_1='1996'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" < 10400) AND ((""o"".""OrderDate"" IS NOT NULL AND (DATEPART(month, ""o"".""OrderDate"") = :dateFilter_Value_Month_0)) AND (DATEPART(year, ""o"".""OrderDate"") = :dateFilter_Value_Year_1))",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10400");
        }

        public override void Parameter_extraction_short_circuits_2()
        {
            base.Parameter_extraction_short_circuits_2();

            AssertSql(
                @":dateFilter_Value_Month_0='7'
:dateFilter_Value_Year_1='1996'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" < 10400) AND ((""o"".""OrderDate"" IS NOT NULL AND (DATEPART(month, ""o"".""OrderDate"") = :dateFilter_Value_Month_0)) AND (DATEPART(year, ""o"".""OrderDate"") = :dateFilter_Value_Year_1))",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE 0 = 1");
        }

        public override void Parameter_extraction_short_circuits_3()
        {
            base.Parameter_extraction_short_circuits_3();

            AssertSql(
                @":dateFilter_Value_Month_0='7'
:dateFilter_Value_Year_1='1996'

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" < 10400) OR ((""o"".""OrderDate"" IS NOT NULL AND (DATEPART(month, ""o"".""OrderDate"") = :dateFilter_Value_Month_0)) AND (DATEPART(year, ""o"".""OrderDate"") = :dateFilter_Value_Year_1))",
                //
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Subquery_member_pushdown_does_not_change_original_subquery_model()
        {
            base.Subquery_member_pushdown_does_not_change_original_subquery_model();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""CustomerID"", ""t"".""OrderID""
FROM (
    SELECT ""o"".*
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""",
                //
                @":outer_CustomerID='VINET' (Size = 5)

SELECT ""c0"".""City""
FROM ""Customers"" ""c0""
WHERE ""c0"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='TOMSP' (Size = 5)

SELECT ""c0"".""City""
FROM ""Customers"" ""c0""
WHERE ""c0"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='HANAR' (Size = 5)

SELECT ""c0"".""City""
FROM ""Customers"" ""c0""
WHERE ""c0"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID1='TOMSP' (Size = 5)

SELECT ""c2"".""City""
FROM ""Customers"" ""c2""
WHERE ""c2"".""CustomerID"" = :outer_CustomerID1",
                //
                @":outer_CustomerID1='VINET' (Size = 5)

SELECT ""c2"".""City""
FROM ""Customers"" ""c2""
WHERE ""c2"".""CustomerID"" = :outer_CustomerID1",
                //
                @":outer_CustomerID1='HANAR' (Size = 5)

SELECT ""c2"".""City""
FROM ""Customers"" ""c2""
WHERE ""c2"".""CustomerID"" = :outer_CustomerID1");
        }

        public override void Query_expression_with_to_string_and_contains()
        {
            base.Query_expression_with_to_string_and_contains();

            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (INSTR(TO_CHAR(""o"".""EmployeeID""), N'10') > 0)");
        }

        public override void Select_expression_long_to_string()
        {
            base.Select_expression_long_to_string();

            AssertSql(
                @"SELECT TO_CHAR(""o"".""OrderID"") ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_int_to_string()
        {
            base.Select_expression_int_to_string();

            AssertSql(
                @"SELECT TO_CHAR(""o"".""OrderID"") ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void ToString_with_formatter_is_evaluated_on_the_client()
        {
            base.ToString_with_formatter_is_evaluated_on_the_client();

            AssertSql(
                @"SELECT ""o"".""OrderID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL",
                //
                @"SELECT ""o"".""OrderID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_other_to_string()
        {
            base.Select_expression_other_to_string();

            AssertSql(
                @"SELECT TO_CHAR(""o"".""OrderDate"") ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_date_add_year()
        {
            base.Select_expression_date_add_year();

            AssertSql(
                @"SELECT DATEADD(year, 1, ""o"".""OrderDate"") ""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_above_the_range()
        {
            base.Select_expression_date_add_milliseconds_above_the_range();

            AssertSql(
                @"SELECT ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_below_the_range()
        {
            base.Select_expression_date_add_milliseconds_below_the_range();

            AssertSql(
                @"SELECT ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_date_add_milliseconds_large_number_divided()
        {
            base.Select_expression_date_add_milliseconds_large_number_divided();

            AssertSql(
                @":millisecondsPerDay_1='86400000'
:millisecondsPerDay_0='86400000'

SELECT DATEADD(millisecond, DATEPART(millisecond, ""o"".""OrderDate"") % :millisecondsPerDay_1, DATEADD(day, DATEPART(millisecond, ""o"".""OrderDate"") / :millisecondsPerDay_0, ""o"".""OrderDate"")) ""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override void Select_expression_references_are_updated_correctly_with_subquery()
        {
            base.Select_expression_references_are_updated_correctly_with_subquery();

              AssertSql(
                @":nextYear_0='2017'

SELECT ""t"".""c""
FROM (
    SELECT DISTINCT EXTRACT(YEAR FROM ""o"".""OrderDate"") ""c""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderDate"" IS NOT NULL
) ""t""
WHERE ""t"".""c"" < :nextYear_0");
        }

        public override void DefaultIfEmpty_without_group_join()
        {
            base.DefaultIfEmpty_without_group_join();

            AssertSql(
                @"SELECT ""t0"".""CustomerID""
FROM (
    SELECT ""t"".*
    FROM (
        SELECT NULL ""empty""
    ) ""empty""
    LEFT JOIN (
        SELECT ""c"".*
        FROM ""Customers"" ""c""
        WHERE ""c"".""City"" = N'London'
    ) ""t"" ON 1 = 1
) ""t0""
WHERE ""t0"".""CustomerID"" IS NOT NULL");
        }

        public override void DefaultIfEmpty_in_subquery()
        {
            base.DefaultIfEmpty_in_subquery();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""t0"".""OrderID""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""t"".*
    FROM (
        SELECT NULL ""empty""
    ) ""empty""
    LEFT JOIN (
        SELECT ""o"".*
        FROM ""Orders"" ""o""
        WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
    ) ""t"" ON 1 = 1
) ""t0""
WHERE ""t0"".""OrderID"" IS NOT NULL");
        }

        public override void DefaultIfEmpty_in_subquery_nested()
        {
            base.DefaultIfEmpty_in_subquery_nested();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""t0"".""OrderID"", ""t2"".""OrderDate""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""t"".*
    FROM (
        SELECT NULL ""empty""
    ) ""empty""
    LEFT JOIN (
        SELECT ""o"".*
        FROM ""Orders"" ""o""
        WHERE ""o"".""OrderID"" > 11000
    ) ""t"" ON 1 = 1
) ""t0""
CROSS APPLY (
    SELECT ""t1"".*
    FROM (
        SELECT NULL ""empty""
    ) ""empty0""
    LEFT JOIN (
        SELECT ""o0"".*
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = ""c"".""CustomerID""
    ) ""t1"" ON 1 = 1
) ""t2""
WHERE (""c"".""City"" = N'Seattle') AND (""t0"".""OrderID"" IS NOT NULL AND ""t2"".""OrderID"" IS NOT NULL)");
        }

        public override void OrderBy_skip_take()
        {
            base.OrderBy_skip_take();

            AssertSql(
                @":p_0='5'
:p_1='8'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override void OrderBy_skip_take_take()
        {
            base.OrderBy_skip_take_take();

            AssertSql(
                @":p_2='3'
:p_0='5'
:p_1='8'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t""
ORDER BY ""t"".""ContactTitle"", ""t"".""ContactName""");
        }

        public override void OrderBy_skip_take_take_take_take()
        {
            base.OrderBy_skip_take_take_take_take();

            AssertSql(
                @":p_4='5'
:p_3='8'
:p_2='10'
:p_0='5'
:p_1='15'

SELECT ""t1"".*
FROM (
    SELECT ""t0"".*
    FROM (
        SELECT ""t"".*
        FROM (
            SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
            FROM ""Customers"" ""c""
            ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
            OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
        ) ""t""
        ORDER BY ""t"".""ContactTitle"", ""t"".""ContactName""
    ) ""t0""
    ORDER BY ""t0"".""ContactTitle"", ""t0"".""ContactName""
) ""t1""
ORDER BY ""t1"".""ContactTitle"", ""t1"".""ContactName""");
        }

        public override void OrderBy_skip_take_skip_take_skip()
        {
            base.OrderBy_skip_take_skip_take_skip();

            AssertSql(
                @":p_0='5'
:p_1='15'
:p_2='2'
:p_3='8'
:p_4='5'

SELECT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
        OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
    ) ""t""
    ORDER BY ""t"".""ContactTitle"", ""t"".""ContactName""
    OFFSET :p_2 ROWS FETCH NEXT :p_3 ROWS ONLY
) ""t0""
ORDER BY ""t0"".""ContactTitle"", ""t0"".""ContactName""
OFFSET :p_4 ROWS");
        }

        public override void OrderBy_skip_take_distinct()
        {
            base.OrderBy_skip_take_distinct();

            AssertSql(
                @":p_0='5'
:p_1='15'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t""");
        }

        public override void OrderBy_coalesce_take_distinct()
        {
            base.OrderBy_coalesce_take_distinct();

            AssertSql(
                @":p_0='15'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
    FROM ""Products"" ""p""
    ORDER BY COALESCE(""p"".""UnitPrice"", 0.0)
) ""t""");
        }

        public override void OrderBy_coalesce_skip_take_distinct()
        {
            base.OrderBy_coalesce_skip_take_distinct();

            AssertSql(
                @":p_0='5'
:p_1='15'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
    FROM ""Products"" ""p""
    ORDER BY COALESCE(""p"".""UnitPrice"", 0.0)
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t""");
        }

        public override void OrderBy_coalesce_skip_take_distinct_take()
        {
            // Disabled, Distinct no order by
        }

        public override void OrderBy_skip_take_distinct_orderby_take()
        {
            base.OrderBy_skip_take_distinct_orderby_take();

            AssertSql(
                @":p_2='8'
:p_0='5'
:p_1='15'

SELECT ""t0"".""CustomerID"", ""t0"".""Address"", ""t0"".""City"", ""t0"".""CompanyName"", ""t0"".""ContactName"", ""t0"".""ContactTitle"", ""t0"".""Country"", ""t0"".""Fax"", ""t0"".""Phone"", ""t0"".""PostalCode"", ""t0"".""Region""
FROM (
    SELECT DISTINCT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactTitle"", ""c"".""ContactName""
        OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
    ) ""t""
) ""t0""
ORDER BY ""t0"".""ContactTitle""");
        }

        public override void No_orderby_added_for_fully_translated_manually_constructed_LOJ()
        {
            base.No_orderby_added_for_fully_translated_manually_constructed_LOJ();

            AssertSql(
                @"SELECT ""e1"".""City"" ""City1"", ""e2"".""City"" ""City2""
FROM ""Employees"" ""e1""
LEFT JOIN ""Employees"" ""e2"" ON ""e1"".""EmployeeID"" = ""e2"".""ReportsTo""");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON (""o"".""CustomerID"" = ""c"".""CustomerID"") AND (""o"".""OrderID"" = 10000)");
        }

        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON (""o"".""OrderID"" = 10000) AND (""o"".""CustomerID"" = ""c"".""CustomerID"")");
        }

        public override void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"" ""City1"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e1""
LEFT JOIN ""Employees"" ""e2"" ON ""e1"".""EmployeeID"" = ""e2"".""ReportsTo""
ORDER BY ""e1"".""EmployeeID""");
        }

        public override void Contains_with_DateTime_Date()
        {
            base.Contains_with_DateTime_Date();

            AssertSql(
                @"SELECT ""e"".""OrderID"", ""e"".""CustomerID"", ""e"".""EmployeeID"", ""e"".""OrderDate""
FROM ""Orders"" ""e""
WHERE CONVERT(date, ""e"".""OrderDate"") IN ('1996-07-04T00:00:00.000', '1996-07-16T00:00:00.000')",
                //
                @"SELECT ""e"".""OrderID"", ""e"".""CustomerID"", ""e"".""EmployeeID"", ""e"".""OrderDate""
FROM ""Orders"" ""e""
WHERE CONVERT(date, ""e"".""OrderDate"") IN ('1996-07-04T00:00:00.000')");
        }

        public override void Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            base.Contains_with_subquery_involving_join_binds_to_correct_table();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" > 11000) AND ""o"".""OrderID"" IN (
    SELECT ""od"".""OrderID""
    FROM ""Order Details"" ""od""
    INNER JOIN ""Products"" ""od.Product"" ON ""od"".""ProductID"" = ""od.Product"".""ProductID""
    WHERE ""od.Product"".""ProductName"" = N'Chai'
)");
        }

        public override void Complex_query_with_repeated_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_query_model_compiles_correctly();

            AssertSql(
                @"SELECT ""outer"".""CustomerID"", ""outer"".""Address"", ""outer"".""City"", ""outer"".""CompanyName"", ""outer"".""ContactName"", ""outer"".""ContactTitle"", ""outer"".""Country"", ""outer"".""Fax"", ""outer"".""Phone"", ""outer"".""PostalCode"", ""outer"".""Region""
FROM ""Customers"" ""outer""
WHERE ""outer"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c0""
        WHERE EXISTS (
            SELECT 1
            FROM ""Customers"" ""cc1""))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_nested_query_model_compiles_correctly();

            AssertSql(
                @"SELECT ""outer"".""CustomerID"", ""outer"".""Address"", ""outer"".""City"", ""outer"".""CompanyName"", ""outer"".""ContactName"", ""outer"".""ContactTitle"", ""outer"".""Country"", ""outer"".""Fax"", ""outer"".""Phone"", ""outer"".""PostalCode"", ""outer"".""Region""
FROM ""Customers"" ""outer""
WHERE ""outer"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c0""
        WHERE EXISTS (
            SELECT 1
            FROM ""Customers"" ""cc1""
            WHERE EXISTS (
                SELECT DISTINCT 1
                FROM (
                    SELECT ""inner1"".*
                    FROM ""Customers"" ""inner1""
                    ORDER BY ""inner1"".""CustomerID""
                    FETCH FIRST 10 ROWS ONLY
                ) ""t1"")))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Anonymous_member_distinct_where()
        {
            base.Anonymous_member_distinct_where();

            AssertSql(
                @"SELECT ""t"".""CustomerID""
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""CustomerID"" = N'ALFKI'");
        }

        public override void Anonymous_member_distinct_orderby()
        {
            base.Anonymous_member_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""CustomerID""
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""CustomerID""");
        }

        public override void Anonymous_member_distinct_result()
        {
            base.Anonymous_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""CustomerID"", 1, LENGTH(N'A')) = N'A')");
        }

        public override void Anonymous_complex_distinct_where()
        {
            base.Anonymous_complex_distinct_where();

            AssertSql(
                @"SELECT ""t"".""A""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" + ""c"".""City"" ""A""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""A"" = N'ALFKIBerlin'");
        }

        public override void Anonymous_complex_distinct_orderby()
        {
            base.Anonymous_complex_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""A""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" || ""c"".""City"" ""A""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""A""");
        }

        public override void Anonymous_complex_distinct_result()
        {
            base.Anonymous_complex_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" || ""c"".""City"" ""A""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""A"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""A"", 1, LENGTH(N'A')) = N'A')");
        }

        public override void Anonymous_complex_orderby()
        {
            base.Anonymous_complex_orderby();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" + ""c"".""City"" ""A""
FROM ""Customers"" ""c""
ORDER BY ""A""");
        }

        public override void Anonymous_subquery_orderby()
        {
            base.Anonymous_subquery_orderby();

            AssertSql(
                @"SELECT (
    SELECT ""o1"".""OrderDate""
    FROM ""Orders"" ""o1""
    WHERE ""c"".""CustomerID"" = ""o1"".""CustomerID""
    ORDER BY ""o1"".""OrderID"" DESC
) ""A""
FROM ""Customers"" ""c""
WHERE (
    SELECT COUNT(*)
    FROM ""Orders"" ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
) > 1
ORDER BY (
    SELECT ""o0"".""OrderDate""
    FROM ""Orders"" ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
    ORDER BY ""o0"".""OrderID"" DESC
)");
        }

        public override void DTO_member_distinct_where()
        {
            base.DTO_member_distinct_where();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" = N'ALFKI'");
        }

        public override void DTO_member_distinct_orderby()
        {
            base.DTO_member_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""Property""");
        }

        public override void DTO_member_distinct_result()
        {
            base.DTO_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""Property"", 1, LENGTH(N'A')) = N'A')");
        }

        public override void DTO_complex_distinct_where()
        {
            base.DTO_complex_distinct_where();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" + ""c"".""City"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" = N'ALFKIBerlin'");
        }

        public override void DTO_complex_distinct_orderby()
        {
            base.DTO_complex_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" + ""c"".""City"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""Property""");
        }

        public override void DTO_complex_distinct_result()
        {
            base.DTO_complex_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" + ""c"".""City"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""Property"", 1, LENGTH(N'A')) = N'A')");
        }

        public override void DTO_complex_orderby()
        {
            base.DTO_complex_orderby();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" || ""c"".""City"" ""Property""
FROM ""Customers"" ""c""
ORDER BY ""Property""");
        }

        public override void DTO_subquery_orderby()
        {
            base.DTO_subquery_orderby();

            AssertSql(
                @"SELECT (
    SELECT ""o1"".""OrderDate""
    FROM ""Orders"" ""o1""
    WHERE ""c"".""CustomerID"" = ""o1"".""CustomerID""
    ORDER BY ""o1"".""OrderID"" DESC
    FETCH FIRST 1 ROWS ONLY
) ""Property""
FROM ""Customers"" ""c""
WHERE (
    SELECT COUNT(*)
    FROM ""Orders"" ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
) > 1
ORDER BY (
    SELECT ""o0"".""OrderDate""
    FROM ""Orders"" ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
    ORDER BY ""o0"".""OrderID"" DESC
    FETCH FIRST 1 ROWS ONLY
)");
        }

        public override void Include_with_orderby_skip_preserves_ordering()
        {
            base.Include_with_orderby_skip_preserves_ordering();

            AssertSql(
                @":p_0='40'
:p_1='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" <> N'VAFFE'
ORDER BY ""c"".""City"", ""c"".""CustomerID""
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY",
                //
                @":p_0='40'
:p_1='5'

SELECT ""c.Orders"".""OrderID"", ""c.Orders"".""CustomerID"", ""c.Orders"".""EmployeeID"", ""c.Orders"".""OrderDate""
FROM ""Orders"" ""c.Orders""
INNER JOIN (
    SELECT ""c0"".""CustomerID"", ""c0"".""City""
    FROM ""Customers"" ""c0""
    WHERE ""c0"".""CustomerID"" <> N'VAFFE'
    ORDER BY ""c0"".""City"", ""c0"".""CustomerID""
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t"" ON ""c.Orders"".""CustomerID"" = ""t"".""CustomerID""
ORDER BY ""t"".""City"", ""t"".""CustomerID""");
        }

        public override void GroupBy_join_anonymous()
        {
            base.GroupBy_join_anonymous();

            AssertSql(
                @"SELECT ""order0"".""OrderID"", ""order0"".""CustomerID"", ""order0"".""EmployeeID"", ""order0"".""OrderDate"", ""orderDetail0"".""OrderID"", ""orderDetail0"".""ProductID"", ""orderDetail0"".""Discount"", ""orderDetail0"".""Quantity"", ""orderDetail0"".""UnitPrice""
FROM ""Orders"" ""order0""
LEFT JOIN ""Order Details"" ""orderDetail0"" ON ""order0"".""OrderID"" = ""orderDetail0"".""OrderID""
ORDER BY ""order0"".""OrderID""");
        }

        public override void Int16_parameter_can_be_used_for_int_column()
        {
            base.Int16_parameter_can_be_used_for_int_column();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" = 10300");
        }

        public override void Subquery_is_null_translated_correctly()
        {
            base.Subquery_is_null_translated_correctly();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (
    SELECT ""o"".""CustomerID""
    FROM ""Orders"" ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
    ORDER BY ""o"".""OrderID"" DESC
) IS NULL");
        }

        public override void Subquery_is_not_null_translated_correctly()
        {
            base.Subquery_is_not_null_translated_correctly();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (
    SELECT ""o"".""CustomerID""
    FROM ""Orders"" ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
    ORDER BY ""o"".""OrderID"" DESC
) IS NOT NULL");
        }

        public override void Select_take_average()
        {
            base.Select_take_average();

            AssertSql(
                @":p_0='10'

SELECT AVG(CAST(""t"".""OrderID"" AS float))
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Select_take_count()
        {
            base.Select_take_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_orderBy_take_count()
        {
            base.Select_orderBy_take_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country""
) ""t""");
        }

        public override void Select_take_long_count()
        {
            base.Select_take_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_orderBy_take_long_count()
        {
            base.Select_orderBy_take_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country""
) ""t""");
        }

        public override void Select_take_max()
        {
            base.Select_take_max();

            AssertSql(
                @":p_0='10'

SELECT MAX(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Select_take_min()
        {
            base.Select_take_min();

            AssertSql(
                @":p_0='10'

SELECT MIN(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Select_take_sum()
        {
            base.Select_take_sum();

            AssertSql(
                @":p_0='10'

SELECT SUM(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
) ""t""");
        }

        public override void Select_skip_average()
        {
            base.Select_skip_average();

            AssertSql(
                @":p_0='10'

SELECT AVG(CAST(""t"".""OrderID"" float))
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_count()
        {
            base.Select_skip_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_orderBy_skip_count()
        {
            base.Select_orderBy_skip_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_long_count()
        {
            base.Select_skip_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_orderBy_skip_long_count()
        {
            base.Select_orderBy_skip_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_max()
        {
            base.Select_skip_max();

            AssertSql(
                @":p_0='10'

SELECT MAX(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_min()
        {
            base.Select_skip_min();

            AssertSql(
                @":p_0='10'

SELECT MIN(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_sum()
        {
            base.Select_skip_sum();

            AssertSql(
                @":p_0='10'

SELECT SUM(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_distinct_average()
        {
            base.Select_distinct_average();

            AssertSql(
                @"SELECT AVG(CAST(""t"".""OrderID"" float))
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Select_distinct_count()
        {
            base.Select_distinct_count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_distinct_long_count()
        {
            base.Select_distinct_long_count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_distinct_max()
        {
            base.Select_distinct_max();

            AssertSql(
                @"SELECT MAX(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Select_distinct_min()
        {
            base.Select_distinct_min();

            AssertSql(
                @"SELECT MIN(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Select_distinct_sum()
        {
            base.Select_distinct_sum();

            AssertSql(
                @"SELECT SUM(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Comparing_to_fixed_string_parameter()
        {
            base.Comparing_to_fixed_string_parameter();

            AssertSql(
                @":prefix_0='A' (Size = 5)

SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE (""c"".""CustomerID"" LIKE :prefix_0 + N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(:prefix_0)) = :prefix_0)) OR (:prefix_0 = N'')");
        }

        public override void Comparing_entities_using_Equals()
        {
            base.Comparing_entities_using_Equals();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE (""c1"".""CustomerID"" LIKE N'ALFKI' || N'%' AND (SUBSTR(""c1"".""CustomerID"", 1, LENGTH(N'ALFKI')) = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")
ORDER BY ""Id1""");
        }

        public override void Comparing_different_entity_types_using_Equals()
        {
            base.Comparing_different_entity_types_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE (""c"".""CustomerID"" = N' ALFKI') AND (""o"".""CustomerID"" = N'ALFKI')");
        }

        public override void Comparing_entity_to_null_using_Equals()
        {
            base.Comparing_entity_to_null_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID""");
        }

        public override void Comparing_navigations_using_Equals()
        {
            base.Comparing_navigations_using_Equals();

            AssertSql(
                @"SELECT ""o1"".""OrderID"" ""Id1"", ""o2"".""OrderID"" ""Id2""
FROM ""Orders"" ""o1""
CROSS JOIN ""Orders"" ""o2""
WHERE (""o1"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o1"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""o1"".""CustomerID"" = ""o2"".""CustomerID"") OR (""o1"".""CustomerID"" IS NULL AND ""o2"".""CustomerID"" IS NULL))
ORDER BY ""Id1"", ""Id2""");
        }

        public override void Comparing_navigations_using_static_Equals()
        {
            base.Comparing_navigations_using_static_Equals();

            AssertSql(
                @"SELECT ""o1"".""OrderID"" ""Id1"", ""o2"".""OrderID"" ""Id2""
FROM ""Orders"" ""o1""
CROSS JOIN ""Orders"" ""o2""
WHERE (""o1"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o1"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""o1"".""CustomerID"" = ""o2"".""CustomerID"") OR (""o1"".""CustomerID"" IS NULL AND ""o2"".""CustomerID"" IS NULL))
ORDER BY ""Id1"", ""Id2""");
        }

        public override void Comparing_non_matching_entities_using_Equals()
        {
            base.Comparing_non_matching_entities_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"" ""Id2"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Comparing_non_matching_collection_navigations_using_Equals()
        {
            base.Comparing_non_matching_collection_navigations_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""o"".""OrderID"" ""Id2""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE 0 = 1");
        }

        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NULL");
        }

        public override void Comparing_collection_navigation_to_null_complex()
        {
            base.Comparing_collection_navigation_to_null_complex();

            AssertSql(
                @"SELECT ""od"".""ProductID"", ""od"".""OrderID""
FROM ""Order Details"" ""od""
INNER JOIN ""Orders"" ""od.Order"" ON ""od"".""OrderID"" = ""od.Order"".""OrderID""
WHERE (""od"".""OrderID"" < 10250) AND ""od.Order"".""CustomerID"" IS NOT NULL
ORDER BY ""od"".""OrderID"", ""od"".""ProductID""");
        }

        public override void Compare_collection_navigation_with_itself()
        {
            base.Compare_collection_navigation_with_itself();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE (""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND (""c"".""CustomerID"" = ""c"".""CustomerID"")");
        }

        public override void Compare_two_collection_navigations_with_different_query_sources()
        {
            base.Compare_two_collection_navigations_with_different_query_sources();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE ((""c1"".""CustomerID"" = N'ALFKI') AND (""c2"".""CustomerID"" = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")");
        }

        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE ((""c1"".""CustomerID"" = N'ALFKI') AND (""c2"".""CustomerID"" = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")");
        }

        public override void Compare_two_collection_navigations_with_different_property_chains()
        {
            base.Compare_two_collection_navigations_with_different_property_chains();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""o"".""OrderID"" ""Id2""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE (""c"".""CustomerID"" = N'ALFKI') AND (""c"".""CustomerID"" = ""o"".""CustomerID"")
ORDER BY ""Id1"", ""Id2""");
        }

        public override void OrderBy_ThenBy_same_column_different_direction()
        {
            base.OrderBy_ThenBy_same_column_different_direction();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID""");
        }

        public override void OrderBy_OrderBy_same_column_different_direction()
        {
            base.OrderBy_OrderBy_same_column_different_direction();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID"" DESC");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected.Select(s => s.Replace("\r\n", "\n")).ToArray());

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
