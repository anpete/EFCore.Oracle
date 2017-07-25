// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleInternalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.True(builder.Oracle(ConfigurationSource.Convention).ValueGenerationStrategy(OracleValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, builder.Metadata.Oracle().ValueGenerationStrategy);

            Assert.True(builder.Oracle(ConfigurationSource.DataAnnotation).ValueGenerationStrategy(OracleValueGenerationStrategy.IdentityColumn));
            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, builder.Metadata.Oracle().ValueGenerationStrategy);

            Assert.False(builder.Oracle(ConfigurationSource.Convention).ValueGenerationStrategy(OracleValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, builder.Metadata.Oracle().ValueGenerationStrategy);

            Assert.Equal(1, builder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(OracleAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.Oracle(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.Oracle().TableName);

            Assert.True(typeBuilder.Oracle(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.Oracle().TableName);

            Assert.False(typeBuilder.Oracle(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.Oracle().TableName);

            Assert.Equal(1, typeBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(RelationalAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.Oracle(ConfigurationSource.Convention).HiLoSequenceName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.Oracle().HiLoSequenceName);

            Assert.True(propertyBuilder.Oracle(ConfigurationSource.DataAnnotation).HiLoSequenceName("Splow"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Oracle().HiLoSequenceName);

            Assert.False(propertyBuilder.Oracle(ConfigurationSource.Convention).HiLoSequenceName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Oracle().HiLoSequenceName);

            Assert.Equal(1, propertyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(OracleAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type_only_with_explicit()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Name", typeof(string), ConfigurationSource.Convention);

            Assert.False(propertyBuilder.Oracle(ConfigurationSource.Convention)
                .ValueGenerationStrategy(OracleValueGenerationStrategy.SequenceHiLo));

            Assert.Equal(
                OracleStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.Oracle(ConfigurationSource.Explicit).ValueGenerationStrategy(OracleValueGenerationStrategy.SequenceHiLo)).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type_only_with_explicit()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Name", typeof(string), ConfigurationSource.Convention);

            Assert.False(propertyBuilder.Oracle(ConfigurationSource.Convention)
                .ValueGenerationStrategy(OracleValueGenerationStrategy.IdentityColumn));

            Assert.Equal(
                OracleStrings.IdentityBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.Oracle(ConfigurationSource.Explicit).ValueGenerationStrategy(OracleValueGenerationStrategy.IdentityColumn)).Message);
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var idProperty = entityTypeBuilder.Property("Id", typeof(string), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty.Name }, ConfigurationSource.Convention);

            Assert.True(keyBuilder.Oracle(ConfigurationSource.Convention).IsClustered(true));
            Assert.True(keyBuilder.Metadata.Oracle().IsClustered);

            Assert.True(keyBuilder.Oracle(ConfigurationSource.DataAnnotation).IsClustered(false));
            Assert.False(keyBuilder.Metadata.Oracle().IsClustered);

            Assert.False(keyBuilder.Oracle(ConfigurationSource.Convention).IsClustered(true));
            Assert.False(keyBuilder.Metadata.Oracle().IsClustered);

            Assert.Equal(1, keyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(OracleAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(indexBuilder.Oracle(ConfigurationSource.Convention).IsClustered(true));
            Assert.True(indexBuilder.Metadata.Oracle().IsClustered);

            Assert.True(indexBuilder.Oracle(ConfigurationSource.DataAnnotation).IsClustered(false));
            Assert.False(indexBuilder.Metadata.Oracle().IsClustered);

            Assert.False(indexBuilder.Oracle(ConfigurationSource.Convention).IsClustered(true));
            Assert.False(indexBuilder.Metadata.Oracle().IsClustered);

            Assert.Equal(1, indexBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(OracleAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.HasForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.Oracle(ConfigurationSource.Convention).HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.Relational().Name);

            Assert.True(relationshipBuilder.Oracle(ConfigurationSource.DataAnnotation).HasConstraintName("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);

            Assert.False(relationshipBuilder.Oracle(ConfigurationSource.Convention).HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);

            Assert.Equal(1, relationshipBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(RelationalAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
