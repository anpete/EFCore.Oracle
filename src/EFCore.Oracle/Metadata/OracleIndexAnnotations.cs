// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleIndexAnnotations : RelationalIndexAnnotations, IOracleIndexAnnotations
    {
        public OracleIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        protected OracleIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.Metadata[OracleAnnotationNames.Clustered]; }
            [param: CanBeNull] set { SetIsClustered(value); }
        }

        protected virtual bool SetIsClustered(bool? value) => Annotations.SetAnnotation(
            OracleAnnotationNames.Clustered,
            value);
    }
}
