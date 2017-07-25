// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Eman");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("MyNameIs");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.Relational().ColumnName);
            Assert.Equal("MyNameIs", property.Oracle().ColumnName);
        }

        [Fact]
        public void Can_set_column_type()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("nvarchar(42)");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("nvarchar(DA)");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("nvarchar(DA)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.Oracle().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("VanillaCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("CherryCoke");

            Assert.Equal("CherryCoke", property.Relational().DefaultValueSql);
            Assert.Equal("CherryCoke", property.Oracle().DefaultValueSql);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_expression_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedNever()
                .HasDefaultValueSql("VanillaCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("CherryCoke");

            Assert.Equal("CherryCoke", property.Relational().DefaultValueSql);
            Assert.Equal("CherryCoke", property.Oracle().DefaultValueSql);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_column_computed_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasComputedColumnSql("VanillaCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasComputedColumnSql("CherryCoke");

            Assert.Equal("CherryCoke", property.Relational().ComputedColumnSql);
            Assert.Equal("CherryCoke", property.Oracle().ComputedColumnSql);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_column_computed_expression_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedNever()
                .HasComputedColumnSql("VanillaCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasComputedColumnSql("CherryCoke");

            Assert.Equal("CherryCoke", property.Relational().ComputedColumnSql);
            Assert.Equal("CherryCoke", property.Oracle().ComputedColumnSql);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_column_default_value()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .HasDefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .HasDefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)));

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Offset");

            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.Oracle().DefaultValue);
        }

        [Fact]
        public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .HasDefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)));

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Offset");

            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.Oracle().DefaultValue);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_value_overrides_default_sql_and_computed_column_sql()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasComputedColumnSql("0")
                .HasDefaultValueSql("1")
                .HasDefaultValue(2);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal(2, property.Oracle().DefaultValue);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Setting_column_default_sql_overrides_default_value_and_computed_column_sql()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasComputedColumnSql("0")
                .HasDefaultValue(2)
                .HasDefaultValueSql("1");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal("1", property.Oracle().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Null(property.Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Setting_computed_column_sql_overrides_default_value_and_column_default_sql()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1")
                .HasDefaultValue(2)
                .HasComputedColumnSql("0");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal("0", property.Oracle().ComputedColumnSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValue);
        }

        [Fact]
        public void Setting_Oracle_default_sql_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1")
                .HasComputedColumnSql("0")
                .HasDefaultValue(2);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Equal(2, property.Relational().DefaultValue);
            Assert.Equal(2, property.Oracle().DefaultValue);
            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Setting_Oracle_default_value_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(2)
                .HasDefaultValueSql("1")
                .HasComputedColumnSql("0");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Equal("0", property.Relational().ComputedColumnSql);
            Assert.Equal("0", property.Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Setting_Oracle_computed_column_sql_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasComputedColumnSql("0")
                .HasDefaultValue(2)
                .HasDefaultValueSql("1");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Equal("1", property.Relational().DefaultValueSql);
            Assert.Equal("1", property.Oracle().DefaultValueSql);
        }

        [Fact]
        public void Setting_column_default_value_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(1);

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_value_sql_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_Oracle_identity_column_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseOracleIdentityColumn()
                .HasDefaultValue(1)
                .HasDefaultValueSql("1")
                .HasComputedColumnSql("0");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Oracle().DefaultValue);
            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Oracle().DefaultValueSql);
            Assert.Equal("0", property.Relational().ComputedColumnSql);
            Assert.Null(property.Oracle().ComputedColumnSql);
        }

        [Fact]
        public void Can_set_key_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .HasName("KeyLimePie")
                .HasName("LemonSupreme");

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.Equal("LemonSupreme", key.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName(null);

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName(null);

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasConstraintName(null);

            Assert.Equal("FK_OrderDetails_Order_OrderId", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasForeignKey<OrderDetails>(e => e.Id)
                .HasConstraintName("LemonSupreme")
                .HasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_index_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasName("Eeeendeeex")
                .HasName("Dexter");

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("Dexter", index.Relational().Name);
        }

        [Fact]
        public void Can_set_index_filter()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasFilter("Generic expression")
                .HasFilter("Oracle-specific expression");

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("Oracle-specific expression", index.Relational().Filter);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer")
                .ToTable("Custardizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Oracle().TableName);
        }

        [Fact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer")
                .ToTable("Custardizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Oracle().TableName);
        }

        [Fact]
        public void Can_set_table_and_schema_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer", "db0")
                .ToTable("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Oracle().TableName);
            Assert.Equal("dbOh", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.Oracle().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer", "db0")
                .ToTable("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Oracle().TableName);
            Assert.Equal("dbOh", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.Oracle().Schema);
        }

        [Fact]
        public void Can_set_MemoryOptimized()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForOracleIsMemoryOptimized();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.Oracle().IsMemoryOptimized);

            modelBuilder
                .Entity<Customer>()
                .ForOracleIsMemoryOptimized(false);

            Assert.False(entityType.Oracle().IsMemoryOptimized);
        }

        [Fact]
        public void Can_set_MemoryOptimized_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForOracleIsMemoryOptimized();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.Oracle().IsMemoryOptimized);

            modelBuilder
                .Entity(typeof(Customer))
                .ForOracleIsMemoryOptimized(false);

            Assert.False(entityType.Oracle().IsMemoryOptimized);
        }

        [Fact]
        public void Can_set_index_clustering()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .ForOracleIsClustered();

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.Oracle().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .ForOracleIsClustered();

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.True(key.Oracle().IsClustered.Value);
        }

        [Fact]
        public void Can_set_sequences_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForOracleUseSequenceHiLo();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal(OracleModelAnnotations.DefaultHiLoSequenceName, sqlServerExtensions.HiLoSequenceName);
            Assert.Null(sqlServerExtensions.HiLoSequenceSchema);

            Assert.NotNull(relationalExtensions.FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
            Assert.NotNull(sqlServerExtensions.FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForOracleUseSequenceHiLo("Snook");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.HiLoSequenceName);
            Assert.Null(sqlServerExtensions.HiLoSequenceSchema);

            Assert.NotNull(relationalExtensions.FindSequence("Snook"));

            var sequence = sqlServerExtensions.FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForOracleUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.HiLoSequenceSchema);

            Assert.NotNull(relationalExtensions.FindSequence("Snook", "Tasty"));

            var sequence = sqlServerExtensions.FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForOracleUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForOracleUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.FindSequence("Snook", "Tasty"));
        }

        private static void ValidateSchemaNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_set_identities_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForOracleUseIdentityColumns();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.Oracle();

            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.HiLoSequenceName);
            Assert.Null(sqlServerExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
            Assert.Null(sqlServerExtensions.FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Setting_Oracle_identities_for_model_is_lower_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>(eb =>
                    {
                        eb.Property(e => e.Id).HasDefaultValue(1);
                        eb.Property(e => e.Name).HasComputedColumnSql("Default");
                        eb.Property(e => e.Offset).HasDefaultValueSql("Now");
                    });

            modelBuilder.ForOracleUseIdentityColumns();

            var model = modelBuilder.Model;
            var idProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));
            Assert.Null(idProperty.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(1, idProperty.Relational().DefaultValue);
            Assert.Equal(1, idProperty.Oracle().DefaultValue);

            var nameProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Name));
            Assert.Null(nameProperty.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, nameProperty.ValueGenerated);
            Assert.Equal("Default", nameProperty.Relational().ComputedColumnSql);
            Assert.Equal("Default", nameProperty.Oracle().ComputedColumnSql);

            var offsetProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Offset));
            Assert.Null(offsetProperty.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, offsetProperty.ValueGenerated);
            Assert.Equal("Now", offsetProperty.Relational().DefaultValueSql);
            Assert.Equal("Now", offsetProperty.Oracle().DefaultValueSql);
        }

        [Fact]
        public void Can_set_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(OracleModelAnnotations.DefaultHiLoSequenceName, property.Oracle().HiLoSequenceName);

            Assert.NotNull(model.Relational().FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
            Assert.NotNull(model.Oracle().FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Null(property.Oracle().HiLoSequenceSchema);

            Assert.NotNull(model.Relational().FindSequence("Snook"));

            var sequence = model.Oracle().FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);

            Assert.NotNull(model.Oracle().FindSequence("Snook", "Tasty"));

            var sequence = model.Relational().FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222))
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForOracleUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.SequenceHiLo, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.Oracle().HiLoSequenceName);
            Assert.Equal("Tasty", property.Oracle().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_identities_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseOracleIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(OracleValueGenerationStrategy.IdentityColumn, property.Oracle().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Null(property.Oracle().HiLoSequenceName);

            Assert.Null(model.Relational().FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
            Assert.Null(model.Oracle().FindSequence(OracleModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_create_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasSequence("Snook");

            Assert.NotNull(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.Oracle().FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_create_schema_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasSequence("Snook", "Tasty");

            Assert.NotNull(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.Oracle().FindSequence("Snook", "Tasty");

            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            ValidateNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook"));
            ValidateNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook"));
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            ValidateNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook"));
            ValidateNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook"));
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            ValidateNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook"));
            ValidateNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook"));
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            ValidateNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook"));
            ValidateNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook"));
        }

        private static void ValidateNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b => { b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222); });

            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook", "Tasty", b => { b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222); });

            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(modelBuilder.Model.Oracle().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Oracle_entity_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToTable("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToTable("Jay", "Simon"));
        }

        [Fact]
        public void Oracle_entity_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Will");

            modelBuilder
                .Entity<Customer>()
                .ToTable("Jay", "Simon");
        }

        [Fact]
        public void Oracle_property_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasColumnName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasColumnType("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasDefaultValueSql("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasComputedColumnSql("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasDefaultValue("Neil"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForOracleUseSequenceHiLo());

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseOracleIdentityColumn());
        }

        [Fact]
        public void Oracle_property_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasColumnName("Will");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasColumnName("Jay");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasColumnType("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasColumnType("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasDefaultValueSql("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasDefaultValueSql("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasDefaultValue("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasDefaultValue("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasComputedColumnSql("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasComputedColumnSql("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .ForOracleUseSequenceHiLo();

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .ForOracleUseSequenceHiLo();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .UseOracleIdentityColumn();

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .UseOracleIdentityColumn();
        }

        [Fact]
        public void Oracle_relationship_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders)
                    .WithOne(e => e.Customer)
                    .HasConstraintName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Customer)
                    .WithMany(e => e.Orders)
                    .HasConstraintName("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Details)
                    .WithOne(e => e.Order)
                    .HasConstraintName("Simon"));
        }

        [Fact]
        public void Oracle_relationship_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(typeof(Order), "Orders")
                .WithOne("Customer")
                .HasConstraintName("Will");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasConstraintName("Jay");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Order)
                .HasConstraintName("Simon");
        }

        private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<string> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<int> _)
        {
        }

        private void AssertIsGeneric(ReferenceCollectionBuilder<Customer, Order> _)
        {
        }

        private void AssertIsGeneric(ReferenceReferenceBuilder<Order, OrderDetails> _)
        {
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return OracleTestHelpers.Instance.CreateConventionBuilder();
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTimeOffset Offset { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }
    }
}
