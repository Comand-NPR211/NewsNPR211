using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data.Entities;
using WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using WebAPI.Models.News;
using WebAPI.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IImageHulk _imageHulk;
        private readonly IMapper _mapper;

        public NewsController(AppDbContext context, IImageHulk imageHulk, IMapper mapper)
        {
            _context = context;
            _imageHulk = imageHulk;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsEntity>>> GetNews()
        {
            var news = await _context.News
                .ProjectTo<NewsItemViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(news);
        }

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

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] NewsCreateViewModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Category))
            {
                return BadRequest("Категорія не може бути порожньою");
            }

            // Шукаємо категорію, якщо її немає - створюємо
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == model.Category);
            if (category == null)
            {
                category = new Category { Name = model.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(); // Зберігаємо нову категорію
            }



            string imageName = string.Empty;

            if (model.ImageFile != null)
            {
                imageName = await _imageHulk.Save(model.ImageFile);
            }
            var entity = _mapper.Map<NewsEntity>(model);
            entity.ImageUrl = imageName;

            entity.CategoryId = category.Id; // Встановлюємо ID категорії
            entity.Category = null; // Уникаємо циклу в JSON

            _context.News.Add(entity);

            //_context.SaveChanges();
            await _context.SaveChangesAsync();

            //return Ok(entity);
            return Ok(new { entity.Id, entity.Title, entity.CategoryId }); // Уникаємо рекурсивного JSON



        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News.SingleOrDefaultAsync(c => c.Id == id);
            if (news == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(news.ImageUrl))
            {
                _imageHulk.Delete(news.ImageUrl);
            }
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Edit([FromForm] NewsEditViewModel model)
        {
            // Знайти новину за Id
            var news = await _context.News.SingleOrDefaultAsync(c => c.Id == model.Id);

            // Якщо новину не знайдено, повертаємо 404
            if (news == null)
            {
                return NotFound();
            }

            // Мапінг інших полів
            _mapper.Map(model, news);

            // Перевірка, чи передано зображення
            if (model.ImageUrl != null)
            {
                // Якщо вже є зображення в новині, видаляємо його
                if (news.ImageUrl != null)
                {
                    _imageHulk.Delete(news.ImageUrl);
                }

                // Збереження нового зображення
                news.ImageUrl = await _imageHulk.Save(model.ImageUrl);
            }
            else
            {
                // Якщо поле ImageUrl не передано, скидаємо значення
                news.ImageUrl = null;
            }

            // Зберігаємо зміни в базі даних
            await _context.SaveChangesAsync();

            // Повертаємо успішний результат
            return Ok(news);
        }

        //[HttpGet("by-category/{categoryId}")]
        //public async Task<IActionResult> GetNewsByCategory(int categoryId)
        //{
        //    var news = await _context.News
        //        .Where(n => n.CategoryId == categoryId)
        //        .ToListAsync();

        //    if (!news.Any())
        //        return NotFound("Новин у цій категорії немає");

        //    return Ok(_mapper.Map<List<NewsItemViewModel>>(news));
        //}

        [HttpGet("by-category/{categoryId}")] // ефективніший варіант
        public async Task<IActionResult> GetNewsByCategory(int categoryId)
        {
            var news = await _context.News
                .Where(n => n.CategoryId == categoryId)
                .ProjectTo<NewsItemViewModel>(_mapper.ConfigurationProvider)  // Виконуємо мапінг одразу у запиті
                .ToListAsync();

            if (!news.Any())
                return NotFound("Новин у цій категорії немає");

            return Ok(news);
        }



    }
}
