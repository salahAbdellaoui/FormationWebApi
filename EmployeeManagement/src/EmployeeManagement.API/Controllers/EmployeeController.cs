// ============================================================================
// EmployeeController — HTTP endpoints for Employee CRUD operations
// ============================================================================
//
// This controller handles all HTTP requests related to employees. It is the
// outermost layer in the Clean Architecture: it receives HTTP requests,
// delegates business logic to IEmployeeService, and returns HTTP responses.
//
// The controller does NOT:
//   - Access the database directly (no DbContext, no repositories).
//   - Contain business rules or validation logic.
//   - Know about Domain entities.
//
// The controller DOES:
//   - Receive and validate HTTP input (via [ApiController] + Data Annotations).
//   - Call the Application service with DTOs.
//   - Return appropriate HTTP status codes and response bodies.
//   - Handle null results (entity not found) with 404.
//   - Let the exception middleware handle NotFoundException and ConflictException.
// ============================================================================

using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

/// <summary>
/// Provides RESTful HTTP endpoints for managing employees.
/// All endpoints require authentication with the Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    // -----------------------------------------------------------------------
    // Constructor — receives the employee service through dependency injection.
    //
    // The controller depends on the interface (IEmployeeService), not the
    // concrete EmployeeService. This allows the service to be replaced with
    // a mock during testing and keeps the controller decoupled from the
    // Application implementation.
    // -----------------------------------------------------------------------
    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // -----------------------------------------------------------------------
    // GET /api/employee — retrieves all employees.
    //
    // Returns 200 OK with a JSON array of employee DTOs.
    // Returns an empty array (not 404) when no employees exist.
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return Ok(employees);
    }

    // -----------------------------------------------------------------------
    // GET /api/employee/{id} — retrieves a single employee by ID.
    //
    // Returns 200 OK with the employee DTO if found.
    // Returns 404 Not Found if the employee does not exist.
    //
    // The {id:int} route constraint ensures that non-integer values in the
    // URL are rejected with a 404 before the action is even called.
    // -----------------------------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(
        int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);

        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    // -----------------------------------------------------------------------
    // POST /api/employee — creates a new employee.
    //
    // Returns 201 Created with the new employee DTO and a Location header
    // pointing to the GET endpoint for the new resource.
    //
    // Why 201 instead of 200? HTTP semantics specify that 201 indicates a
    // new resource was created. The Location header tells the client where
    // to retrieve the new resource.
    //
    // The [ApiController] attribute automatically validates the request body
    // against Data Annotations on CreateEmployeeDto and returns 400 if
    // validation fails. The controller does not need to check ModelState.
    //
    // Application-level exceptions (NotFoundException for missing department,
    // ConflictException for duplicate email) are handled by the exception
    // middleware, which maps them to 404 and 409 respectively.
    // -----------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDto>> Create(
        [FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var created = await _employeeService.CreateAsync(dto, cancellationToken);

        // CreatedAtAction returns 201 with a Location header pointing to
        // the GetById action for the newly created employee.
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created);
    }

    // -----------------------------------------------------------------------
    // PUT /api/employee/{id} — updates an existing employee.
    //
    // The {id} parameter comes from the URL route. The DTO in the body
    // contains the new values. The route ID is the authoritative identifier;
    // the DTO does not include an Id field, so there is no risk of a
    // mismatch between the URL and the body.
    //
    // Returns 200 OK with the updated employee DTO.
    // Returns 404 Not Found if the employee does not exist.
    // -----------------------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDto>> Update(
        int id, [FromBody] UpdateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var updated = await _employeeService.UpdateAsync(id, dto, cancellationToken);

        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/employee/{id} — deletes an employee by ID.
    //
    // Returns 204 No Content on success. 204 indicates the operation
    // succeeded but there is no response body to return.
    //
    // Returns 404 Not Found if the employee does not exist.
    // -----------------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken)
    {
        var deleted = await _employeeService.DeleteAsync(id, cancellationToken);

        if (!deleted)
            return NotFound();

        // 204 No Content — the deletion succeeded and there is nothing
        // to return in the response body.
        return NoContent();
    }
}
