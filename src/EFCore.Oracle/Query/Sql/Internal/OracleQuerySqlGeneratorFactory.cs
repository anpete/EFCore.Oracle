// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public class OracleQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        private readonly IOracleOptions _sqlServerOptions;

        public OracleQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] IOracleOptions sqlServerOptions)
            : base(dependencies)
        {
            _sqlServerOptions = sqlServerOptions;
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new OracleQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)));
    }
}
