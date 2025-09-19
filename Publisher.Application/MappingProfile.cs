using AutoMapper;
using Publisher.Domain;
using Publisher.Application.Models;

namespace Publisher.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Article, ArticleDto>().ReverseMap();
        CreateMap<Opinion, OpinionDto>().ReverseMap();
    }
}
