using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Domain.Tests;

public class DepartmentTests
{
    // Valid construction — Name and IsActive set correctly
    [Fact]
    public void Constructor_ValidName_SetsProperties()
    {
        var department = new Department("Engineering");

        Assert.Equal("Engineering", department.Name);
        Assert.Null(department.Description);
        Assert.True(department.IsActive);
    }

    // Name null — throws ArgumentException
    [Fact]
    public void Constructor_NullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Department(null!));
    }

    // Name empty — throws ArgumentException
    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Department(""));
    }

    // Name whitespace — throws ArgumentException
    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Department("   "));
    }

    // Name is trimmed
    [Fact]
    public void Constructor_NameIsTrimmed()
    {
        var department = new Department("  Engineering  ");

        Assert.Equal("Engineering", department.Name);
    }

    // Description null — accepted
    [Fact]
    public void Constructor_NullDescription_IsAccepted()
    {
        var department = new Department("Engineering", null);

        Assert.Null(department.Description);
    }

    // Description non-null — accepted
    [Fact]
    public void Constructor_WithDescription_SetsDescription()
    {
        var department = new Department("Engineering", "Builds software");

        Assert.Equal("Builds software", department.Description);
    }

    // Description is trimmed
    [Fact]
    public void Constructor_DescriptionIsTrimmed()
    {
        var department = new Department("Engineering", "  Builds software  ");

        Assert.Equal("Builds software", department.Description);
    }

    // UpdateName with valid data — updates Name
    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        var department = new Department("Engineering");

        department.UpdateName("Human Resources");

        Assert.Equal("Human Resources", department.Name);
    }

    // UpdateName with invalid data — throws, entity unchanged
    [Fact]
    public void UpdateName_EmptyName_ThrowsAndLeavesEntityUnchanged()
    {
        var department = new Department("Engineering");

        Assert.Throws<ArgumentException>(() =>
            department.UpdateName(""));

        Assert.Equal("Engineering", department.Name);
    }

    // UpdateDescription with valid data — updates Description
    [Fact]
    public void UpdateDescription_ValidDescription_UpdatesDescription()
    {
        var department = new Department("Engineering");

        department.UpdateDescription("Builds software");

        Assert.Equal("Builds software", department.Description);
    }

    // UpdateDescription with null — sets Description to null
    [Fact]
    public void UpdateDescription_Null_SetsDescriptionToNull()
    {
        var department = new Department("Engineering", "Original description");

        department.UpdateDescription(null);

        Assert.Null(department.Description);
    }

    // Activate sets IsActive = true
    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var department = new Department("Engineering");
        department.Deactivate();

        department.Activate();

        Assert.True(department.IsActive);
    }

    // Deactivate sets IsActive = false
    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var department = new Department("Engineering");

        department.Deactivate();

        Assert.False(department.IsActive);
    }
}
