using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data.Entities;
using WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/news all
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsEntity>>> GetNews()
        {
            var news = await _context.News.ToListAsync();
            return Ok(news);
        }

        // GET: api/news element
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsEntity>> GetNews(int id)
        {
            var news_element = await _context.News.FindAsync(id);

            if (news_element == null)
            {
                return NotFound();
            }

            return Ok(news_element);
        }
    }
}
