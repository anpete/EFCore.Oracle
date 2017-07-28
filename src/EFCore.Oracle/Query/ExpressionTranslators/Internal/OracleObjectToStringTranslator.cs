// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleObjectToStringTranslator : IMethodCallTranslator
    {
        private static readonly List<Type> _typeMapping
            = new List<Type>
            {
                typeof(int),
                typeof(long),
                typeof(DateTime),
                typeof(Guid),
                typeof(bool),
                typeof(byte),
                typeof(byte[]),
                typeof(double),
                typeof(DateTimeOffset),
                typeof(char),
                typeof(short),
                typeof(float),
                typeof(decimal),
                typeof(TimeSpan),
                typeof(uint),
                typeof(ushort),
                typeof(ulong),
                typeof(sbyte)
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(ToString)
                && methodCallExpression.Arguments.Count == 0
                && methodCallExpression.Object != null
                && _typeMapping.Contains(
                    methodCallExpression.Object.Type
                        .UnwrapNullableType()
                        .UnwrapEnumType()))
            {
                return new SqlFunctionExpression(
                    functionName: "TO_CHAR",
                    returnType: methodCallExpression.Type,
                    arguments: new[]
                    {
                        methodCallExpression.Object
                    });
            }

            return null;
        }
    }
}
