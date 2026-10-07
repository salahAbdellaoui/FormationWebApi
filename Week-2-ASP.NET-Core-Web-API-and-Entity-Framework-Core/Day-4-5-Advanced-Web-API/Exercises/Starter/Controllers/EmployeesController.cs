using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        var dtos = employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department != null ? e.Department.Name : null
        });

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound();

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department != null ? employee.Department.Name : null
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var response = new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary,
            DepartmentId = employee.DepartmentId
        };

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = response.Id },
            value: response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        employee.Name = dto.Name;
        employee.Email = dto.Email;
        employee.Salary = dto.Salary;
        employee.DepartmentId = dto.DepartmentId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
