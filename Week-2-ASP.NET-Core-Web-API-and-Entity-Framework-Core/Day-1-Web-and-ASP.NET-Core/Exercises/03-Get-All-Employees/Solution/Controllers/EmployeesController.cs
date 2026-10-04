using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim", Department = "HR" },
        new Employee { Id = 3, Name = "Sara", Department = "Engineering" }
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Employees);
}
