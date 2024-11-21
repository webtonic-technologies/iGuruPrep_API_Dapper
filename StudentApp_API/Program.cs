using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentApp_API.Repository.Implementations;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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

builder.Services.AddTransient<IProjectForStudentsServices, ProjectForStudentsServices>();
builder.Services.AddTransient<IProjectForStudentsRepository, ProjectForStudentsRepository>();

builder.Services.AddTransient<IRefresherGuideRepository, RefresherGuideRepository>();
builder.Services.AddTransient<IRefresherGuideServices, RefresherGuideServices>();

builder.Services.AddTransient<IBoardPapersRepository, BoardPapersRepositiry>();
builder.Services.AddTransient<IBoardPapersServices, BoardPapersServices>();

// Add other services as needed in the future
// builder.Services.AddTransient<IOtherService, OtherService>();
// builder.Services.AddTransient<IOtherRepository, OtherRepository>();
builder.Services.AddSingleton<JwtHelper>();

builder.Services.AddAuthentication(cfg =>
{
    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8
            .GetBytes(builder.Configuration["Jwt:Key"])
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ControlPanel API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
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

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();

app.UseCors("corsapp");

app.MapControllers();

app.Run();