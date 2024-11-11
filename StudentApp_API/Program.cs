using StudentApp_API.Repository.Implementations;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Register the IDbConnection to use the connection string for SQL Server
builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));

// Registering the services and repositories for the Registration module
builder.Services.AddTransient<IRegistrationService, RegistrationService>();
builder.Services.AddTransient<IRegistrationRepository, RegistrationRepository>();

builder.Services.AddTransient<ICourseService, CourseService>();
builder.Services.AddTransient<ICourseRepository, CourseRepository>();

builder.Services.AddTransient<IClassCourseService, ClassCourseService>();
builder.Services.AddTransient<IClassCourseRepository, ClassCourseRepository>();

builder.Services.AddTransient<IScholarshipService, ScholarshipService>();
builder.Services.AddTransient<IScholarshipRepository, ScholarshipRepository>();


// Add other services as needed in the future
// builder.Services.AddTransient<IOtherService, OtherService>();
// builder.Services.AddTransient<IOtherRepository, OtherRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS to allow requests from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corsapp");

app.UseAuthorization();

app.MapControllers();

app.Run();
