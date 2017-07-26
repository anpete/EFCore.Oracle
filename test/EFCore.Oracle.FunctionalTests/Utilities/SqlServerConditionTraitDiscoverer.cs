// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class OracleConditionTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var sqlServerCondition = (traitAttribute as IReflectionAttributeInfo)?.Attribute as OracleConditionAttribute;
            if (sqlServerCondition == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return Enum.GetValues(typeof(OracleCondition)).Cast<OracleCondition>()
                .Where(c => sqlServerCondition.Conditions.HasFlag(c))
                .Select(c => new KeyValuePair<string, string>(nameof(OracleCondition), c.ToString()));
        }
    }
}
