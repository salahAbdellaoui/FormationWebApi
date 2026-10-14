using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Domain.Tests;

public class EmployeeTests
{
    // Valid construction — all properties set correctly
    [Fact]
    public void Constructor_ValidData_SetsAllProperties()
    {
        var hireDate = new DateTime(2024, 1, 15);

        var employee = new Employee("John", "Doe", "john@example.com", hireDate, 50000m, 1, "+1234567890");

        Assert.Equal("John", employee.FirstName);
        Assert.Equal("Doe", employee.LastName);
        Assert.Equal("john@example.com", employee.Email);
        Assert.Equal(hireDate, employee.HireDate);
        Assert.Equal(50000m, employee.Salary);
        Assert.Equal(1, employee.DepartmentId);
        Assert.Equal("+1234567890", employee.PhoneNumber);
        Assert.True(employee.IsActive);
    }

    // FirstName null — throws ArgumentException
    [Fact]
    public void Constructor_NullFirstName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee(null!, "Doe", "john@example.com", DateTime.Now, 50000m, 1));
    }

    // FirstName empty — throws ArgumentException
    [Fact]
    public void Constructor_EmptyFirstName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("", "Doe", "john@example.com", DateTime.Now, 50000m, 1));
    }

    // FirstName whitespace — throws ArgumentException
    [Fact]
    public void Constructor_WhitespaceFirstName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("   ", "Doe", "john@example.com", DateTime.Now, 50000m, 1));
    }

    // LastName null — throws ArgumentException
    [Fact]
    public void Constructor_NullLastName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", null!, "john@example.com", DateTime.Now, 50000m, 1));
    }

    // LastName empty — throws ArgumentException
    [Fact]
    public void Constructor_EmptyLastName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "", "john@example.com", DateTime.Now, 50000m, 1));
    }

    // LastName whitespace — throws ArgumentException
    [Fact]
    public void Constructor_WhitespaceLastName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "   ", "john@example.com", DateTime.Now, 50000m, 1));
    }

    // Email null — throws ArgumentException
    [Fact]
    public void Constructor_NullEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", null!, DateTime.Now, 50000m, 1));
    }

    // Email empty — throws ArgumentException
    [Fact]
    public void Constructor_EmptyEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", "", DateTime.Now, 50000m, 1));
    }

    // Email whitespace — throws ArgumentException
    [Fact]
    public void Constructor_WhitespaceEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", "   ", DateTime.Now, 50000m, 1));
    }

    // FirstName, LastName, Email are trimmed
    [Fact]
    public void Constructor_StringsAreTrimmed()
    {
        var employee = new Employee("  John  ", "  Doe  ", "  john@example.com  ", DateTime.Now, 50000m, 1);

        Assert.Equal("John", employee.FirstName);
        Assert.Equal("Doe", employee.LastName);
        Assert.Equal("john@example.com", employee.Email);
    }

    // Negative salary — throws ArgumentException
    [Fact]
    public void Constructor_NegativeSalary_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", "john@example.com", DateTime.Now, -1m, 1));
    }

    // Zero salary — accepted (business rule permits it)
    [Fact]
    public void Constructor_ZeroSalary_IsAccepted()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 0m, 1);

        Assert.Equal(0m, employee.Salary);
    }

    // DepartmentId zero — throws ArgumentException
    [Fact]
    public void Constructor_ZeroDepartmentId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 0));
    }

    // DepartmentId negative — throws ArgumentException
    [Fact]
    public void Constructor_NegativeDepartmentId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, -1));
    }

    // Future HireDate — accepted (no restriction)
    [Fact]
    public void Constructor_FutureHireDate_IsAccepted()
    {
        var futureDate = DateTime.Today.AddDays(30);

        var employee = new Employee("John", "Doe", "john@example.com", futureDate, 50000m, 1);

        Assert.Equal(futureDate, employee.HireDate);
    }

    // PhoneNumber is trimmed when provided
    [Fact]
    public void Constructor_PhoneNumberIsTrimmed()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1, "  +1234567890  ");

        Assert.Equal("+1234567890", employee.PhoneNumber);
    }

    // PhoneNumber null — accepted
    [Fact]
    public void Constructor_NullPhoneNumber_IsAccepted()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        Assert.Null(employee.PhoneNumber);
    }

    // UpdatePersonalDetails with valid data — updates all fields
    [Fact]
    public void UpdatePersonalDetails_ValidData_UpdatesAllFields()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        employee.UpdatePersonalDetails("Jane", "Smith", "jane@example.com", "+9876543210");

        Assert.Equal("Jane", employee.FirstName);
        Assert.Equal("Smith", employee.LastName);
        Assert.Equal("jane@example.com", employee.Email);
        Assert.Equal("+9876543210", employee.PhoneNumber);
    }

    // UpdatePersonalDetails with invalid firstName — throws, entity unchanged
    [Fact]
    public void UpdatePersonalDetails_EmptyFirstName_ThrowsAndLeavesEntityUnchanged()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        Assert.Throws<ArgumentException>(() =>
            employee.UpdatePersonalDetails("", "Smith", "jane@example.com", null));

        Assert.Equal("John", employee.FirstName);
        Assert.Equal("Doe", employee.LastName);
        Assert.Equal("john@example.com", employee.Email);
    }

    // UpdateEmploymentDetails with valid data — updates salary and departmentId
    [Fact]
    public void UpdateEmploymentDetails_ValidData_UpdatesFields()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        employee.UpdateEmploymentDetails(75000m, 2);

        Assert.Equal(75000m, employee.Salary);
        Assert.Equal(2, employee.DepartmentId);
    }

    // UpdateEmploymentDetails with negative salary — throws ArgumentException
    [Fact]
    public void UpdateEmploymentDetails_NegativeSalary_ThrowsArgumentException()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        Assert.Throws<ArgumentException>(() =>
            employee.UpdateEmploymentDetails(-1m, 2));
    }

    // UpdateEmploymentDetails with invalid departmentId — throws ArgumentException
    [Fact]
    public void UpdateEmploymentDetails_InvalidDepartmentId_ThrowsArgumentException()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        Assert.Throws<ArgumentException>(() =>
            employee.UpdateEmploymentDetails(75000m, 0));
    }

    // Activate sets IsActive = true
    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);
        employee.Deactivate();

        employee.Activate();

        Assert.True(employee.IsActive);
    }

    // Deactivate sets IsActive = false
    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var employee = new Employee("John", "Doe", "john@example.com", DateTime.Now, 50000m, 1);

        employee.Deactivate();

        Assert.False(employee.IsActive);
    }
}
