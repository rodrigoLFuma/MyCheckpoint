using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class GameListItemProfile : Profile
    {
        public GameListItemProfile()
        {
            CreateMap<GameListItem, GameListItemDto>()
                .ForMember(dest => dest.AddedAt,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.AddedAt)));

            CreateMap<GameListItemAddDto, GameListItem>();
        }
    }
}
