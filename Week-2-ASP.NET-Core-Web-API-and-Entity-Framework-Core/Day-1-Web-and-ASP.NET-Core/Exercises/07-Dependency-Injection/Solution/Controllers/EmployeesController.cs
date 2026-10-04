using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_employeeService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var employee = _employeeService.GetById(id);
        return employee is null ? NotFound() : Ok(employee);
    }
}
