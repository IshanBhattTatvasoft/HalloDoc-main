//importing all the classes and structures defined in the Models namespace of a project HalloDoc.
//By bringing these models into your current code scope, you can directly use them without having to specify the namespace repeatedly.This makes your code more concise and readable, especially when accessing these models frequently.
using DocumentFormat.OpenXml.InkML;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDoc.LogicLayer.Patient_Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;
using System.Text;

// creates WebApplicationBuilder instance to configure the web application
var builder = WebApplication.CreateBuilder(args);
// retrieves IConfiguration instance and provides access to configuration settings for the application
ConfigurationManager configuration = builder.Configuration;

// adds services necessary for MVC (Model-View-Controller) pattern support. It registers services required for controllers and views.
builder.Services.AddControllersWithViews();

// registers the HttpContextAccessor, which allows access to the HTTP context within the application.
builder.Services.AddHttpContextAccessor();  

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set session timeout
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("HalloDocDbContext"), providerOptions =>
{
    providerOptions.CommandTimeout(180);
}));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ILoginPage, LoginPage>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPatientRequest, PatientRequest>();
builder.Services.AddScoped<IFamilyRequest, FamilyRequest>();
builder.Services.AddScoped<IBusinessRequest, BusinessRequest>();
builder.Services.AddScoped<IConciergeRequest, ConciergeRequest>();
builder.Services.AddScoped<IPatientDashboard, PatientDashboard>();
builder.Services.AddScoped<IViewDocuments, ViewDocuments>();
builder.Services.AddScoped<IPatientProfile,  PatientProfile>();
builder.Services.AddScoped<ICreateRequestForMe, CreateRequestForMe>();
builder.Services.AddScoped<ICreateRequestForSomeoneElse,  CreateRequestForSomeoneElse>();
builder.Services.AddScoped<IAdminInterface, AdminInterface>();
builder.Services.AddScoped<IJwtToken, JwtToken>();
builder.Services.AddScoped<IProviderInterface, ProviderInterface>();
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

app.UseRotativa();
//This method is the terminal middleware in the request processing pipeline. It delegates the handling of requests to the application's request-handling logic.
app.Run();
