// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class OracleEntityTypeBuilderAnnotations : OracleEntityTypeAnnotations
    {
        public OracleEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder, ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        public virtual bool ToSchema([CanBeNull] string name)
            => SetSchema(Check.NullButNotEmpty(name, nameof(name)));

        public virtual bool ToTable([CanBeNull] string name)
            => SetTableName(Check.NullButNotEmpty(name, nameof(name)));

        public virtual bool ToTable([CanBeNull] string name, [CanBeNull] string schema)
        {
            var originalTable = TableName;
            if (!SetTableName(Check.NullButNotEmpty(name, nameof(name))))
            {
                return false;
            }

            if (!SetSchema(Check.NullButNotEmpty(schema, nameof(schema))))
            {
                SetTableName(originalTable);
                return false;
            }

            return true;
        }

#pragma warning disable 109

        public new virtual bool IsMemoryOptimized(bool value) => SetIsMemoryOptimized(value);
#pragma warning restore 109
    }
}
