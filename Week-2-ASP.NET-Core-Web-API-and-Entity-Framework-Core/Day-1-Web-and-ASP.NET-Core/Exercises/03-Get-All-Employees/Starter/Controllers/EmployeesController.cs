using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    // TODO: Add a static list of employees
    // private static readonly List<Employee> Employees = new()
    // {
    //     new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
    //     new Employee { Id = 2, Name = "Karim", Department = "HR" }
    // };

    [HttpGet]
    public IActionResult GetAll() => Ok("employees endpoint");
}
