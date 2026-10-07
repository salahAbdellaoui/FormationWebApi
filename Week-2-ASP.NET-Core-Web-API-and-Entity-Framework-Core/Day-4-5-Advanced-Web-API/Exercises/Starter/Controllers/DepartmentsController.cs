using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var departments = await _context.Departments
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var department = await _context.Departments
            .Where(d => d.Id == id)
            .Select(d => new { d.Id, d.Name })
            .FirstOrDefaultAsync();

        if (department == null)
            return NotFound();

        return Ok(department);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Name = dto.Name
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = department.Id },
            value: new { department.Id, department.Name });
    }
}
