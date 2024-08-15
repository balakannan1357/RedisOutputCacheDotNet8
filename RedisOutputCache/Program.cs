using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache(opt => opt.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5));

builder.Services.AddStackExchangeRedisOutputCache(opt =>
    {
        opt.InstanceName = "UserAPI";
        opt.Configuration = "localhost:32768,password=redispw";
    });

var app = builder.Build();

app.UseOutputCache();

app.MapGet("/user/{id}", async (string id) =>
{
    var url = $"https://reqres.in/api/users/{id}";
    using var httpClient = new HttpClient();
    var response = await httpClient.GetFromJsonAsync<dynamic>(url);

    return Results.Ok(response);

}).CacheOutput(x => x.Tag("user"));

app.MapGet("/invalidate", async (IOutputCacheStore cacheStore) =>
{
    await cacheStore.EvictByTagAsync("user", CancellationToken.None);
    return Results.Ok();
});

app.Run();

