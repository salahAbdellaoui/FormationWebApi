using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    IEnumerable<Employee> GetAll();
    Employee? GetById(int id);
    Employee Create(Employee employee);
    bool Update(int id, Employee employee);
    bool Delete(int id);
}
