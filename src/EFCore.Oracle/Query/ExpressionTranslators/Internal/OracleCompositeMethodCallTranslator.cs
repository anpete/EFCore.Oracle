// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new OracleContainsOptimizedTranslator(),
            new OracleConvertTranslator(),
            new OracleDateAddTranslator(),
            new OracleEndsWithOptimizedTranslator(),
            new OracleMathTranslator(),
            new OracleNewGuidTranslator(),
            new OracleObjectToStringTranslator(),
            new OracleStartsWithOptimizedTranslator(),
            new OracleStringIsNullOrWhiteSpaceTranslator(),
            new OracleStringReplaceTranslator(),
            new OracleStringSubstringTranslator(),
            new OracleStringToLowerTranslator(),
            new OracleStringToUpperTranslator(),
            new OracleStringTrimEndTranslator(),
            new OracleStringTrimStartTranslator(),
            new OracleStringTrimTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
