using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

// This controller has 3 bugs. Find them and fix them.
[ApiController]
[Route("api/employee")]
public class EmployeesController : ControllerBase
{
    private static readonly List<Employee> Employees = new()
    {
        new Employee { Id = 1, Name = "Amira", Department = "Engineering" },
        new Employee { Id = 2, Name = "Karim", Department = "HR" }
    };

    [HttpGet("api/employees")]
    public IActionResult GetAll() => Ok(Employees);

    [HttpGet("{id:int}")]
    public IActionResult GetById(int employeeId)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == employeeId);
        return employee is null ? NotFound() : Ok(employee);
    }
}
