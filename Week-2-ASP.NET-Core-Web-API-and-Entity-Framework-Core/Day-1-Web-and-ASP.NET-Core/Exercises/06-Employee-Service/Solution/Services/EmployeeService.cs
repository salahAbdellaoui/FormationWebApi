using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public class EmployeeService : IEmployeeService
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim", Department = "HR" },
        new Employee { Id = 3, Name = "Sara", Department = "Engineering" }
    };

    public IEnumerable<Employee> GetAll() => Employees;

    public Employee? GetById(int id) => Employees.FirstOrDefault(e => e.Id == id);
}
