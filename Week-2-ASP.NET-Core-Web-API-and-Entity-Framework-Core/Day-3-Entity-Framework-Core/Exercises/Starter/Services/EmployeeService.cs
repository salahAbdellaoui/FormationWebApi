using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly List<Employee> _employees = new()
    {
        new Employee { Id = 1, Name = "Sara", Department = "IT" },
        new Employee { Id = 2, Name = "Ahmed", Department = "HR" },
        new Employee { Id = 3, Name = "Leila", Department = "Finance" }
    };

    public IEnumerable<Employee> GetAll() => _employees;

    public Employee? GetById(int id) => _employees.FirstOrDefault(e => e.Id == id);

    public Employee Create(Employee employee)
    {
        employee.Id = _employees.Count > 0 ? _employees.Max(e => e.Id) + 1 : 1;
        _employees.Add(employee);
        return employee;
    }

    public bool Update(int id, Employee employee)
    {
        var existing = _employees.FirstOrDefault(e => e.Id == id);
        if (existing == null) return false;

        existing.Name = employee.Name;
        existing.Department = employee.Department;
        return true;
    }

    public bool Delete(int id)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == id);
        if (employee == null) return false;

        _employees.Remove(employee);
        return true;
    }
}
