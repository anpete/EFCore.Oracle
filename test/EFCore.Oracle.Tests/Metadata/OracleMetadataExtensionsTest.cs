// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.Oracle().ColumnName);
            Assert.Equal("Name", ((IProperty)property).Oracle().ColumnName);

            property.Relational().ColumnName = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().ColumnName);
            Assert.Equal("Eman", property.Oracle().ColumnName);
            Assert.Equal("Eman", ((IProperty)property).Oracle().ColumnName);

            property.Oracle().ColumnName = "MyNameIs";

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.Relational().ColumnName);
            Assert.Equal("MyNameIs", property.Oracle().ColumnName);
            Assert.Equal("MyNameIs", ((IProperty)property).Oracle().ColumnName);

            property.Oracle().ColumnName = null;

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", property.Relational().ColumnName);
            Assert.Equal("Name", property.Oracle().ColumnName);
            Assert.Equal("Name", ((IProperty)property).Oracle().ColumnName);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = GetModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Oracle().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).Oracle().TableName);

            entityType.Relational().TableName = "Customizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Customizer", entityType.Oracle().TableName);
            Assert.Equal("Customizer", ((IEntityType)entityType).Oracle().TableName);

            entityType.Oracle().TableName = "Custardizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Oracle().TableName);
            Assert.Equal("Custardizer", ((IEntityType)entityType).Oracle().TableName);

            entityType.Oracle().TableName = null;

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customer", entityType.Relational().TableName);
            Assert.Equal("Customer", entityType.Oracle().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).Oracle().TableName);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = GetModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.Oracle().Schema);
            Assert.Null(((IEntityType)entityType).Oracle().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.Oracle().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Oracle().Schema);

            entityType.Oracle().Schema = "dbOh";

            Assert.Equal("dbOh", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.Oracle().Schema);
            Assert.Equal("dbOh", ((IEntityType)entityType).Oracle().Schema);

            entityType.Oracle().Schema = null;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.Oracle().Schema);
            Assert.Null(((IEntityType)entityType).Oracle().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.Oracle().ColumnType);
            Assert.Null(((IProperty)property).Oracle().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.Oracle().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Oracle().ColumnType);

            property.Oracle().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(verstappen)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(verstappen)", property.Oracle().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).Oracle().ColumnType);

            property.Oracle().ColumnType = null;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.Oracle().ColumnType);
            Assert.Null(((IProperty)property).Oracle().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Null(((IProperty)property).Oracle().DefaultValueSql);

            property.Relational().DefaultValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultValueSql);
            Assert.Equal("newsequentialid()", property.Oracle().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Oracle().DefaultValueSql);

            property.Oracle().DefaultValueSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().DefaultValueSql);
            Assert.Equal("expressyourself()", property.Oracle().DefaultValueSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Oracle().DefaultValueSql);

            property.Oracle().DefaultValueSql = null;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Null(((IProperty)property).Oracle().DefaultValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_computed_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
            Assert.Null(((IProperty)property).Oracle().ComputedColumnSql);

            property.Relational().ComputedColumnSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().ComputedColumnSql);
            Assert.Equal("newsequentialid()", property.Oracle().ComputedColumnSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Oracle().ComputedColumnSql);

            property.Oracle().ComputedColumnSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().ComputedColumnSql);
            Assert.Equal("expressyourself()", property.Oracle().ComputedColumnSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Oracle().ComputedColumnSql);

            property.Oracle().ComputedColumnSql = null;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
            Assert.Null(((IProperty)property).Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Can_get_and_set_column_default_value()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.ByteArray)
                .Metadata;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Null(((IProperty)property).Oracle().DefaultValue);

            property.Relational().DefaultValue = new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.Oracle().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, ((IProperty)property).Oracle().DefaultValue);

            property.Oracle().DefaultValue = new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.Oracle().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, ((IProperty)property).Oracle().DefaultValue);

            property.Oracle().DefaultValue = null;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Null(((IProperty)property).Oracle().DefaultValue);
        }

        [Theory]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(OraclePropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(OraclePropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(OraclePropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(OraclePropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(OraclePropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(OraclePropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        public void Metadata_throws_when_setting_conflicting_serverGenerated_values(string firstConfiguration, string secondConfiguration)
        {
            var modelBuilder = GetModelBuilder();

            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt);

            ConfigureProperty(propertyBuilder.Metadata, firstConfiguration, "1");

            Assert.Equal(RelationalStrings.ConflictingColumnServerGeneration(secondConfiguration, nameof(Customer.NullableInt), firstConfiguration),
                Assert.Throws<InvalidOperationException>(() =>
                    ConfigureProperty(propertyBuilder.Metadata, secondConfiguration, "2")).Message);
        }

        protected virtual void ConfigureProperty(IMutableProperty property, string configuration, string value)
        {
            var propertyAnnotations = property.Oracle();
            switch (configuration)
            {
                case nameof(RelationalPropertyAnnotations.DefaultValue):
                    propertyAnnotations.DefaultValue = int.Parse(value);
                    break;
                case nameof(RelationalPropertyAnnotations.DefaultValueSql):
                    propertyAnnotations.DefaultValueSql = value;
                    break;
                case nameof(RelationalPropertyAnnotations.ComputedColumnSql):
                    propertyAnnotations.ComputedColumnSql = value;
                    break;
                case nameof(OraclePropertyAnnotations.ValueGenerationStrategy):
                    propertyAnnotations.ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.Oracle().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Oracle().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", key.Oracle().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Oracle().Name);

            key.Oracle().Name = "PrimarySchool";

            Assert.Equal("PrimarySchool", key.Relational().Name);
            Assert.Equal("PrimarySchool", key.Oracle().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).Oracle().Name);

            key.Oracle().Name = null;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.Oracle().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Oracle().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .HasOne<Customer>()
                .WithOne()
                .HasForeignKey<Order>(e => e.CustomerId)
                .Metadata;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "KFC";

            Assert.Equal("KFC", foreignKey.Relational().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);

            index.Oracle().Name = "DexKnows";

            Assert.Equal("DexKnows", index.Relational().Name);
            Assert.Equal("DexKnows", ((IIndex)index).Relational().Name);

            index.Oracle().Name = null;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_filter()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.Oracle().Filter);
            Assert.Null(((IIndex)index).Oracle().Filter);

            index.Relational().Name = "Generic expression";

            Assert.Equal("Generic expression", index.Relational().Name);
            Assert.Equal("Generic expression", index.Oracle().Name);
            Assert.Equal("Generic expression", ((IIndex)index).Oracle().Name);

            index.Oracle().Name = "Oracle-specific expression";

            Assert.Equal("Oracle-specific expression", index.Relational().Name);
            Assert.Equal("Oracle-specific expression", index.Oracle().Name);
            Assert.Equal("Oracle-specific expression", ((IIndex)index).Oracle().Name);

            index.Oracle().Name = null;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.Oracle().Filter);
            Assert.Null(((IIndex)index).Oracle().Filter);
        }

        [Fact]
        public void Can_get_and_set_index_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.Oracle().IsClustered);
            Assert.Null(((IIndex)index).Oracle().IsClustered);

            index.Oracle().IsClustered = true;

            Assert.True(index.Oracle().IsClustered.Value);
            Assert.True(((IIndex)index).Oracle().IsClustered.Value);

            index.Oracle().IsClustered = null;

            Assert.Null(index.Oracle().IsClustered);
            Assert.Null(((IIndex)index).Oracle().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_key_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Null(key.Oracle().IsClustered);
            Assert.Null(((IKey)key).Oracle().IsClustered);

            key.Oracle().IsClustered = true;

            Assert.True(key.Oracle().IsClustered.Value);
            Assert.True(((IKey)key).Oracle().IsClustered.Value);

            key.Oracle().IsClustered = null;

            Assert.Null(key.Oracle().IsClustered);
            Assert.Null(((IKey)key).Oracle().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo"));
            Assert.Null(model.Oracle().FindSequence("Foo"));
            Assert.Null(((IModel)model).Oracle().FindSequence("Foo"));

            var sequence = model.Oracle().GetOrAddSequence("Foo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", model.Oracle().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Oracle().FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo"));

            var sequence2 = model.Oracle().FindSequence("Foo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo", "Smoo"));
            Assert.Null(model.Oracle().FindSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Oracle().FindSequence("Foo", "Smoo"));

            var sequence = model.Oracle().GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.Oracle().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Oracle().FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo", "Smoo"));

            var sequence2 = model.Oracle().FindSequence("Foo", "Smoo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci");
            model.Oracle().GetOrAddSequence("Golomb");

            var sequences = model.Oracle().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
        }

        [Fact]
        public void Can_get_multiple_sequences_when_overridden()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci").StartValue = 1;
            model.Oracle().GetOrAddSequence("Fibonacci").StartValue = 3;
            model.Oracle().GetOrAddSequence("Golomb");

            var sequences = model.Oracle().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Golomb");

            var sequence = sequences.FirstOrDefault(s => s.Name == "Fibonacci");
            Assert.NotNull(sequence);
            Assert.Equal(3, sequence.StartValue);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, model.Oracle().ValueGenerationStrategy);

            model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, model.Oracle().ValueGenerationStrategy);

            model.Oracle().ValueGenerationStrategy = null;

            Assert.Null(model.Oracle().ValueGenerationStrategy);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Oracle().HiLoSequenceName);
            Assert.Null(((IModel)model).Oracle().HiLoSequenceName);

            model.Oracle().HiLoSequenceName = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty.Snook", ((IModel)model).Oracle().HiLoSequenceName);

            model.Oracle().HiLoSequenceName = null;

            Assert.Null(model.Oracle().HiLoSequenceName);
            Assert.Null(((IModel)model).Oracle().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Oracle().HiLoSequenceSchema);
            Assert.Null(((IModel)model).Oracle().HiLoSequenceSchema);

            model.Oracle().HiLoSequenceSchema = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.Oracle().HiLoSequenceSchema);
            Assert.Equal("Tasty.Snook", ((IModel)model).Oracle().HiLoSequenceSchema);

            model.Oracle().HiLoSequenceSchema = null;

            Assert.Null(model.Oracle().HiLoSequenceSchema);
            Assert.Null(((IModel)model).Oracle().HiLoSequenceSchema);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.Model.Oracle().ValueGenerationStrategy = null;

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, ((IProperty)property).Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.Oracle().ValueGenerationStrategy = null;

            Assert.Null(property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_nullable_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt)
                .Metadata;

            Assert.Null(property.Oracle().ValueGenerationStrategy);

            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, ((IProperty)property).Oracle().ValueGenerationStrategy);

            property.Oracle().ValueGenerationStrategy = null;

            Assert.Null(property.Oracle().ValueGenerationStrategy);
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                OracleStrings.SequenceBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                OracleStrings.IdentityBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn).Message);
        }

        [Fact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Oracle().HiLoSequenceName);
            Assert.Null(((IProperty)property).Oracle().HiLoSequenceName);

            property.Oracle().HiLoSequenceName = "Snook";

            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Snook", ((IProperty)property).Oracle().HiLoSequenceName);

            property.Oracle().HiLoSequenceName = null;

            Assert.Null(property.Oracle().HiLoSequenceName);
            Assert.Null(((IProperty)property).Oracle().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Oracle().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).Oracle().HiLoSequenceSchema);

            property.Oracle().HiLoSequenceSchema = "Tasty";

            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);
            Assert.Equal("Tasty", ((IProperty)property).Oracle().HiLoSequenceSchema);

            property.Oracle().HiLoSequenceSchema = null;

            Assert.Null(property.Oracle().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).Oracle().HiLoSequenceSchema);
        }

        [Fact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw");

            Assert.Null(property.Oracle().FindHiLoSequence());
            Assert.Null(((IProperty)property).Oracle().FindHiLoSequence());

            property.Oracle().HiLoSequenceName = "DaneelOlivaw";

            Assert.Null(property.Oracle().FindHiLoSequence());
            Assert.Null(((IProperty)property).Oracle().FindHiLoSequence());

            modelBuilder.Model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn;

            Assert.Null(property.Oracle().FindHiLoSequence());
            Assert.Null(((IProperty)property).Oracle().FindHiLoSequence());

            modelBuilder.Model.Oracle().ValueGenerationStrategy = null;
            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn;

            Assert.Null(property.Oracle().FindHiLoSequence());
            Assert.Null(((IProperty)property).Oracle().FindHiLoSequence());
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw");
            property.Oracle().HiLoSequenceName = "DaneelOlivaw";
            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;
            property.Oracle().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Oracle().HiLoSequenceName = "DaneelOlivaw";
            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.Oracle().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw", "R");
            property.Oracle().HiLoSequenceName = "DaneelOlivaw";
            property.Oracle().HiLoSequenceSchema = "R";
            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
            Assert.Equal("R", property.Oracle().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Oracle().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;
            property.Oracle().HiLoSequenceName = "DaneelOlivaw";
            property.Oracle().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
            Assert.Equal("R", property.Oracle().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Oracle().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Oracle().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.Oracle().HiLoSequenceSchema = "R";
            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
            Assert.Equal("R", property.Oracle().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Oracle().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Oracle().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.Oracle().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.Oracle().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.Oracle().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Oracle().FindHiLoSequence().Name);
            Assert.Equal("R", property.Oracle().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Oracle().FindHiLoSequence().Schema);
        }

        private static ModelBuilder GetModelBuilder() => OracleTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
            public string Name { get; set; }
            public byte Byte { get; set; }
            public byte? NullableByte { get; set; }
            public byte[] ByteArray { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
