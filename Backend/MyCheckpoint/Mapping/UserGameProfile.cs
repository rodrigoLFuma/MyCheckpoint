using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class UserGameProfile : Profile
    {
        public UserGameProfile()
        {
            CreateMap<UserGame, UserGameDto>()
                .ForMember(dest => dest.UserSummary,
                    opt => opt.MapFrom(src => src.User));

            CreateMap<UserGameAddDto, UserGame>();
            CreateMap<UserGameModifyDto, UserGame>();
        }
    }
}
