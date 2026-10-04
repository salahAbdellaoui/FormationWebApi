using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private static readonly List<Department> Departments = new()
    {
        new Department { Id = 1, Name = "Engineering", Location = "Building A" },
        new Department { Id = 2, Name = "HR",          Location = "Building B" },
        new Department { Id = 3, Name = "Finance",     Location = "Building A" }
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Departments);

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var department = Departments.FirstOrDefault(d => d.Id == id);

        return department is null ? NotFound() : Ok(department);
    }
}
