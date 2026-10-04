using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public class DepartmentService : IDepartmentService
{
    private static readonly List<Department> Departments = new()
    {
        new Department { Id = 1, Name = "Engineering" },
        new Department { Id = 2, Name = "HR" },
        new Department { Id = 3, Name = "Finance" }
    };

    public IEnumerable<Department> GetAll() => Departments;

    public Department? GetById(int id) => Departments.FirstOrDefault(d => d.Id == id);
}
