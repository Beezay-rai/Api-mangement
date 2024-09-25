using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// Configure the HTTP request pipeline.

app.MapGet("/getpets", () =>
{
    return new Pet[2] { new Pet { name = "tom", type = "cat" }, new Pet { name = "billy", type = "dog" } };
}).WithName("GetPets");

app.MapPost("/addPet", ([FromHeader] string createdBy, [FromBody] Pet pet) =>
{
    return new Pet { name = pet.name, type = pet.type };
}).WithName("AddPet");

app.Run();

internal class Pet
{
    public string name { get; set; }
    public string type { get; set; }
}

