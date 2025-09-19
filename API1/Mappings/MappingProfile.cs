using AutoMapper;
using API1.Data;
using API1.Models;

namespace API1.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
        }
    }
}
