using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_departmentService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var department = _departmentService.GetById(id);
        return department is null ? NotFound() : Ok(department);
    }
}
