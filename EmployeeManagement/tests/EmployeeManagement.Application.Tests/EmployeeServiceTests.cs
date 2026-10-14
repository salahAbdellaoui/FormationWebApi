using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeManagement.Application.Tests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IDepartmentRepository> _departmentRepoMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _departmentRepoMock = new Mock<IDepartmentRepository>();
        _loggerMock = new Mock<ILogger<EmployeeService>>();
        _sut = new EmployeeService(
            _employeeRepoMock.Object,
            _departmentRepoMock.Object,
            _loggerMock.Object);
    }

    // Builds an Employee entity with the given Id using reflection
    // to set the private Id setter (simulating EF Core persistence).
    private static Employee BuildEmployee(int id = 1, string firstName = "John",
        string lastName = "Doe", string email = "john@example.com",
        DateTime? hireDate = null, decimal salary = 50000m,
        int departmentId = 1, string? phoneNumber = null)
    {
        var employee = new Employee(firstName, lastName, email,
            hireDate ?? new DateTime(2023, 1, 15), salary, departmentId, phoneNumber);
        typeof(Employee).GetProperty(nameof(Employee.Id))!
            .SetValue(employee, id);
        return employee;
    }

    private static CreateEmployeeDto BuildCreateDto(int departmentId = 1,
        string email = "john@example.com") => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = email,
        HireDate = new DateTime(2023, 1, 15),
        Salary = 50000m,
        DepartmentId = departmentId,
        PhoneNumber = "+1234567890"
    };

    private static UpdateEmployeeDto BuildUpdateDto(int departmentId = 1,
        string email = "updated@example.com") => new()
    {
        FirstName = "Updated",
        LastName = "Name",
        Email = email,
        HireDate = new DateTime(2023, 6, 1),
        Salary = 60000m,
        DepartmentId = departmentId,
        PhoneNumber = "+9876543210"
    };

    // Verifies GetByIdAsync returns a mapped DTO when the employee exists.
    [Fact]
    public async Task GetByIdAsync_WithExistingEmployee_ReturnsDto()
    {
        var employee = BuildEmployee(id: 5);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(5, default))
            .ReturnsAsync(employee);

        var result = await _sut.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.True(result.IsActive);
        _employeeRepoMock.Verify(r => r.GetByIdAsync(5, default), Times.Once);
    }

    // Verifies GetByIdAsync returns null when the repository returns null.
    [Fact]
    public async Task GetByIdAsync_WithMissingEmployee_ReturnsNull()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Employee?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
        _employeeRepoMock.Verify(r => r.GetByIdAsync(99, default), Times.Once);
    }

    // Verifies GetAllAsync maps all employees to DTOs.
    [Fact]
    public async Task GetAllAsync_ReturnsAllEmployees()
    {
        var employees = new List<Employee>
        {
            BuildEmployee(1, "Alice", "Smith", "alice@example.com"),
            BuildEmployee(2, "Bob", "Jones", "bob@example.com")
        };
        _employeeRepoMock.Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(employees);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].FirstName);
        Assert.Equal("Bob", result[1].FirstName);
        _employeeRepoMock.Verify(r => r.GetAllAsync(default), Times.Once);
    }

    // Verifies CreateAsync creates and returns a DTO when department exists
    // and email is not duplicated.
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesEmployee()
    {
        var dto = BuildCreateDto();
        _departmentRepoMock.Setup(r => r.ExistsAsync(dto.DepartmentId, default))
            .ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, 0, default))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), default))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        _departmentRepoMock.Verify(r => r.ExistsAsync(dto.DepartmentId, default), Times.Once);
        _employeeRepoMock.Verify(r => r.EmailExistsAsync(dto.Email, 0, default), Times.Once);
        _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), default), Times.Once);
    }

    // Verifies CreateAsync throws NotFoundException when the department does not exist.
    [Fact]
    public async Task CreateAsync_WithNonExistentDepartment_ThrowsNotFoundException()
    {
        var dto = BuildCreateDto(departmentId: 999);
        _departmentRepoMock.Setup(r => r.ExistsAsync(999, default))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateAsync(dto));

        Assert.Contains("Department", ex.Message);
        _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), default), Times.Never);
    }

    // Verifies CreateAsync throws ConflictException when the email is already taken.
    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsConflictException()
    {
        var dto = BuildCreateDto(email: "taken@example.com");
        _departmentRepoMock.Setup(r => r.ExistsAsync(dto.DepartmentId, default))
            .ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync("taken@example.com", 0, default))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _sut.CreateAsync(dto));

        Assert.Contains("taken@example.com", ex.Message);
        _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), default), Times.Never);
    }

    // Verifies UpdateAsync retrieves the employee, validates department/email,
    // applies changes, and returns an updated DTO.
    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesEmployee()
    {
        var employee = BuildEmployee(id: 1);
        var dto = BuildUpdateDto(email: "new@example.com");

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(employee);
        _departmentRepoMock.Setup(r => r.ExistsAsync(dto.DepartmentId, default))
            .ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync("new@example.com", 1, default))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), default))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal("new@example.com", result.Email);
        _employeeRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), default), Times.Once);
    }

    // Verifies UpdateAsync returns null when the employee does not exist.
    [Fact]
    public async Task UpdateAsync_WithMissingEmployee_ReturnsNull()
    {
        var dto = BuildUpdateDto();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Employee?)null);

        var result = await _sut.UpdateAsync(99, dto);

        Assert.Null(result);
        _employeeRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), default), Times.Never);
    }

    // Verifies UpdateAsync throws NotFoundException when the department does not exist.
    [Fact]
    public async Task UpdateAsync_WithNonExistentDepartment_ThrowsNotFoundException()
    {
        var employee = BuildEmployee(id: 1);
        var dto = BuildUpdateDto(departmentId: 999);

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(employee);
        _departmentRepoMock.Setup(r => r.ExistsAsync(999, default))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateAsync(1, dto));

        Assert.Contains("Department", ex.Message);
        _employeeRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), default), Times.Never);
    }

    // Verifies DeleteAsync returns true when the employee is found and deleted.
    [Fact]
    public async Task DeleteAsync_WithExistingEmployee_ReturnsTrue()
    {
        _employeeRepoMock.Setup(r => r.DeleteAsync(1, default))
            .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _employeeRepoMock.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }

    // Verifies DeleteAsync returns false when the employee is not found.
    [Fact]
    public async Task DeleteAsync_WithMissingEmployee_ReturnsFalse()
    {
        _employeeRepoMock.Setup(r => r.DeleteAsync(99, default))
            .ReturnsAsync(false);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result);
        _employeeRepoMock.Verify(r => r.DeleteAsync(99, default), Times.Once);
    }
}
