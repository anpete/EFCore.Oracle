// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleQuerySqlGenerator : DefaultQuerySqlGenerator, IOracleExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression)
            : base(dependencies, selectExpression)
        {
        }

        protected override string TypedTrueLiteral => "1";
        protected override string TypedFalseLiteral => "0";

        protected override string GenerateOperator(Expression expression)
            => expression.NodeType == ExpressionType.Add
               && expression.Type == typeof(string)
                ? " || "
                : base.GenerateOperator(expression);

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.And:
                {
                    Sql.Append("BITAND(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(")");

                    return binaryExpression;
                }
                case ExpressionType.Or:
                {
                    Visit(binaryExpression.Left);

                    Sql.Append(" - BITAND(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(") + ");

                    Visit(binaryExpression.Right);

                    return binaryExpression;
                }
                case ExpressionType.Modulo:
                {
                    Sql.Append("MOD(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(")");

                    return binaryExpression;
                    }
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // Not supported
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                Sql.AppendLine().Append("FETCH FIRST ");

                Visit(selectExpression.Limit);

                Sql.Append(" ROWS ONLY");
            }
            else
            {
                base.GenerateLimitOffset(selectExpression);
            }
        }

        public override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                Sql.AppendLine("(");

                subQueryIndent = Sql.Indent();
            }

            Sql.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                Sql.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            var projectionAdded = false;

            if (selectExpression.IsProjectStar)
            {
                var tableAlias = selectExpression.ProjectStarTable.Alias;

                Sql
                    .Append(SqlGenerator.DelimitIdentifier(tableAlias))
                    .Append(".*");

                projectionAdded = true;
            }

            if (selectExpression.Projection.Any())
            {
                if (selectExpression.IsProjectStar)
                {
                    Sql.Append(", ");
                }

                ProcessExpressionList(selectExpression.Projection, GenerateProjection);

                projectionAdded = true;
            }

            if (!projectionAdded)
            {
                Sql.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                Sql.AppendLine()
                    .Append("FROM ");

                ProcessExpressionList(selectExpression.Tables, sql => sql.AppendLine());
            }
            else
            {
                Sql.Append(" FROM DUAL");
            }

            if (selectExpression.Predicate != null)
            {
                GeneratePredicate(selectExpression.Predicate);
            }

            if (selectExpression.OrderBy.Any())
            {
                Sql.AppendLine();

                GenerateOrderBy(selectExpression.OrderBy);
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                Sql.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    Sql.Append(" ")
                        .Append(SqlGenerator.DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        private void ProcessExpressionList(
            IReadOnlyList<Expression> expressions, Action<IRelationalCommandBuilder> joinAction = null)
            => ProcessExpressionList(expressions, e => Visit(e), joinAction);

        private void ProcessExpressionList<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(Sql);
                }

                itemAction(items[i]);
            }
        }

        public override Expression VisitAlias(AliasExpression aliasExpression)
        {
            Check.NotNull(aliasExpression, nameof(aliasExpression));

            Visit(aliasExpression.Expression);

            if (aliasExpression.Alias != null)
            {
                Sql.Append(" ");
            }

            if (aliasExpression.Alias != null)
            {
                Sql.Append(SqlGenerator.DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        public override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (tableExpression.Schema != null)
            {
                Sql.Append(SqlGenerator.DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            Sql.Append(SqlGenerator.DelimitIdentifier(tableExpression.Table))
                .Append(" ")
                .Append(SqlGenerator.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitCrossJoinLateral(CrossJoinLateralExpression crossJoinLateralExpression)
        {
            Check.NotNull(crossJoinLateralExpression, nameof(crossJoinLateralExpression));

            Sql.Append("CROSS APPLY ");

            Visit(crossJoinLateralExpression.TableExpression);

            return crossJoinLateralExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            switch (sqlFunctionExpression.FunctionName)
            {
                case "EXTRACT":
                {
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" FROM ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;
                }
                case "CAST":
                {
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" AS ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;
                }
                case "AVG" when sqlFunctionExpression.Type == typeof(decimal):
                {
                    Sql.Append("CAST(");

                    base.VisitSqlFunction(sqlFunctionExpression);

                    Sql.Append(" AS DECIMAL(29,4))");

                    return sqlFunctionExpression;
                }
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        protected override void GenerateProjection(Expression projection)
        {
            var aliasedProjection = projection as AliasExpression;
            var expressionToProcess = aliasedProjection?.Expression ?? projection;
            var updatedExperssion = ExplicitCastToBool(expressionToProcess);

            expressionToProcess = aliasedProjection != null
                ? new AliasExpression(aliasedProjection.Alias, updatedExperssion)
                : updatedExperssion;

            base.GenerateProjection(expressionToProcess);
        }

        private static Expression ExplicitCastToBool(Expression expression)
        {
            return (expression as BinaryExpression)?.NodeType == ExpressionType.Coalesce
                   && expression.Type.UnwrapNullableType() == typeof(bool)
                ? new ExplicitCastExpression(expression, expression.Type)
                : expression;
        }
    }
}
