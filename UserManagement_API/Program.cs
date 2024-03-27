using System.Data;
using System.Data.SqlClient;
using UserManagement_API.Repository.Implementations;
using UserManagement_API.Repository.Interfaces;
using UserManagement_API.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
builder.Services.AddTransient<IUserRegistrationRepository, UserRegistrationRepository>();
builder.Services.AddTransient<IUserRegistrationServices, UserRegistrationServices>();
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
