using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace RedisOutputCache.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "CacheByIdWithTag")]
        public async Task<IActionResult> GetUser(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://reqres.in/api/users/{id}";
            var response = await client.GetFromJsonAsync<User>(url);

            if (response == null)
            {
                return NoContent();
            }

            return Ok(response);
        }

        [HttpPost("invalidate")]
        public async Task<IActionResult> InvalidateCache([FromServices] IOutputCacheStore cacheStore)
        {
            await cacheStore.EvictByTagAsync("user", CancellationToken.None);
            return Ok("Cache invalidated");
        }
    }
}
