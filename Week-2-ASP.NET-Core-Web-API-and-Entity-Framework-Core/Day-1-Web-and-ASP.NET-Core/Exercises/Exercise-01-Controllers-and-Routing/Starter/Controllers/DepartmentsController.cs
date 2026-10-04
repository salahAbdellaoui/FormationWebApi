using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

// TODO 1: give this controller a base route built with the [controller] token
//         so that it answers on /api/departments
[ApiController]
public class DepartmentsController : ControllerBase
{
    // TODO 2: keep a static in-memory list of at least three departments here
    // private static readonly List<Department> Departments = new()
    // {
    //     new Department { Id = 1, Name = "...", Location = "..." },
    //     ...
    // };

    // TODO 3: GET /api/departments → 200 + the whole list
    //         public IActionResult GetAll() ...

    // TODO 4: GET /api/departments/{id:int} → 200 + one department,
    //         or 404 when the id does not exist
    //         public IActionResult GetById(int id) ...
}
