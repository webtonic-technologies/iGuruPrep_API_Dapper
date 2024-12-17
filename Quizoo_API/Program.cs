using Quizoo_API.Repository.Implementations;
using Quizoo_API.Repository.Interfaces;
using Quizoo_API.Services.Implementations;
using Quizoo_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
// Add services to the container.
builder.Services.AddTransient<IQuizooServices, QuizooService>();
builder.Services.AddTransient<IQuizooRepository, QuizooRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
