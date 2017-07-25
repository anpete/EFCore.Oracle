// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleEntityTypeAnnotations : RelationalEntityTypeAnnotations, IOracleEntityTypeAnnotations
    {
        public OracleEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public OracleEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool IsMemoryOptimized
        {
            get => Annotations.Metadata[OracleAnnotationNames.MemoryOptimized] as bool? ?? false;
            set => SetIsMemoryOptimized(value);
        }

        protected virtual bool SetIsMemoryOptimized(bool value)
            => Annotations.SetAnnotation(OracleAnnotationNames.MemoryOptimized, value);
    }
}
