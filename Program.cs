using API_Credit_Card.Data;
using API_Credit_Card.Models;
using API_Credit_Card.Repository.IRepository;
using API_Credit_Card.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API_Credit_Card.AppMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySQLConn");

// secret key from appsettings.json
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

// Add services to the container.

// mysql connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// identity service
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

// Caché
builder.Services.AddResponseCaching();

// repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();

// CORS
builder.Services.AddCors(p => p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

//Auto mapper
builder.Services.AddAutoMapper(typeof(AppMapper));

// add authentication to swagger
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers(option =>
{
    option.CacheProfiles.Add("Default10Sec", new CacheProfile() { Duration = 10 });
});

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.ToString());

    options.SwaggerDoc("users", new OpenApiInfo
    {
        Title = "users",
        Version = "v1",
    });

    options.SwaggerDoc("credit_card", new OpenApiInfo
    {
        Title = "credit_card",
        Version = "v1",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticando JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la palabra 'Beare' seguida de un [espacio] y luego su token en el campo de abajo \r\n\r\n" +
        "Ej: \"Bearer tdadahsdh\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    
//}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/users/swagger.json", "users");
    options.SwaggerEndpoint("/swagger/credit_card/swagger.json", "credit_card");
});

app.UseRouting();

// CORS
app.UseCors("PolicyCors");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
