using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    IEnumerable<Employee> GetAll();
    Employee? GetById(int id);
}
