using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.Sqlite;
using Dapper;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/", async (HttpContext context, IAntiforgery antiforgery) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    
    string htmlContent = await File.ReadAllTextAsync("wwwroot/index.html");
    htmlContent = htmlContent.Replace("__RequestToken__", tokens.RequestToken);
    htmlContent = htmlContent.Replace("__FormFieldName__", tokens.FormFieldName);

    return Results.Content(htmlContent, "text/html");
});

app.MapGet("/api/cities", async () =>
{
    using var connection = new SqliteConnection(connectionString);

    connection.Open();

    var cities = await connection.QueryAsync<City>("SELECT * FROM Cities");
    var html = "";

    foreach (var city in cities)
    {
        html += $"""
            <div class="col g-2 mx-2 city-mn-width p-3 bg-dark rounded-3 text-light" id="{city.Id}">
                {city.Name}
            </div>
        """;
    }


    return Results.Content(html, "text/html");
});

app.MapGet("/api/options", async () =>
{
    using var connection = new SqliteConnection(connectionString);

    connection.Open();

    var cities = await connection.QueryAsync<City>("SELECT * FROM Cities");
    var html = $"""
        <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
            Pick a city to delete
        </button>
        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton1">
    """;

    foreach (var city in cities)
    {
        html += $"""
            <li><a class="dropdown-item delete-city" data-city-id="{city.Id}" href="#" onclick="redirectToDeleteApi(event, this)">{city.Name}</a></li>
        """;
    }

    html += "</ul>";

    return Results.Content(html, "text/html");
});

app.MapPost("/admin/cities/add", async (
                        [FromForm] string name, 
                        HttpContext context,
                        IAntiforgery antiforgery
                        ) =>
{
    await antiforgery.ValidateRequestAsync(context);

    City city = new() { Id = Guid.NewGuid().ToString(), Name = name };

    using var connection = new SqliteConnection(connectionString);

    connection.Open();

    var sql = "INSERT INTO Cities (Id, Name) VALUES (@Id, @Name)";
    int rowsAffected = await connection.ExecuteAsync(sql, city);

    return Results.NoContent();
});

app.MapDelete("/admin/cities/delete/{Id}", async (string Id) => 
{
    using var connection = new SqliteConnection(connectionString);

    connection.Open();

    var sql = "DELETE FROM Cities WHERE Id = @Id";
    int rowsAffected = await connection.ExecuteAsync(sql, new { Id });

    return Results.NoContent();
});

app.Run();

public class City
{
        public required string Id { get; set; }
        public string? Name { get; set; }
}