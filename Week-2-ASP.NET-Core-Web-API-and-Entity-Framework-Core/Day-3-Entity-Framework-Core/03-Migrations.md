# 03 — Migrations

**Duration:** ~30 min

---

## What You'll Learn

By the end of this topic, you will:

- Understand what a migration is
- Understand why migrations exist
- Create the initial migration
- Apply the migration to create the database and tables

---

## What Is a Migration?

A migration is a **snapshot of your database schema** at a point in time.

```text
Change C# Model
    |
    v
Create Migration (EF Core compares old and new model)
    |
    v
EF Core generates C# code to transform the database
    |
    v
Apply Migration (database is updated)
```

A migration file contains two methods:

- `Up()` — apply the change (create tables, add columns)
- `Down()` — reverse the change (drop tables, remove columns)

---

## Why Do Migrations Exist?

Without migrations:

- You would write SQL by hand to create tables
- Schema changes would be manual and error-prone
- Team members would have different database versions
- You would lose track of what changed

With migrations:

- Schema changes are tracked in code
- Everyone runs the same migrations
- Changes are version-controlled alongside your application code

---

## Step 1: Install the EF Core CLI Tool

Check if the tool is installed:

```bash
dotnet ef --version
```

If not installed, install it:

```bash
dotnet tool install --global dotnet-ef
```

This tool provides the `dotnet ef` commands for migration management.

---

## Step 2: Create the Initial Migration

Open a terminal in your **project folder** (where the `.csproj` file is) and run:

```bash
dotnet ef migrations add InitialCreate
```

**What does this command do?**

1. EF Core reads your `DbContext` and entity classes
2. It compares the current model to the previous state (nothing — this is the first migration)
3. It generates C# code to create the database schema

**What is created?**

A `Migrations/` folder appears with two files:

```text
Migrations/
    20240101000000_InitialCreate.cs
    20240101000000_InitialCreate.Designer.cs
    AppDbContextModelSnapshot.cs
```

| File | Purpose |
|------|---------|
| `InitialCreate.cs` | Contains `Up()` and `Down()` methods |
| `InitialCreate.Designer.cs` | Metadata about the model at this point |
| `AppDbContextModelSnapshot.cs` | Current model state (used for next migration comparison) |

**Look at the generated `Up()` method:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Departments",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Departments", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Employees",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DepartmentId = table.Column<int>(type: "int", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Employees", x => x.Id);
            table.ForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                column: x => x.DepartmentId,
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });
}
```

**What does this tell you?**

- Two tables: `Departments` and `Employees`
- `Departments` has `Id` (identity) and `Name`
- `Employees` has `Id`, `Name`, `Email`, `Salary`, `DepartmentId`
- `DepartmentId` is a foreign key pointing to `Departments.Id`
- Both `Id` columns use SQL Server identity (auto-increment)

---

## Step 3: Apply the Migration

Run:

```bash
dotnet ef database update
```

**What does this command do?**

1. Reads the migration files
2. Connects to SQL Server using the connection string
3. Creates the `EmployeeManagement` database (if it does not exist)
4. Executes the `Up()` method
5. Records that this migration has been applied

**Expected output:**

```text
Build started...
Build succeeded.
Done.
```

---

## Step 4: Verify the Database

Open SQL Server Management Studio (SSMS) or Azure Data Studio and check:

1. Database `EmployeeManagement` exists
2. Tables `Departments` and `Employees` exist
3. Columns match the entity classes
4. Foreign key constraint exists on `Employees.DepartmentId`

---

## The Migration Flow

```text
C# Entity
    |
    v
dotnet ef migrations add <Name>
    |
    v
Migration file generated (C# code)
    |
    v
dotnet ef database update
    |
    v
Database tables created
```

---

## What Happens When You Change the Model Later?

When you modify an entity (add a property, rename a column):

1. Run `dotnet ef migrations add <DescriptiveName>`
2. EF Core compares the new model to the snapshot
3. It generates a new migration with only the differences
4. Run `dotnet ef database update` to apply

Each migration builds on the previous one. EF Core tracks the full history.

---

## Common Mistakes

**Mistake 1: Running migration commands in the wrong folder.**

You must run `dotnet ef` commands from the project folder that contains the `.csproj` file and the `DbContext`.

**Mistake 2: Deleting migration files manually.**

If you delete a migration file but the database still has those changes, EF Core loses track. Use `dotnet ef migrations remove` to undo the last unapplied migration.

**Mistake 3: Modifying a migration after it has been applied.**

Once a migration is applied, do not edit it. Create a new migration instead.

---

## Knowledge Check

1. What is a migration?
2. What does `dotnet ef migrations add` do?
3. What does `dotnet ef database update` do?
4. What are the `Up()` and `Down()` methods?
5. What happens if you change a model after the initial migration?

---

## Next

The database and tables are created. Next, we define the one-to-many relationship between Department and Employee using navigation properties.
