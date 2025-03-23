using AutoMapper;
using WebAPI.Data.Entities;
using WebAPI.Models.News;
using WebAPI.Models.Category;
using System.Text.Json;

namespace WebAPI.Mapper
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<NewsEntity, NewsItemViewModel>()
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore());

            CreateMap<NewsCreateViewModel, NewsEntity>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())  // заповнюється вручну
            .ForMember(dest => dest.Category, opt => opt.Ignore()); // Додаємо ігнорування, заповнюється вручну

            CreateMap<NewsEditViewModel, NewsEntity>()
                .ForMember(x => x.ImageUrl, opt => opt.Ignore());

            CreateMap<CategoryCreateViewModel, Category>();
            CreateMap<Category, CategoryViewModel>();

        }
    }
}
