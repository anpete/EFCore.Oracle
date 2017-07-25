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
            = new OracleStringTypeMapping("nvarchar(max)", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _keyUnicodeString
            = new OracleStringTypeMapping("nvarchar(450)", dbType: null, unicode: true, size: 450);

        private readonly OracleStringTypeMapping _unboundedAnsiString
            = new OracleStringTypeMapping("varchar(max)", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _keyAnsiString
            = new OracleStringTypeMapping("varchar(900)", dbType: DbType.AnsiString, unicode: false, size: 900);

        private readonly OracleByteArrayTypeMapping _unboundedBinary
            = new OracleByteArrayTypeMapping("varbinary(max)");

        private readonly OracleByteArrayTypeMapping _keyBinary
            = new OracleByteArrayTypeMapping("varbinary(900)", dbType: DbType.Binary, size: 900);

        private readonly OracleByteArrayTypeMapping _rowversion
            = new OracleByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

        private readonly IntTypeMapping _int = new IntTypeMapping("int", DbType.Int32);

        private readonly LongTypeMapping _long = new LongTypeMapping("bigint", DbType.Int64);

        private readonly ShortTypeMapping _short = new ShortTypeMapping("smallint", DbType.Int16);

        private readonly ByteTypeMapping _byte = new ByteTypeMapping("tinyint", DbType.Byte);

        private readonly BoolTypeMapping _bool = new BoolTypeMapping("bit");

        private readonly OracleStringTypeMapping _fixedLengthUnicodeString
            = new OracleStringTypeMapping("nchar", dbType: DbType.String, unicode: true);

        private readonly OracleStringTypeMapping _variableLengthUnicodeString
            = new OracleStringTypeMapping("nvarchar", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _fixedLengthAnsiString
            = new OracleStringTypeMapping("char", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _variableLengthAnsiString
            = new OracleStringTypeMapping("varchar", dbType: DbType.AnsiString);

        private readonly OracleByteArrayTypeMapping _variableLengthBinary = new OracleByteArrayTypeMapping("varbinary");

        private readonly OracleByteArrayTypeMapping _fixedLengthBinary = new OracleByteArrayTypeMapping("binary");

        private readonly OracleDateTimeTypeMapping _date = new OracleDateTimeTypeMapping("date", dbType: DbType.Date);

        private readonly OracleDateTimeTypeMapping _datetime = new OracleDateTimeTypeMapping("datetime", dbType: DbType.DateTime);

        private readonly OracleDateTimeTypeMapping _datetime2 = new OracleDateTimeTypeMapping("datetime2", dbType: DbType.DateTime2);

        private readonly DoubleTypeMapping _double = new OracleDoubleTypeMapping("float"); // Note: "float" is correct Oracle type to map to CLR-type double

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset = new OracleDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly FloatTypeMapping _real = new OracleFloatTypeMapping("real"); // Note: "real" is correct Oracle type to map to CLR-type float

        private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("uniqueidentifier", DbType.Guid);

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("decimal(18, 2)");

        private readonly TimeSpanTypeMapping _time = new OracleTimeSpanTypeMapping("time");

        private readonly OracleStringTypeMapping _xml = new OracleStringTypeMapping("xml", dbType: null, unicode: true);

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
                    { "datetime2", _datetime2 },
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
                    { "nvarchar", _variableLengthUnicodeString },
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
                    { "varchar", _variableLengthAnsiString },
                    { "xml", _xml }
                };

            // Note: sbyte, ushort, uint, char and ulong type mappings are not supported by Oracle.
            // We would need the type conversions feature to allow this to work - see https://github.com/aspnet/EntityFramework/issues/242.
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime2 },
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
                    "nvarchar",
                    "varbinary",
                    "varchar"
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
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 4000,
                    defaultUnicodeMapping: _unboundedUnicodeString,
                    unboundedUnicodeMapping: _unboundedUnicodeString,
                    keyUnicodeMapping: _keyUnicodeString,
                    createBoundedUnicodeMapping: size => new OracleStringTypeMapping(
                        "nvarchar(" + size + ")",
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
