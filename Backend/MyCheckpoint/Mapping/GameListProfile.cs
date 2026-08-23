using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class GameListProfile : Profile
    {
        public GameListProfile()
        {
            CreateMap<GameList, GameListDto>()
                .ForMember(dest => dest.Games,
                    opt => opt.MapFrom(src => src.GameListItems));

            CreateMap<GameListAddDto, GameList>();
            CreateMap<GameListModifyDto, GameList>();
        }
    }
}
