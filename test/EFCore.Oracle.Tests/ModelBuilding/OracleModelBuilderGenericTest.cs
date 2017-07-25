// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class OracleModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class OracleGenericInheritance : GenericInheritance
        {
            [Fact] // #7240
            public void Can_use_shadow_FK_that_collides_with_convention_shadow_FK_on_other_derived_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Child>();
                modelBuilder.Entity<Parent>()
                    .HasOne(p => p.A)
                    .WithOne()
                    .HasForeignKey<DisjointChildSubclass1>("ParentId");

                modelBuilder.Validate();

                var property1 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty("ParentId");
                Assert.True(property1.IsForeignKey());
                Assert.Equal("ParentId", property1.Oracle().ColumnName);
                var property2 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty("ParentId");
                Assert.True(property2.IsForeignKey());
                Assert.Equal("DisjointChildSubclass2_ParentId", property2.Oracle().ColumnName);
            }

            public class Parent
            {
                public int Id { get; set; }
                public DisjointChildSubclass1 A { get; set; }
                public IList<DisjointChildSubclass2> B { get; set; }
            }

            public abstract class Child
            {
                public int Id { get; set; }
            }

            public class DisjointChildSubclass1 : Child { }

            public class DisjointChildSubclass2 : Child { }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(OracleTestHelpers.Instance);
        }

        public class OracleGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(OracleTestHelpers.Instance);
        }

        public class OracleGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(OracleTestHelpers.Instance);
        }

        public class OracleGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(OracleTestHelpers.Instance);
        }

        public class OracleGenericOwnedTypes : GenericOwnedTypes
        {
            [Fact]
            public virtual void Owned_types_use_table_splitting()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var bookOwnershipBuilder2 = modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel);
                var bookLabel2OwnershipBuilder1 = bookOwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);
                var bookOwnershipBuilder1 = modelBuilder.Entity<Book>().OwnsOne(b => b.Label);
                var bookLabel1OwnershipBuilder2 = bookOwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;

                // Only owned types have the table name set
                Assert.Equal(book.Oracle().TableName, bookOwnership1.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookOwnership2.DeclaringEntityType.Oracle().TableName);
                Assert.NotEqual(book.Oracle().TableName, bookLabel1Ownership1.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel1Ownership2.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel2Ownership1.DeclaringEntityType.Oracle().TableName);
                Assert.NotEqual(book.Oracle().TableName, bookLabel2Ownership2.DeclaringEntityType.Oracle().TableName);

                var bookLabel1OwnershipBuilder1 = bookOwnershipBuilder1.OwnsOne(l => l.AnotherBookLabel);
                var bookLabel2OwnershipBuilder2 = bookOwnershipBuilder2.OwnsOne(l => l.SpecialBookLabel);
                bookLabel1OwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);
                bookLabel1OwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);
                bookLabel2OwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);
                bookLabel2OwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);

                modelBuilder.Validate();

                Assert.Equal(book.Oracle().TableName, bookOwnership1.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookOwnership2.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel1Ownership1.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel1Ownership2.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel2Ownership1.DeclaringEntityType.Oracle().TableName);
                Assert.Equal(book.Oracle().TableName, bookLabel2Ownership2.DeclaringEntityType.Oracle().TableName);

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

                Assert.Equal(nameof(Book.Label) + "_" + nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).Oracle().ColumnName);
                Assert.Equal(nameof(Book.AlternateLabel) + "_" + nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).Oracle().ColumnName);

                bookOwnershipBuilder1.ToTable("Label");
                bookOwnershipBuilder2.ToTable("AlternateLabel");

                Assert.Equal(nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).Oracle().ColumnName);
                Assert.Equal(nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).Oracle().ColumnName);
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(OracleTestHelpers.Instance);
        }
    }
}
