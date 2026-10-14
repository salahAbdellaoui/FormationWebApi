// ============================================================================
// EmployeeConfiguration — EF Core mapping for the Employee entity
// ============================================================================
//
// This class defines how the Employee entity maps to the Employees table.
// It configures column types, the foreign key relationship to Department,
// the unique email index, and the delete behavior that protects referential
// integrity.
//
// Key decisions:
// - Salary uses decimal(18, 2) for monetary precision.
// - Email has a unique index to enforce uniqueness at the database level,
//   since the Domain entity intentionally does not check uniqueness.
// - The foreign key uses Restrict delete behavior to prevent deleting a
//   department that still has employees assigned to it.
// ============================================================================

using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the mapping between the <see cref="Employee"/> entity
/// and the Employees table in SQL Server.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // -------------------------------------------------------------------
        // Table name
        // -------------------------------------------------------------------
        builder.ToTable("Employees");

        // -------------------------------------------------------------------
        // Primary key — Id is database-generated (IDENTITY).
        // -------------------------------------------------------------------
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // -------------------------------------------------------------------
        // Required string properties with maximum lengths.
        //
        // Without explicit lengths, SQL Server would create nvarchar(max)
        // columns, which are wasteful for names and emails and cannot be
        // indexed efficiently. These limits are generous enough for real
        // data while keeping the schema practical.
        // -------------------------------------------------------------------
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        // -------------------------------------------------------------------
        // PhoneNumber — optional, with a reasonable maximum length.
        // -------------------------------------------------------------------
        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(30);

        // -------------------------------------------------------------------
        // HireDate — maps to datetime2 in SQL Server.
        // datetime2 provides better precision than the older datetime type
        // and is the recommended type for date/time values in SQL Server.
        // -------------------------------------------------------------------
        builder.Property(e => e.HireDate)
            .HasColumnType("datetime2")
            .IsRequired();

        // -------------------------------------------------------------------
        // Salary — decimal with precision (18, 2).
        //
        // Precision 18 means up to 18 total digits. Scale 2 means 2 digits
        // after the decimal point. This supports salaries up to
        // 9,999,999,999,999,999.99 — more than sufficient for any role.
        //
        // Using decimal (not float or real) avoids floating-point rounding
        // errors that would make salary calculations unreliable.
        // -------------------------------------------------------------------
        builder.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // -------------------------------------------------------------------
        // DepartmentId — foreign key to the Departments table.
        //
        // The Domain entity does not have a navigation property to
        // Department, and we do not add one just for EF Core. Instead,
        // we configure the relationship using the foreign key alone.
        // EF Core will create the FK constraint in the database without
        // requiring a C# navigation property.
        // -------------------------------------------------------------------
        builder.Property(e => e.DepartmentId)
            .IsRequired();

        // -------------------------------------------------------------------
        // Relationship — Employee belongs to Department.
        //
        // WithMany().WithOne() defines the one-to-many relationship without
        // adding navigation properties to the Domain entities. The
        // HasForeignKey call specifies which property holds the FK value.
        //
        // DeleteBehavior.Restrict — if a department has employees, the
        // database will reject attempts to delete that department. This
        // prevents orphaned employee records. The application must
        // reassign or remove employees before deleting a department.
        // -------------------------------------------------------------------
        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------------------
        // Unique index on Email.
        //
        // The Domain entity does not enforce email uniqueness because a
        // single entity cannot check against the database. Instead, the
        // database enforces it through this unique index. If application
        // code tries to insert a duplicate email, SQL Server will reject
        // the operation with a unique constraint violation.
        //
        // Note: SQL Server's default collation determines whether this
        // index treats "Alice@Example.com" and "alice@example.com" as
        // the same value. The default SQL Server collation is typically
        // case-insensitive (CI), but this depends on server configuration.
        // If case-sensitive uniqueness is required, the collation must be
        // configured explicitly — this is left to a later phase.
        // -------------------------------------------------------------------
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_Employees_Email");

        // -------------------------------------------------------------------
        // IsActive
        // -------------------------------------------------------------------
        builder.Property(e => e.IsActive)
            .IsRequired();
    }
}
