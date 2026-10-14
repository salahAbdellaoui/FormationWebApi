// ============================================================================
// DepartmentConfiguration — EF Core mapping for the Department entity
// ============================================================================
//
// This class tells EF Core how the Department entity maps to the Departments
// table in SQL Server. It defines column types, constraints, and keys that
// go beyond what EF Core can infer from the C# types alone.
//
// Using IEntityTypeConfiguration<T> keeps mapping logic in separate files
// instead of piling everything into OnModelCreating. Each entity gets its
// own configuration class, which is easier to read and maintain.
// ============================================================================

using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the mapping between the <see cref="Department"/> entity
/// and the Departments table in SQL Server.
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // -------------------------------------------------------------------
        // Table name — explicit mapping to "Departments".
        // EF Core would default to "Departments" anyway (DbSet property name),
        // but stating it explicitly makes the mapping clear.
        // -------------------------------------------------------------------
        builder.ToTable("Departments");

        // -------------------------------------------------------------------
        // Primary key — Id is the primary key.
        // ValueGeneratedOnAdd tells EF Core that the database generates
        // the value (IDENTITY in SQL Server). EF Core will not send a
        // value for Id when inserting a new row; SQL Server assigns it.
        // -------------------------------------------------------------------
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd();

        // -------------------------------------------------------------------
        // Name — required string with a maximum length.
        // Without a maximum length, SQL Server would use nvarchar(max),
        // which is less efficient for indexing and allows unreasonably
        // long values. 100 characters is generous for a department name.
        // -------------------------------------------------------------------
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        // -------------------------------------------------------------------
        // Description — optional string with a maximum length.
        // IsRequired(false) is the default for nullable properties, but
        // stating it explicitly makes the intent clear to readers.
        // 500 characters allows a meaningful description without being
        // unbounded.
        // -------------------------------------------------------------------
        builder.Property(d => d.Description)
            .HasMaxLength(500);

        // -------------------------------------------------------------------
        // IsActive — boolean flag.
        // EF Core maps C# bool to SQL Server bit by convention. No extra
        // configuration is needed, but we include it here for completeness
        // so that all columns are visible in one place.
        // -------------------------------------------------------------------
        builder.Property(d => d.IsActive)
            .IsRequired();
    }
}
