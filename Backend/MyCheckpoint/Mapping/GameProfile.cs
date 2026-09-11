using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class GameProfile : Profile
    {
        public GameProfile()
        {
            CreateMap<Game, GameDto>()
                .ForMember(dest => dest.Categories,
                    opt => opt.MapFrom(src => src.GameCategories.Select(gc => gc.Category)));

            CreateMap<GameAddDto, Game>()
                .ForMember(dest => dest.GameCategories, opt => opt.Ignore());

            CreateMap<GameModifyDto, Game>()
                .ForMember(dest => dest.GameCategories, opt => opt.Ignore());
        }
    }
}
