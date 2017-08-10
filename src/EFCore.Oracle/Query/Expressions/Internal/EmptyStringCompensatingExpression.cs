// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    public class EmptyStringCompensatingExpression : Expression
    {
        private readonly Expression _expression;
        private readonly ParameterExpression _parameterExpression;
        private readonly Expression _compensatedExpression;

        public EmptyStringCompensatingExpression(
            [NotNull] Expression expression,
            [NotNull] ParameterExpression maybeEmptyStringExpression,
            [NotNull] Expression compensatedExpression)
        {
            _expression = expression;
            _parameterExpression = maybeEmptyStringExpression;
            _compensatedExpression = compensatedExpression;
        }

        public virtual Expression Compensate([NotNull] IReadOnlyDictionary<string, object> parameterValues)
        {
            if (parameterValues.TryGetValue(_parameterExpression.Name, out var value)
                && (string)value == string.Empty)
            {
                return _compensatedExpression;
            }

            return _expression;
        }

        public override Type Type => _expression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => false;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as IOracleExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitEmptyStringCompensating(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newExpression = visitor.Visit(_expression);
            var newCompensatedExpression = visitor.Visit(_compensatedExpression);

            return !ReferenceEquals(newExpression, _expression)
                   || !ReferenceEquals(_compensatedExpression, newCompensatedExpression)
                ? new EmptyStringCompensatingExpression(newExpression, _parameterExpression, newCompensatedExpression)
                : this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((EmptyStringCompensatingExpression)obj);
        }

        private bool Equals([NotNull] EmptyStringCompensatingExpression other)
            => Equals(_expression, other._expression)
               && Equals(_parameterExpression, other._parameterExpression)
               && Equals(_compensatedExpression, other._compensatedExpression);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _expression.GetHashCode();
                hashCode = (hashCode * 397) ^ _parameterExpression.GetHashCode();
                hashCode = (hashCode * 397) ^ _compensatedExpression.GetHashCode();
                return hashCode;
            }
        }
    }
}
