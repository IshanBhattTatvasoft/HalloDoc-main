//importing all the classes and structures defined in the Models namespace of a project HalloDoc.
//By bringing these models into your current code scope, you can directly use them without having to specify the namespace repeatedly.This makes your code more concise and readable, especially when accessing these models frequently.
using Azure.Core;
using HalloDoc.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;

//This line imports the entire Microsoft.EntityFrameworkCore namespace into your code.Entity Framework Core(EF Core) is an object-relational mapper(ORM) framework from Microsoft that simplifies working with relational databases in .NET applications.It enables you to interact with database tables by creating corresponding C# classes (models) that map to those tables' columns.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.X86;
using System.Drawing;
using System.IO;
using HalloDoc.Data;

//Following function marks the starting point for building an ASP.NET Core web application by providing a flexible and efficient configuration API.
//It initializes a WebApplicationBuilder object and the parameter 'args' accepts an array of strings representing command-line arguments passed to your application when it starts.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(720); // Set session timeout
});


//builder.Services is used to add services to the dependency injection container.
//i) a dependency is when one object relies on another object to perform its function. For example, a Car object might depend on a Engine object to run.
//ii)  Injection refers to the passing of a dependency (usually as a parameter) to a dependent object. Instead of the dependent object creating its own dependencies, they are "injected" into it from an external source.
//iii) Dependency Injection Container is a mechanism used to manage the dependencies and perform the injections.

//IMP: AddDbContext is a method provided by the .NET Core dependency injection framework. It registers the HalloDocDbContext as a service in the application's service container, allowing other parts of the application to request instances of HalloDocDbContext as needed.

//(options => ...): This is a lambda expression that configures the DbContext options. Inside the lambda expression, options refers to an instance of DbContextOptionsBuilder, which is used to configure the DbContext.
//options.UseNpgsql(...): This configures the DbContext to use a PostgreSQL database. UseNpgsql is a method provided by Entity Framework Core's PostgreSQL provider (Npgsql.EntityFrameworkCore.PostgreSQL). It takes a connection string as an argument.
//builder.Configuration.GetConnectionString("HalloDocDbContext"): This retrieves the connection string named "HalloDocDbContext" from the application's configuration. The connection string typically contains information such as the server, database name, credentials, etc., needed to connect to the PostgreSQL database.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("HalloDocDbContext")));
var app = builder.Build();

// Configure the HTTP request pipeline.
//checks if current hosting environment is set to development
if (!app.Environment.IsDevelopment())
{
        //configures the application to use the exception handling middleware.In case an unhandled exception occurs during the processing of an HTTP request, this middleware will catch the exception and redirect the user to the specified error handling endpoint("/Home/Error" in this case). This endpoint is typically a controller action that renders an error page or performs other error - handling logic.
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    }
    //This middleware is used to redirect HTTP requests to HTTPS.
    app.UseHttpsRedirection();
    //This middleware enables serving of static files (such as HTML, CSS, JavaScript, images, etc.) from the web root directory or other specified locations within the application's file system.
    app.UseStaticFiles();
    //This middleware sets up routing for the application, enabling it to match incoming HTTP requests to the appropriate endpoint (e.g., controller action or Razor page handler). It inspects the incoming requests' URLs and routes them to the appropriate endpoint based on the configured routes.
    app.UseRouting();
    //This middleware is used to enable authorization in the application. It performs authentication and authorization checks for incoming requests based on the configured policies and requirements.
    app.UseAuthorization();

    app.UseSession();

    //This method configures MVC-style routing for controller actions. It defines a route named "default" with a pattern that specifies the default controller (LoginController) and action to be invoked when a request matches the specified pattern. The {id?} part indicates that the id parameter is optional.
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=PatientSite}/{id?}");
    //This method is the terminal middleware in the request processing pipeline. It delegates the handling of requests to the application's request-handling logic.
    app.Run();
