using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public class DepartmentService : IDepartmentService
{
    // In-memory data: a teaching stand-in for a real database (later lessons).
    private static readonly List<Department> Departments = new()
    {
        new Department { Id = 1, Name = "Engineering", Location = "Building A" },
        new Department { Id = 2, Name = "HR",          Location = "Building B" },
        new Department { Id = 3, Name = "Finance",     Location = "Building A" }
    };

    public IEnumerable<Department> GetAll() => Departments;

    public Department? GetById(int id) => Departments.FirstOrDefault(d => d.Id == id);
}
