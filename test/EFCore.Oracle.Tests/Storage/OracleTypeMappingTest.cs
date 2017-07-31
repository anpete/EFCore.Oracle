// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class OracleTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand() => new OracleCommand();

        protected override DbType DefaultParameterType
            => DbType.Int32;

        [Fact]
        public override void GenerateSqlLiteral_returns_ByteArray_literal()
        {
            var value = new byte[] { 0xDA, 0x7A };
            var literal = new OracleTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(byte[])).GenerateSqlLiteral(value);
            Assert.Equal("0xDA7A", literal);
        }

        [Fact]
        public override void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new OracleTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTime)).GenerateSqlLiteral(value);

            Assert.Equal("TO_DATE('2015-03-12 13:36:37', 'YYYY-MM-DD HH24:MI:SS')", literal);
        }

        [Fact]
        public override void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new OracleTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTimeOffset)).GenerateSqlLiteral(value);

            Assert.Equal("'2015-03-12T13:36:37.371-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteralValue_returns_Unicode_String_literal()
        {
            var literal = new OracleTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping("nvarchar2").GenerateSqlLiteral("A Unicode String");
            
            Assert.Equal("N'A Unicode String'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteralValue_returns_NonUnicode_String_literal()
        {
            var literal = new OracleTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping("varchar2").GenerateSqlLiteral("A Non-Unicode String");
            
            Assert.Equal("'A Non-Unicode String'", literal);
        }
    }
}
