// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class TestRelationalTransaction : IDbContextTransaction, IInfrastructure<DbTransaction>
    {
        private readonly IDbContextTransaction _realTransaction;
        private readonly TestSqlServerConnection _testConnection;
        private bool _connectionClosed;

        public TestRelationalTransaction(
            TestSqlServerConnection connection, DbTransaction transaction, ILoggerFactory loggerFactory, DiagnosticSource diagnosticSource, bool transactionOwned)
            : this(
                connection,
                new RelationalTransaction(
                    connection,
                    transaction,
                    new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                        loggerFactory, new LoggingOptions(), diagnosticSource),
                    transactionOwned))
        {
        }

        public virtual Guid TransactionId { get; } = Guid.NewGuid();

        public TestRelationalTransaction(TestSqlServerConnection connection, IDbContextTransaction transaction)
        {
            _testConnection = connection;
            _realTransaction = transaction;
        }

        public void Dispose()
        {
            _realTransaction.Dispose();

            ClearTransaction();
        }

        public void Commit()
        {
            if (_testConnection.CommitFailures.Count > 0)
            {
                var fail = _testConnection.CommitFailures.Dequeue();
                if (fail.HasValue)
                {
                    if (fail.Value)
                    {
                        _realTransaction.GetDbTransaction().Rollback();
                    }
                    else
                    {
                        _realTransaction.GetDbTransaction().Commit();
                    }
                    _testConnection.DbConnection.Close();
                    throw SqlExceptionFactory.CreateSqlException(_testConnection.ErrorNumber);
                }
            }

            _realTransaction.Commit();

            ClearTransaction();
        }

        public void Rollback()
        {
            _realTransaction.Rollback();

            ClearTransaction();
        }

        private void ClearTransaction()
        {
            _testConnection.UseTransaction(null);

            if (!_connectionClosed)
            {
                _connectionClosed = true;

                _testConnection.Close();
            }
        }

        public DbTransaction Instance => _realTransaction.GetDbTransaction();
    }
}
