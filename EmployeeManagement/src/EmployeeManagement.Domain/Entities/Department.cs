// ============================================================================
// Department — a core domain entity representing an organizational unit
// ============================================================================
//
// A Department groups employees into organizational units such as "Human
// Resources", "Engineering", or "Finance". The Department entity enforces
// its own business rules: a name is always required, names are trimmed,
// and a department can be activated or deactivated.
//
// This entity lives in the Domain layer, which means it has no dependencies
// on databases, web frameworks, or external libraries. All validation and
// state changes happen through methods that protect the entity's invariants.
// ============================================================================

namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Represents an organizational department that employees can belong to.
/// All property changes go through methods that enforce business rules,
/// so callers cannot put the entity into an invalid state.
/// </summary>
public class Department
{
    // -----------------------------------------------------------------------
    // Id — the unique identifier for this department.
    // The setter is private because only the persistence layer (EF Core)
    // assigns the Id when the entity is saved to the database. Application
    // code should never change an existing department's Id.
    // -----------------------------------------------------------------------
    public int Id { get; private set; }

    // -----------------------------------------------------------------------
    // Name — the department's display name (e.g., "Engineering").
    // Private setter prevents direct assignment. Changes go through
    // UpdateName(), which validates the new value before applying it.
    // -----------------------------------------------------------------------
    public string Name { get; private set; } = string.Empty;

    // -----------------------------------------------------------------------
    // Description — an optional longer explanation of the department's role.
    // Nullable because not every department needs a description.
    // -----------------------------------------------------------------------
    public string? Description { get; private set; }

    // -----------------------------------------------------------------------
    // IsActive — indicates whether the department is currently in use.
    // New departments are active by default. Deactivating a department
    // is a soft-disable mechanism; it does not delete the department.
    // -----------------------------------------------------------------------
    public bool IsActive { get; private set; } = true;

    // -----------------------------------------------------------------------
    // Constructor — creates a new department with a validated name.
    //
    // The constructor is the first line of defense: it ensures that no
    // Department object can ever exist without a valid name. This is a
    // core principle of object-oriented design — objects are responsible
    // for their own validity.
    //
    // Parameters:
    //   name        — required department name; trimmed and validated.
    //   description — optional description; may be null or empty.
    //
    // Throws:
    //   ArgumentException — if name is null, empty, or whitespace.
    // -----------------------------------------------------------------------
    public Department(string name, string? description = null)
    {
        // ValidateName throws if the value is invalid, so we know that
        // after this line, name is a non-empty, trimmed string.
        Name = ValidateName(name);
        Description = description?.Trim();
    }

    // -----------------------------------------------------------------------
    // UpdateName — changes the department's name after validating it.
    //
    // Without this method, callers would need a public setter on Name,
    // which would allow invalid values like "" or "   ". By requiring
    // all name changes to go through this method, the business rule
    // "a department must always have a valid name" is always enforced.
    //
    // Throws:
    //   ArgumentException — if newName is null, empty, or whitespace.
    // -----------------------------------------------------------------------
    public void UpdateName(string newName)
    {
        Name = ValidateName(newName);
    }

    // -----------------------------------------------------------------------
    // UpdateDescription — changes the department's description.
    //
    // Descriptions are optional, so this method accepts null or empty
    // values. It still trims whitespace to keep stored values clean.
    // -----------------------------------------------------------------------
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    // -----------------------------------------------------------------------
    // Activate — marks the department as active.
    //
    // An active department can accept new employee assignments. This method
    // is idempotent: calling it on an already-active department has no
    // effect, which simplifies calling code.
    // -----------------------------------------------------------------------
    public void Activate()
    {
        IsActive = true;
    }

    // -----------------------------------------------------------------------
    // Deactivate — marks the department as inactive.
    //
    // An inactive department signals that it is no longer in use. Existing
    // employees may still reference it, but new assignments should be
    // prevented at the application layer.
    // -----------------------------------------------------------------------
    public void Deactivate()
    {
        IsActive = false;
    }

    // -----------------------------------------------------------------------
    // ValidateName — shared validation logic for the department name.
    //
    // This private static method is called by both the constructor and
    // UpdateName to ensure the same rule is applied everywhere. Keeping
    // validation in one place prevents inconsistencies.
    //
    // Returns the trimmed name if valid.
    // Throws ArgumentException if the name is null, empty, or whitespace.
    // -----------------------------------------------------------------------
    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Department name cannot be null, empty, or whitespace.",
                nameof(name));
        }

        // Trim whitespace so that " Engineering " becomes "Engineering".
        // This prevents subtle bugs where two departments appear different
        // but only differ by surrounding spaces.
        return name.Trim();
    }
}
