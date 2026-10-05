# Day 3 Exercises — Entity Framework Core

These exercises build on the starter project in `Starter/`.

The starter project is the Day 2 final API: an `EmployeeManagement.Api` with in-memory data, CRUD endpoints, and DTOs.

Your job is to evolve it to use EF Core and SQL Server.

---

## Exercise 1 — Create DbContext

**Goal:** Set up `AppDbContext` with `DbSet` properties.

**Steps:**

1. Install the EF Core packages:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.*
   dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.*
   ```

2. Create a `Data/` folder.

3. Create `Data/AppDbContext.cs` with:
   - `DbSet<Employee> Employees`
   - `DbSet<Department> Departments`

4. Add the SQL Server connection string to `appsettings.json`.

5. Register `AppDbContext` in `Program.cs` using `AddDbContext`.

**Verify:** The project builds with `dotnet build`.

---

## Exercise 2 — Code First Entities

**Goal:** Create the entity classes that become database tables.

**Steps:**

1. Create `Models/Department.cs` with `Id` and `Name`.

2. Update `Models/Employee.cs`:
   - Add `Email` (string)
   - Add `Salary` (decimal)
   - Replace `Department` (string) with `DepartmentId` (int) and `Department` navigation property

3. Update the DTOs to match the new entity shape.

**Verify:** The project builds.

---

## Exercise 3 — Create and Apply Migration

**Goal:** Create the database and tables.

**Steps:**

1. Install the EF Core CLI tool (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Create the initial migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. Apply the migration:
   ```bash
   dotnet ef database update
   ```

4. Verify the database and tables were created in SQL Server.

**Verify:** A `Migrations/` folder exists with the migration files. The database has `Departments` and `Employees` tables.

---

## Exercise 4 — Define the Relationship

**Goal:** Ensure the one-to-many relationship works correctly.

**Steps:**

1. Add the `ICollection<Employee> Employees` navigation property to `Department`.

2. Run the API and create a department:
   ```http
   POST /api/departments
   { "name": "IT" }
   ```

3. Create an employee linked to that department:
   ```http
   POST /api/employees
   { "name": "Alice", "email": "alice@company.com", "salary": 5000, "departmentId": 1 }
   ```

4. Verify the employee's `DepartmentId` matches the department's `Id`.

**Note:** You may need a `DepartmentsController` to create departments. Create a simple one with CRUD endpoints similar to `EmployeesController`.

**Verify:** Employee is linked to the correct department.

---

## Exercise 5 — EF Core Queries

**Goal:** Implement LINQ queries through EF Core.

**Steps:**

Update `EmployeesController` to use `AppDbContext` instead of the in-memory service.

Implement these queries:

1. **Get all employees** — include the department name
2. **Get employee by ID** — include the department name
3. **Filter by salary** — `GET /api/employees?minSalary=3000`
4. **Order by salary descending** — add to the get-all query
5. **Create, update, delete** — use `SaveChangesAsync()`

**Verify:**
- `GET /api/employees` returns all employees with department names
- `GET /api/employees/1` returns one employee with department name
- `GET /api/employees?minSalary=3000` returns filtered results
- POST, PUT, DELETE work correctly and persist to the database

---

## Tips

- Build often: `dotnet build`
- Run the API: `dotnet run`
- Test with Swagger: navigate to `https://localhost:<port>/swagger`
- Check the database in SQL Server Management Studio or Azure Data Studio
- If a migration goes wrong, use `dotnet ef migrations remove` to undo it

---

## Solution

The completed project from the lesson topics shows the full solution. Work through each exercise before looking at the topic code.
