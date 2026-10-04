# Exercise 01 — Create the Web API Project

## 🎯 Objective

Create and run your first ASP.NET Core Web API project.

## 📋 Task

Open a terminal and run:

```bash
dotnet new webapi -n EmployeeManagement.Api --use-controllers
cd EmployeeManagement.Api
dotnet run
```

You should see:

```text
Now listening on: http://localhost:5189
```

Open a second terminal and test:

```bash
curl -i http://localhost:5189/weatherforecast
```

You should get back the sample `WeatherForecast` data.

## ✅ Success Criteria

- [ ] The project was created successfully
- [ ] `dotnet run` starts without errors
- [ ] You can see the "Now listening on:" message
- [ ] `curl` returns a JSON response from `/weatherforecast`

## 🧹 Clean Up

Delete the sample files:

```bash
rm Controllers/WeatherForecastController.cs
rm WeatherForecast.cs
```

Rebuild:

```bash
dotnet build
```

The build succeeds — you have an empty API.

## 📝 What You Learned

- How to create a Web API project with `--use-controllers`
- How to run it with `dotnet run`
- How to test an endpoint with `curl`
- The template includes sample code you can remove

## 🔜 Next

**Exercise 02** — Create the `EmployeesController` skeleton.

## 📚 Related Lesson

[04 — Controllers and Routing](../../04-Controllers-and-Routing.md)
