using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Interfaces.Repositories;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeManagement.Application.Tests;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepoMock;
    private readonly Mock<ILogger<DepartmentService>> _loggerMock;
    private readonly DepartmentService _sut;

    public DepartmentServiceTests()
    {
        _departmentRepoMock = new Mock<IDepartmentRepository>();
        _loggerMock = new Mock<ILogger<DepartmentService>>();
        _sut = new DepartmentService(_departmentRepoMock.Object, _loggerMock.Object);
    }

    // Builds a Department entity with the given Id using reflection
    // to set the private Id setter (simulating EF Core persistence).
    private static Department BuildDepartment(int id = 1, string name = "Engineering",
        string? description = "Software development")
    {
        var department = new Department(name, description);
        typeof(Department).GetProperty(nameof(Department.Id))!
            .SetValue(department, id);
        return department;
    }

    private static CreateDepartmentDto BuildCreateDto(string name = "Engineering",
        string? description = "Software development") => new()
    {
        Name = name,
        Description = description
    };

    private static UpdateDepartmentDto BuildUpdateDto(string name = "Updated Engineering",
        string? description = "Updated description") => new()
    {
        Name = name,
        Description = description
    };

    // Verifies GetByIdAsync returns a mapped DTO when the department exists.
    [Fact]
    public async Task GetByIdAsync_WithExistingDepartment_ReturnsDto()
    {
        var department = BuildDepartment(id: 3, name: "Finance");
        _departmentRepoMock.Setup(r => r.GetByIdAsync(3, default))
            .ReturnsAsync(department);

        var result = await _sut.GetByIdAsync(3);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Finance", result.Name);
        Assert.Equal("Software development", result.Description);
        Assert.True(result.IsActive);
        _departmentRepoMock.Verify(r => r.GetByIdAsync(3, default), Times.Once);
    }

    // Verifies GetByIdAsync returns null when the repository returns null.
    [Fact]
    public async Task GetByIdAsync_WithMissingDepartment_ReturnsNull()
    {
        _departmentRepoMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Department?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
        _departmentRepoMock.Verify(r => r.GetByIdAsync(99, default), Times.Once);
    }

    // Verifies GetAllAsync maps all departments to DTOs.
    [Fact]
    public async Task GetAllAsync_ReturnsAllDepartments()
    {
        var departments = new List<Department>
        {
            BuildDepartment(1, "Engineering", "Software"),
            BuildDepartment(2, "Finance", "Accounting")
        };
        _departmentRepoMock.Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(departments);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Engineering", result[0].Name);
        Assert.Equal("Finance", result[1].Name);
        _departmentRepoMock.Verify(r => r.GetAllAsync(default), Times.Once);
    }

    // Verifies CreateAsync constructs the entity, persists it, and returns a DTO.
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesDepartment()
    {
        var dto = BuildCreateDto(name: "Marketing");
        _departmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Department>(), default))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Marketing", result.Name);
        Assert.Equal("Software development", result.Description);
        _departmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Department>(), default), Times.Once);
    }

    // Verifies UpdateAsync retrieves the department, applies changes,
    // persists, and returns an updated DTO.
    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesDepartment()
    {
        var department = BuildDepartment(id: 1, name: "Old Name");
        var dto = BuildUpdateDto(name: "New Name", description: "New description");

        _departmentRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(department);
        _departmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Department>(), default))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New description", result.Description);
        _departmentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Department>(), default), Times.Once);
    }

    // Verifies UpdateAsync returns null when the department does not exist.
    [Fact]
    public async Task UpdateAsync_WithMissingDepartment_ReturnsNull()
    {
        var dto = BuildUpdateDto();
        _departmentRepoMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Department?)null);

        var result = await _sut.UpdateAsync(99, dto);

        Assert.Null(result);
        _departmentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Department>(), default), Times.Never);
    }

    // Verifies DeleteAsync deletes a department with no assigned employees
    // and returns true.
    [Fact]
    public async Task DeleteAsync_WithNoEmployees_DeletesDepartment()
    {
        var department = BuildDepartment(id: 1, name: "Legacy");
        _departmentRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(department);
        _departmentRepoMock.Setup(r => r.HasEmployeesAsync(1, default))
            .ReturnsAsync(false);
        _departmentRepoMock.Setup(r => r.DeleteAsync(1, default))
            .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _departmentRepoMock.Verify(r => r.HasEmployeesAsync(1, default), Times.Once);
        _departmentRepoMock.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }

    // Verifies DeleteAsync throws ConflictException when the department
    // has assigned employees.
    [Fact]
    public async Task DeleteAsync_WithAssignedEmployees_ThrowsConflictException()
    {
        var department = BuildDepartment(id: 1, name: "Engineering");
        _departmentRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(department);
        _departmentRepoMock.Setup(r => r.HasEmployeesAsync(1, default))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _sut.DeleteAsync(1));

        Assert.Contains("Engineering", ex.Message);
        Assert.Contains("employees", ex.Message);
        _departmentRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), default), Times.Never);
    }

    // Verifies DeleteAsync returns false when the department does not exist.
    [Fact]
    public async Task DeleteAsync_WithMissingDepartment_ReturnsFalse()
    {
        _departmentRepoMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Department?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result);
        _departmentRepoMock.Verify(r => r.HasEmployeesAsync(It.IsAny<int>(), default), Times.Never);
        _departmentRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), default), Times.Never);
    }
}
