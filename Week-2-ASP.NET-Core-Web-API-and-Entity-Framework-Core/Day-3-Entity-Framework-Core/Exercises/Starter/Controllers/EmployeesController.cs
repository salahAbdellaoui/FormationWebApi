using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Models;
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
    public IActionResult GetAll()
    {
        var employees = _employeeService.GetAll();

        var dtos = employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Department = e.Department
        });

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var employee = _employeeService.GetById(id);
        if (employee == null)
            return NotFound();

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Department = employee.Department
        };

        return Ok(dto);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Department = dto.Department
        };

        var created = _employeeService.Create(employee);

        var response = new EmployeeDto
        {
            Id = created.Id,
            Name = created.Name,
            Department = created.Department
        };

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = response.Id },
            value: response);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Department = dto.Department
        };

        var success = _employeeService.Update(id, employee);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var success = _employeeService.Delete(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
