using AutoMapper;
using WebAPI.Data.Entities;
using WebAPI.Models.News;
using WebAPI.Models.Category;

namespace WebAPI.Mapper
{
    public class MapProfile : Profile
    {
        public MapProfile() 
        {
            CreateMap<NewsEntity, NewsItemViewModel>();

            CreateMap<NewsCreateViewModel, NewsEntity>()
                .ForMember(x => x.ImageUrl, opt => opt.MapFrom(src => src.ImageFile));

            CreateMap<NewsEditViewModel, NewsEntity>()
                .ForMember(x => x.ImageUrl, opt => opt.Ignore());

            CreateMap<CategoryCreateViewModel, Category>();
            CreateMap<Category, CategoryViewModel>();
            CreateMap<NewsCreateViewModel, NewsEntity>()
    .ForMember(dest => dest.Category, opt => opt.Ignore()); // Додаємо ігнорування
        }
    }
}
