// ============================================================================
// DepartmentController — HTTP endpoints for Department CRUD operations
// ============================================================================
//
// This controller handles all HTTP requests related to departments. Like
// EmployeeController, it delegates to an Application service and returns
// HTTP responses. It does not access the database directly.
//
// The department deletion endpoint relies on the Application service to
// check whether employees are assigned. If the service throws
// ConflictException, the exception middleware returns 409. The controller
// does not duplicate the employee-assignment check.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

/// <summary>
/// Provides RESTful HTTP endpoints for managing departments.
/// All endpoints require authentication with the Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // -----------------------------------------------------------------------
    // GET /api/department — retrieves all departments.
    // Returns 200 OK with a JSON array.
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }

    // -----------------------------------------------------------------------
    // GET /api/department/{id} — retrieves a single department by ID.
    // Returns 200 OK if found, 404 Not Found if not.
    // -----------------------------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentResponseDto>> GetById(
        int id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetByIdAsync(id, cancellationToken);

        if (department is null)
            return NotFound();

        return Ok(department);
    }

    // -----------------------------------------------------------------------
    // POST /api/department — creates a new department.
    // Returns 201 Created with the new department and a Location header.
    // -----------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<DepartmentResponseDto>> Create(
        [FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var created = await _departmentService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    // -----------------------------------------------------------------------
    // PUT /api/department/{id} — updates an existing department.
    // Returns 200 OK with the updated department, or 404 if not found.
    // -----------------------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentResponseDto>> Update(
        int id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var updated = await _departmentService.UpdateAsync(id, dto, cancellationToken);

        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/department/{id} — deletes a department by ID.
    //
    // Returns 204 No Content on success.
    // Returns 404 Not Found if the department does not exist.
    //
    // If the department has employees assigned, the Application service
    // throws ConflictException. The exception middleware catches it and
    // returns 409 Conflict. The controller does not need to check for
    // assigned employees — that is the service's responsibility.
    // -----------------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken)
    {
        var deleted = await _departmentService.DeleteAsync(id, cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
