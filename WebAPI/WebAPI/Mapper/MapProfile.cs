using AutoMapper;
using WebAPI.Data.Entities;
using WebAPI.Models.News;

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
        }
    }
}
