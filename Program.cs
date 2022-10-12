using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using minimalApi.Data;
using minimalApi.Dtos;
using minimalApi.Models;
using minimalApi.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqlConnection = new SqlConnectionStringBuilder();
sqlConnection.ConnectionString = builder.Configuration.GetConnectionString("Default");
sqlConnection.UserID = builder.Configuration["UserId"];
sqlConnection.Password = builder.Configuration["Password"];

builder.Services.AddDbContext<CommandDbContext>(option => option.UseSqlServer(sqlConnection.ConnectionString));
builder.Services.AddScoped<IRepository, CommandRepository>();
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("api/v1/commands", async (IRepository repo, IMapper mapper) =>
{
    var commands = await repo.GetAllAsync();
    return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
}).Produces<IEnumerable<CommandReadDto>>()
  .WithName("GetALLCommands");
app.MapGet("api/v1/commands/{id}", async (IRepository repo, IMapper mapper, [FromRoute] int id) =>
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

app.MapPost("api/v1/commands", async (IRepository repo, IMapper mapper, [FromBody] CommandCreateDto cmd) =>
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



app.Run();

