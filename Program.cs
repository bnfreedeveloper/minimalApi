using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimalApi.Data;
using minimalApi.Dtos;
using minimalApi.Models;
using minimalApi.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

//swagger configuration authentication
var security = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = " jwt authentication"
};
var requirements = new OpenApiSecurityRequirement{
    {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
                }
            },
            new string []{}
    }

};

var infos = new OpenApiInfo
{
    Version = "v1",
    Title = "command line api with jwt authentication",
    Description = "command line api with jwt authentication"
};
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", infos);
    options.AddSecurityDefinition("Bearer", security);
    options.AddSecurityRequirement(requirements);
});

var sqlConnection = new SqlConnectionStringBuilder();
sqlConnection.ConnectionString = builder.Configuration.GetConnectionString("Default");
sqlConnection.UserID = builder.Configuration["UserId"];
sqlConnection.Password = builder.Configuration["Password"];

builder.Services.AddDbContext<CommandDbContext>(option => option.UseSqlServer(sqlConnection.ConnectionString));
builder.Services.AddScoped<IRepository, CommandRepository>();
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(Program));

//authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
});

//builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("adminOnly", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "admin");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//route for authentication
app.MapPost("api/v1/accounts/login", [AllowAnonymous] async (requestDto user) =>
{
    //for demo only we hardcode the username and password
    //still we simulate a check on database
    if (await Task.FromResult(user.userName == "jeanvaljeant@gmail.com" && user.password == "hugo"))
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
        //for the choice of algorith in hosting, we balance with performance, for ex checking Ram available
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var jwtHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(new Claim[]{
              new Claim("Id","1"),
              new Claim(JwtRegisteredClaimNames.Sub,user.userName),
              new Claim(JwtRegisteredClaimNames.Email, user.userName),
              new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
              new Claim(ClaimTypes.Role,"admin")
            }),
            Expires = DateTime.Now.AddSeconds(50)
        };
        var token = jwtHandler.CreateToken(tokenDescriptor);
        var jwtoken = jwtHandler.WriteToken(token);
        return Results.Ok(jwtoken);
    }
    return Results.Unauthorized();
});
app.MapGet("api/v1/commands", [AllowAnonymous] async (IRepository repo, IMapper mapper) =>
{
    var commands = await repo.GetAllAsync();
    return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
}).Produces<IEnumerable<CommandReadDto>>()
  .WithName("GetALLCommands");

app.MapGet("api/v1/commands/{id}", [AllowAnonymous] async (IRepository repo, IMapper mapper, [FromRoute] int id) =>
{
    var result = await repo.GetByIdAsync(id);
    if (result != null)
    {
        return Results.Ok(mapper.Map<CommandReadDto>(result));
    }
    return Results.BadRequest(new
    {
        error = "no command at this id"
    });

}).Produces<CommandReadDto>().Produces(404).WithName("getCommand");

app.MapPost("api/v1/commands", [Authorize(policy: "adminOnly")] async (IRepository repo, IMapper mapper, [FromBody] CommandCreateDto cmd) =>
{
    var commandModel = mapper.Map<Command>(cmd);
    await repo.CreateAsync(commandModel);
    await repo.SaveChangesAsync();
    var commandDto = mapper.Map<CommandReadDto>(commandModel);
    return Results.CreatedAtRoute("getCommand", new { id = commandModel.Id }, commandDto);

});

app.MapPut("api/v1/commands/{id}", async (IRepository repo, IMapper mapper, [FromRoute] int id, CommandCreateDto cmd) =>
{
    var command = await repo.GetByIdAsync(id);
    if (command != null)
    {
        mapper.Map(cmd, command);
        await repo.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.BadRequest(new
    {
        error = "no command at this id"
    });
}).Produces(StatusCodes.Status201Created).Produces(StatusCodes.Status400BadRequest).WithName("updateCommand");
app.MapDelete("api/vs1/commands/{id}", async (IRepository repo, IMapper mapper, [FromRoute] int id) =>
{
    var command = await repo.GetByIdAsync(id);
    if (command != null)
    {
        repo.Delete(command);
        await repo.SaveChangesAsync();
        return Results.NoContent();

    }
    return Results.BadRequest(new
    {
        error = "no command found for this id"
    });
}).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound)
.WithName("deleteCommand");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

//for authentication
record requestDto(string userName, string password);