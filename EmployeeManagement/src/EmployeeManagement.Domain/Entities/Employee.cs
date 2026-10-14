// ============================================================================
// Employee — a core domain entity representing a person employed by the org
// ============================================================================
//
// An Employee is the central entity in this application. It represents a
// person who works for the organization, belongs to a department, and has
// employment details such as hire date and salary.
//
// This entity enforces its own business rules through its constructor and
// methods. No external code can put an Employee into an invalid state
// (e.g., negative salary, empty name, or zero department ID) because all
// property changes go through validated paths.
//
// The entity has no knowledge of databases, web requests, or DTOs. It is
// pure domain logic. This makes it easy to test, reuse, and reason about.
// ============================================================================

namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Represents a person employed by the organization.
/// All property changes go through methods that enforce business rules,
/// preventing callers from creating invalid employee records.
/// </summary>
public class Employee
{
    // -----------------------------------------------------------------------
    // Id — the unique identifier for this employee.
    // Private setter because only the persistence layer assigns the Id
    // after saving to the database. Application code never changes it.
    // -----------------------------------------------------------------------
    public int Id { get; private set; }

    // -----------------------------------------------------------------------
    // FirstName — the employee's first (given) name. Required and trimmed.
    // Private setter prevents direct assignment; use UpdatePersonalDetails().
    // -----------------------------------------------------------------------
    public string FirstName { get; private set; } = string.Empty;

    // -----------------------------------------------------------------------
    // LastName — the employee's last (family) name. Required and trimmed.
    // -----------------------------------------------------------------------
    public string LastName { get; private set; } = string.Empty;

    // -----------------------------------------------------------------------
    // Email — the employee's email address. Required and trimmed.
    // Note: uniqueness is not enforced here. Email uniqueness is an
    // application/persistence concern (checked against the database),
    // not a domain invariant that a single entity can verify alone.
    // -----------------------------------------------------------------------
    public string Email { get; private set; } = string.Empty;

    // -----------------------------------------------------------------------
    // PhoneNumber — the employee's phone number. Optional.
    // Stored as a string (not a numeric type) because phone numbers may
    // include country codes, extensions, or formatting characters.
    // -----------------------------------------------------------------------
    public string? PhoneNumber { get; private set; }

    // -----------------------------------------------------------------------
    // HireDate — the date the employee started working.
    // Uses DateTime to represent the hire date. No restriction on future
    // dates is enforced at this phase — a future hire date may represent
    // a scheduled start for a new hire.
    // -----------------------------------------------------------------------
    public DateTime HireDate { get; private set; }

    // -----------------------------------------------------------------------
    // Salary — the employee's salary. Must be zero or positive.
    // Uses decimal (not double or float) to avoid floating-point precision
    // errors when representing monetary values.
    // -----------------------------------------------------------------------
    public decimal Salary { get; private set; }

    // -----------------------------------------------------------------------
    // DepartmentId — the identifier of the department this employee belongs
    // to. Must be greater than zero, which ensures that an employee is
    // always assigned to a valid department reference (not 0, which is the
    // default int value and would indicate an unassigned employee).
    // -----------------------------------------------------------------------
    public int DepartmentId { get; private set; }

    // -----------------------------------------------------------------------
    // IsActive — indicates whether the employee is currently active.
    // Active employees are working; inactive employees have been deactivated
    // (e.g., terminated, on extended leave). Like Department, this is a
    // soft-disable mechanism, not a deletion.
    // -----------------------------------------------------------------------
    public bool IsActive { get; private set; } = true;

    // -----------------------------------------------------------------------
    // Constructor — creates a new employee with validated required fields.
    //
    // This constructor ensures that no Employee object can exist without
    // a valid first name, last name, email, hire date, salary, and
    // department assignment. Every required field is validated before
    // the object is created.
    //
    // Parameters:
    //   firstName    — required; trimmed and validated.
    //   lastName     — required; trimmed and validated.
    //   email        — required; trimmed and validated.
    //   hireDate     — the employee's start date.
    //   salary       — must be zero or positive.
    //   departmentId — must be greater than zero.
    //   phoneNumber  — optional; trimmed if provided.
    //
    // Throws:
    //   ArgumentException — if any required field is invalid.
    // -----------------------------------------------------------------------
    public Employee(
        string firstName,
        string lastName,
        string email,
        DateTime hireDate,
        decimal salary,
        int departmentId,
        string? phoneNumber = null)
    {
        // Validate each required string field. ValidateRequiredString throws
        // if the value is null, empty, or whitespace, and returns the
        // trimmed value otherwise.
        FirstName = ValidateRequiredString(firstName, nameof(firstName));
        LastName = ValidateRequiredString(lastName, nameof(lastName));
        Email = ValidateRequiredString(email, nameof(email));

        // PhoneNumber is optional, but we still trim it if provided to
        // keep stored values consistent.
        PhoneNumber = phoneNumber?.Trim();

        // Store the hire date. No future-date restriction in this phase.
        HireDate = hireDate;

        // Validate salary: negative salaries do not make sense and would
        // cause problems in reports and calculations.
        ValidateSalary(salary);
        Salary = salary;

        // Validate department ID: zero means "not assigned to any
        // department", which is not allowed. The ID must reference a
        // real department.
        ValidateDepartmentId(departmentId);
        DepartmentId = departmentId;
    }

    // -----------------------------------------------------------------------
    // UpdatePersonalDetails — changes the employee's name, email, and phone.
    //
    // These are the employee's personal/contact fields. Grouping them into
    // a single method makes the intent clear: "I am updating who this
    // person is and how to reach them." This is separate from employment
    // details (salary, department), which change for different reasons.
    //
    // Throws:
    //   ArgumentException — if firstName, lastName, or email is invalid.
    // -----------------------------------------------------------------------
    public void UpdatePersonalDetails(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber)
    {
        FirstName = ValidateRequiredString(firstName, nameof(firstName));
        LastName = ValidateRequiredString(lastName, nameof(lastName));
        Email = ValidateRequiredString(email, nameof(email));
        PhoneNumber = phoneNumber?.Trim();
    }

    // -----------------------------------------------------------------------
    // UpdateEmploymentDetails — changes the employee's salary and department.
    //
    // These are employment-related fields that typically change due to
    // promotions, transfers, or compensation adjustments. Keeping them
    // separate from personal details makes it clear what kind of change
    // is being made and which business rules apply.
    //
    // Throws:
    //   ArgumentException — if salary is negative or departmentId is invalid.
    // -----------------------------------------------------------------------
    public void UpdateEmploymentDetails(decimal salary, int departmentId)
    {
        ValidateSalary(salary);
        Salary = salary;

        ValidateDepartmentId(departmentId);
        DepartmentId = departmentId;
    }

    // -----------------------------------------------------------------------
    // Activate — marks the employee as active.
    //
    // Idempotent: calling on an already-active employee has no side effects.
    // -----------------------------------------------------------------------
    public void Activate()
    {
        IsActive = true;
    }

    // -----------------------------------------------------------------------
    // Deactivate — marks the employee as inactive (e.g., terminated,
    // on extended leave). The employee record is preserved for historical
    // and reporting purposes.
    // -----------------------------------------------------------------------
    public void Deactivate()
    {
        IsActive = false;
    }

    // =======================================================================
    // Private validation helpers
    // =======================================================================
    // These static methods centralize validation logic so that the same
    // rules are applied by the constructor and update methods. This
    // prevents inconsistencies where one code path accepts invalid data
    // that another rejects.
    // =======================================================================

    // -----------------------------------------------------------------------
    // ValidateRequiredString — ensures a string is not null, empty, or
    // whitespace. Returns the trimmed value if valid.
    //
    // This is a shared helper used for FirstName, LastName, and Email.
    // The parameterName argument is included in the exception message so
    // that callers know exactly which field failed validation.
    // -----------------------------------------------------------------------
    private static string ValidateRequiredString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{parameterName} cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }

    // -----------------------------------------------------------------------
    // ValidateSalary — ensures the salary is zero or positive.
    // A negative salary would not make sense in the real world and would
    // cause errors in payroll calculations and reports.
    // -----------------------------------------------------------------------
    private static void ValidateSalary(decimal salary)
    {
        if (salary < 0)
        {
            throw new ArgumentException(
                "Salary cannot be negative.",
                nameof(salary));
        }
    }

    // -----------------------------------------------------------------------
    // ValidateDepartmentId — ensures the department ID is greater than zero.
    // An ID of zero would indicate that the employee is not assigned to
    // any department, which violates the rule that every employee must
    // belong to a department.
    // -----------------------------------------------------------------------
    private static void ValidateDepartmentId(int departmentId)
    {
        if (departmentId <= 0)
        {
            throw new ArgumentException(
                "Department ID must be greater than zero.",
                nameof(departmentId));
        }
    }
}
