// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleRelationalCommandBuilderFactory : RelationalCommandBuilderFactory
    {
        public OracleRelationalCommandBuilderFactory(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(logger, typeMapper)
        {
        }

        protected override IRelationalCommandBuilder CreateCore(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            IRelationalTypeMapper relationalTypeMapper)
            => new OracleRelationalCommandBuilder(logger, relationalTypeMapper);

        private class OracleRelationalCommandBuilder : RelationalCommandBuilder
        {
            public OracleRelationalCommandBuilder(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMapper typeMapper)
                : base(logger, typeMapper)
            {
            }

            protected override IRelationalCommand BuildCore(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters)
                => new OracleRelationalCommand(logger, commandText, parameters);

            private class OracleRelationalCommand : RelationalCommand
            {
                public OracleRelationalCommand(
                    IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                    string commandText,
                    IReadOnlyList<IRelationalParameter> parameters)
                    : base(logger, commandText, parameters)
                {
                }

                public override RelationalDataReader ExecuteReader(
                    IRelationalConnection connection,
                    IReadOnlyDictionary<string, object> parameterValues)
                    => new RelationalDataReaderDecorator(
                        base.ExecuteReader(connection, AdjustParameters(parameterValues)));

                private static IReadOnlyDictionary<string, object> AdjustParameters(
                    IReadOnlyDictionary<string, object> parameterValues)
                {
                    if (parameterValues.Count == 0)
                    {
                        return parameterValues;
                    }

                    return parameterValues.ToDictionary(
                        kv => kv.Key,
                        kv =>
                            {
                                var type = kv.Value?.GetType();

                                if (type != null && type == typeof(bool))
                                {
                                    var b = (bool)kv.Value;

                                    return b ? 1 : 0;
                                }

                                return kv.Value;
                            });
                }

                public override async Task<RelationalDataReader> ExecuteReaderAsync(
                    IRelationalConnection connection,
                    IReadOnlyDictionary<string, object> parameterValues,
                    CancellationToken cancellationToken = new CancellationToken())
                    => new RelationalDataReaderDecorator(
                        await base.ExecuteReaderAsync(connection, AdjustParameters(parameterValues), cancellationToken));

                private class RelationalDataReaderDecorator : RelationalDataReader
                {
                    private readonly RelationalDataReader _relationalDataReader;

                    public RelationalDataReaderDecorator(RelationalDataReader relationalDataReader)
                        : base(new DbDataReaderDecorator(relationalDataReader.DbDataReader))
                        => _relationalDataReader = relationalDataReader;

                    public override bool Read() => _relationalDataReader.Read();

                    public override Task<bool> ReadAsync(CancellationToken cancellationToken = new CancellationToken())
                        => _relationalDataReader.ReadAsync(cancellationToken);

                    public override void Dispose() => _relationalDataReader.Dispose();

                    private class DbDataReaderDecorator : DbDataReader
                    {
                        private readonly DbDataReader _reader;

                        public DbDataReaderDecorator(DbDataReader reader)
                        {
                            _reader = reader;
                        }

                        protected override void Dispose(bool disposing)
                        {
                            _reader.Dispose();

                            base.Dispose(disposing);
                        }

                        public override void Close()
                        {
                            _reader.Close();
                        }

                        public override string GetDataTypeName(int ordinal)
                        {
                            return _reader.GetDataTypeName(ordinal);
                        }

                        public override IEnumerator GetEnumerator()
                        {
                            return _reader.GetEnumerator();
                        }

                        public override Type GetFieldType(int ordinal)
                        {
                            return _reader.GetFieldType(ordinal);
                        }

                        public override string GetName(int ordinal)
                        {
                            return _reader.GetName(ordinal);
                        }

                        public override int GetOrdinal(string name)
                        {
                            return _reader.GetOrdinal(name);
                        }

                        public override DataTable GetSchemaTable()
                        {
                            return _reader.GetSchemaTable();
                        }

                        public override bool GetBoolean(int ordinal)
                        {
                            return _reader.GetInt32(ordinal) == 1;
                        }

                        public override byte GetByte(int ordinal)
                        {
                            return _reader.GetByte(ordinal);
                        }

                        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                        {
                            return _reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
                        }

                        public override char GetChar(int ordinal)
                        {
                            return _reader.GetChar(ordinal);
                        }

                        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                        {
                            return _reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
                        }

                        public override DateTime GetDateTime(int ordinal)
                        {
                            return _reader.GetDateTime(ordinal);
                        }

                        public override decimal GetDecimal(int ordinal)
                        {
                            return _reader.GetDecimal(ordinal);
                        }

                        public override double GetDouble(int ordinal)
                        {
                            return _reader.GetDouble(ordinal);
                        }

                        public override float GetFloat(int ordinal)
                        {
                            return _reader.GetFloat(ordinal);
                        }

                        public override Guid GetGuid(int ordinal)
                        {
                            return _reader.GetGuid(ordinal);
                        }

                        public override short GetInt16(int ordinal)
                        {
                            return _reader.GetInt16(ordinal);
                        }

                        public override int GetInt32(int ordinal)
                        {
                            return _reader.GetInt32(ordinal);
                        }

                        public override long GetInt64(int ordinal)
                        {
                            return _reader.GetInt64(ordinal);
                        }

                        public override Type GetProviderSpecificFieldType(int ordinal)
                        {
                            return _reader.GetProviderSpecificFieldType(ordinal);
                        }

                        public override object GetProviderSpecificValue(int ordinal)
                        {
                            return _reader.GetProviderSpecificValue(ordinal);
                        }

                        public override int GetProviderSpecificValues(object[] values)
                        {
                            return _reader.GetProviderSpecificValues(values);
                        }

                        public override string GetString(int ordinal)
                        {
                            return _reader.GetString(ordinal);
                        }

                        public override Stream GetStream(int ordinal)
                        {
                            return _reader.GetStream(ordinal);
                        }

                        public override TextReader GetTextReader(int ordinal)
                        {
                            return _reader.GetTextReader(ordinal);
                        }

                        public override object GetValue(int ordinal)
                        {
                            return _reader.GetValue(ordinal);
                        }

                        public override T GetFieldValue<T>(int ordinal)
                        {
                            return _reader.GetFieldValue<T>(ordinal);
                        }

                        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
                        {
                            return _reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
                        }

                        public override int GetValues(object[] values)
                        {
                            return _reader.GetValues(values);
                        }

                        public override bool IsDBNull(int ordinal)
                        {
                            return _reader.IsDBNull(ordinal);
                        }

                        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
                        {
                            return _reader.IsDBNullAsync(ordinal, cancellationToken);
                        }

                        public override bool NextResult()
                        {
                            return _reader.NextResult();
                        }

                        public override bool Read()
                        {
                            return _reader.Read();
                        }

                        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
                        {
                            return _reader.ReadAsync(cancellationToken);
                        }

                        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
                        {
                            return _reader.NextResultAsync(cancellationToken);
                        }

                        public override int Depth => _reader.Depth;

                        public override int FieldCount => _reader.FieldCount;

                        public override bool HasRows => _reader.HasRows;

                        public override bool IsClosed => _reader.IsClosed;

                        public override int RecordsAffected => _reader.RecordsAffected;

                        public override int VisibleFieldCount => _reader.VisibleFieldCount;

                        public override object this[int ordinal] => _reader[ordinal];

                        public override object this[string name] => _reader[name];
                    }
                }
            }
        }
    }
}
