// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class OracleValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = OracleTestHelpers.Instance.CreateConventionBuilder().Model;

            Assert.Equal(1, model.GetAnnotations().Count());

            Assert.Equal(OracleAnnotationNames.ValueGenerationStrategy, model.GetAnnotations().Single().Name);
            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, model.GetAnnotations().Single().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = OracleTestHelpers.Instance.CreateConventionBuilder()
                .ForOracleUseSequenceHiLo()
                .Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name);
            Assert.Equal(3, annotations.Count());

            Assert.Equal(
                RelationalAnnotationNames.SequencePrefix +
                "." +
                OracleModelAnnotations.DefaultHiLoSequenceName,
                annotations.ElementAt(2).Name);
            
            Assert.NotNull(annotations.ElementAt(2).Value);

            Assert.Equal(OracleAnnotationNames.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(OracleModelAnnotations.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(OracleAnnotationNames.ValueGenerationStrategy, annotations.ElementAt(1).Name);
            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(1).Value);
        }
    }
}
