using Microsoft.AspNetCore.OutputCaching;
using RedisOutputCache;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisOutputCache(opt =>
{
    opt.InstanceName = "UserAPI";
    opt.Configuration = "localhost:6379";
});

builder.Services.AddOutputCache(opt =>
{
    opt.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
    
    opt.AddPolicy("CacheForTenMinutes", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(10));
    });

    opt.AddPolicy("CacheByIdWithTag", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(5))
              .SetVaryByQuery("id")
              .Tag("user");
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.UseHttpsRedirection();
app.UseOutputCache();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/user/{id}", async (string id) =>
{
    var url = $"https://reqres.in/api/users/{id}";
    using var httpClient = new HttpClient();
    var response = await httpClient.GetFromJsonAsync<User>(url);

    return Results.Ok(response);

}).CacheOutput(x => x.Tag("user"));

app.MapGet("/api/invalidate", async (IOutputCacheStore cacheStore) =>
{
    await cacheStore.EvictByTagAsync("user", CancellationToken.None);
    return Results.Ok();
});

app.Run();

