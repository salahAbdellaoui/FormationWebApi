// Final Challenge starter — a clean .NET 8 Web API project.
// Everything below is the application skeleton only: no model, no controller, no service.
//
// Your TODOs (see README.md):
//   1. Create Models/Department.cs
//   2. Create Services/IDepartmentService.cs and Services/DepartmentService.cs
//   3. Create Controllers/DepartmentsController.cs with
//        GET /api/departments      → 200 + the list
//        GET /api/departments/{id} → 200 + one department, or 404
//   4. Register the service as Scoped before builder.Build():
//        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
//      (plus the matching using directive at the top of this file)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
