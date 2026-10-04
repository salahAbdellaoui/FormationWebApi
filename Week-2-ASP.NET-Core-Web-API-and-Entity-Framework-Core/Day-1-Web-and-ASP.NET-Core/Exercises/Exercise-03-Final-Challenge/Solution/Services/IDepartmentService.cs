using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

public interface IDepartmentService
{
    IEnumerable<Department> GetAll();
    Department? GetById(int id);
}
