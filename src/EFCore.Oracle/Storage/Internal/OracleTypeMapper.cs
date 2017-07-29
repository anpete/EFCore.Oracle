// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleTypeMapper : RelationalTypeMapper
    {
        private readonly OracleStringTypeMapping _unboundedUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2(4000)", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _keyUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2(450)", dbType: null, unicode: true, size: 450);

        private readonly OracleStringTypeMapping _unboundedAnsiString
            = new OracleStringTypeMapping("VARCHAR2", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _keyAnsiString
            = new OracleStringTypeMapping("VARCHAR2(900)", dbType: DbType.AnsiString, unicode: false, size: 900);

        private readonly OracleByteArrayTypeMapping _unboundedBinary
            = new OracleByteArrayTypeMapping("LOB");

        private readonly OracleByteArrayTypeMapping _keyBinary
            = new OracleByteArrayTypeMapping("RAW(900)", dbType: DbType.Binary, size: 900);

        private readonly OracleByteArrayTypeMapping _rowversion
            = new OracleByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

        private readonly IntTypeMapping _int = new IntTypeMapping("NUMBER(10)", DbType.Int32);

        private readonly LongTypeMapping _long = new LongTypeMapping("NUMBER(19)", DbType.Int64);

        private readonly ShortTypeMapping _short = new ShortTypeMapping("NUMBER(6)", DbType.Int16);

        private readonly ByteTypeMapping _byte = new ByteTypeMapping("NUMBER(3)", DbType.Byte);

        private readonly BoolTypeMapping _bool = new BoolTypeMapping("INTEGER");

        private readonly OracleStringTypeMapping _fixedLengthUnicodeString
            = new OracleStringTypeMapping("NCHAR", dbType: DbType.String, unicode: true);

        private readonly OracleStringTypeMapping _variableLengthUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _fixedLengthAnsiString
            = new OracleStringTypeMapping("CHAR", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _variableLengthAnsiString
            = new OracleStringTypeMapping("VARCHAR2", dbType: DbType.AnsiString);

        private readonly OracleByteArrayTypeMapping _variableLengthBinary = new OracleByteArrayTypeMapping("BLOB");

        private readonly OracleByteArrayTypeMapping _fixedLengthBinary = new OracleByteArrayTypeMapping("RAW");

        private readonly OracleDateTimeTypeMapping _date = new OracleDateTimeTypeMapping("DATE", dbType: DbType.Date);

        private readonly OracleDateTimeTypeMapping _datetime = new OracleDateTimeTypeMapping("TIMESTAMP", dbType: DbType.DateTime);

        private readonly DoubleTypeMapping _double = new OracleDoubleTypeMapping("FLOAT(49)"); 

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset = new OracleDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

        private readonly FloatTypeMapping _real = new OracleFloatTypeMapping("FLOAT(23)"); 

        private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("RAW(16)", DbType.Guid);

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("DECIMAL(29,4)");

        private readonly TimeSpanTypeMapping _time = new OracleTimeSpanTypeMapping("INTERVAL");

        private readonly OracleStringTypeMapping _xml = new OracleStringTypeMapping("XML", dbType: null, unicode: true);

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "bit", _bool },
                    { "char varying", _variableLengthAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "date", _date },
                    { "datetime", _datetime },
                    { "datetimeoffset", _datetimeoffset },
                    { "dec", _decimal },
                    { "decimal", _decimal },
                    { "float", _double },
                    { "image", _variableLengthBinary },
                    { "int", _int },
                    { "money", _decimal },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _variableLengthUnicodeString },
                    { "numeric", _decimal },
                    { "NVARCHAR2", _variableLengthUnicodeString },
                    { "real", _real },
                    { "rowversion", _rowversion },
                    { "smalldatetime", _datetime },
                    { "smallint", _short },
                    { "smallmoney", _decimal },
                    { "text", _variableLengthAnsiString },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _byte },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _variableLengthBinary },
                    { "VARCHAR2", _variableLengthAnsiString },
                    { "xml", _xml }
                };

            // Note: sbyte, ushort, uint, char and ulong type mappings are not supported by Oracle.
            // We would need the type conversions feature to allow this to work - see https://github.com/aspnet/EntityFramework/issues/242.
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time }
                };

            // These are disallowed only if specified without any kind of length specified in parenthesis.
            // This is because we don't try to make a new type from this string and any max length value
            // specified in the model, which means use of these strings is almost certainly an error, and
            // if it is not an error, then using, for example, varbinary(1) will work instead.
            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "binary varying",
                    "binary",
                    "char varying",
                    "char",
                    "character varying",
                    "character",
                    "national char varying",
                    "national character varying",
                    "national character",
                    "nchar",
                    "varbinary"
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    maxBoundedLength: 8000,
                    defaultMapping: _unboundedBinary,
                    unboundedMapping: _unboundedBinary,
                    keyMapping: _keyBinary,
                    rowVersionMapping: _rowversion,
                    createBoundedMapping: size => new OracleByteArrayTypeMapping(
                        "varbinary(" + size + ")",
                        DbType.Binary,
                        size));

            StringMapper
                = new StringRelationalTypeMapper(
                    maxBoundedAnsiLength: 8000,
                    defaultAnsiMapping: _unboundedAnsiString,
                    unboundedAnsiMapping: _unboundedAnsiString,
                    keyAnsiMapping: _keyAnsiString,
                    createBoundedAnsiMapping: size => new OracleStringTypeMapping(
                        "VARCHAR2(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 4000,
                    defaultUnicodeMapping: _unboundedUnicodeString,
                    unboundedUnicodeMapping: _unboundedUnicodeString,
                    keyUnicodeMapping: _keyUnicodeString,
                    createBoundedUnicodeMapping: size => new OracleStringTypeMapping(
                        "NVARCHAR2(" + size + ")",
                        dbType: null,
                        unicode: true,
                        size: size));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IStringRelationalTypeMapper StringMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException(OracleStrings.UnqualifiedDataType(storeType));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetColumnType(IProperty property) => property.Oracle().ColumnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? _unboundedUnicodeString
                : (clrType == typeof(byte[])
                    ? _unboundedBinary
                    : base.FindMapping(clrType));
        }

        // Indexes in Oracle have a max size of 900 bytes
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
