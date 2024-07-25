using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoApi;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/allCustomers", async (TodoDb db) =>
    await db.Todos.ToListAsync());


app.MapGet("/Customer/country", async (TodoDb db) =>
    await db.Todos.Where(t => t.Country == "Pakistan").ToListAsync());

app.MapGet("/Customer/{CustomerId}", async (int CustomerId, TodoDb db) =>
{
    var customer = await db.Todos.FindAsync(CustomerId);
    return customer != null ? Results.Ok(customer) : Results.NotFound();
});


app.MapPost("/Customers", async (HttpRequest request, TodoDb db) =>
{
    try
    {
        var customer = await JsonSerializer.DeserializeAsync<Customer>(request.Body);
        db.Todos.Add(customer);
        await db.SaveChangesAsync();
        return Results.Created($"/Customers/{customer.CustomerId}", customer);
    }
    catch (JsonException ex)
    {
        // Log the detailed exception
        app.Logger.LogError(ex, "JSON parsing error");
        return Results.BadRequest(new { message = "Invalid JSON format" });
    }
});

app.MapPut("/Customers/{CustomerId}", async (int CustomerId, HttpRequest request, TodoDb db) =>
{
    try
    {
        var inputCustomer = await JsonSerializer.DeserializeAsync<Customer>(request.Body);
        var customer = await db.Todos.FindAsync(CustomerId);

        if (customer is null) return Results.NotFound();

        customer.CustomerName = inputCustomer.CustomerName;
        customer.Country = inputCustomer.Country;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (JsonException ex)
    {
        // Log the detailed exception
        app.Logger.LogError(ex, "JSON parsing error");
        return Results.BadRequest(new { message = "Invalid JSON format" });
    }
});

app.MapDelete("/Customers/{CustomerId}", async (int CustomerId, TodoDb db) =>
{
    if (await db.Todos.FindAsync(CustomerId) is Customer customer)
    {
        db.Todos.Remove(customer);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});


app.Run();