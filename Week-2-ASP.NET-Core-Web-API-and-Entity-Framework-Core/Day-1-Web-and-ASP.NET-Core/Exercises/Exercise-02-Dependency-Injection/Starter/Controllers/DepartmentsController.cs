using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

// This controller works, but it does three jobs: HTTP, data, and lookup.
//
// TODO: extract everything except HTTP into a service
//   1. Create Services/IDepartmentService.cs          (the contract)
//   2. Create Services/DepartmentService.cs           (the list + the lookups)
//   3. Register it in Program.cs as Scoped, before builder.Build()
//   4. Inject it through the constructor
//
// The controller must finish with:
//   - no List<Department> field
//   - no FirstOrDefault call
//   - no data at all — only routing, service calls, and status codes
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
