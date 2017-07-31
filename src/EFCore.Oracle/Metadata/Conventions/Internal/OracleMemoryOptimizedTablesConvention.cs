// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class OracleMemoryOptimizedTablesConvention : IEntityTypeAnnotationChangedConvention, IKeyAddedConvention, IIndexAddedConvention
    {
        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == OracleAnnotationNames.MemoryOptimized)
            {
                var memoryOptimized = annotation?.Value as bool? == true;
                foreach (var key in entityTypeBuilder.Metadata.GetDeclaredKeys())
                {
                    key.Builder.Oracle(ConfigurationSource.Convention).IsClustered(memoryOptimized ? false : (bool?)null);
                }
                foreach (var index in entityTypeBuilder.Metadata.GetDerivedIndexesInclusive())
                {
                    index.Builder.Oracle(ConfigurationSource.Convention).IsClustered(memoryOptimized ? false : (bool?)null);
                }
            }

            return annotation;
        }

        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            if (keyBuilder.Metadata.DeclaringEntityType.Oracle().IsMemoryOptimized)
            {
                keyBuilder.Oracle(ConfigurationSource.Convention).IsClustered(false);
            }

            return keyBuilder;
        }

        public virtual InternalIndexBuilder Apply(InternalIndexBuilder indexBuilder)
        {
            if (indexBuilder.Metadata.DeclaringEntityType.GetAllBaseTypesInclusive().Any(et => et.Oracle().IsMemoryOptimized))
            {
                indexBuilder.Oracle(ConfigurationSource.Convention).IsClustered(false);
            }

            return indexBuilder;
        }
    }
}
