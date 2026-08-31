using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FavoriteGenres,
                    opt => opt.MapFrom(src => src.FavoriteGenres.Select(fg => fg.Category)));
            CreateMap<User, UserSummaryDto>();

            CreateMap<UserAddDto, User>();

            // FavoriteGenres se reconcilia a mano en UserService (mismo motivo
            // que GameCategories en GameService: evita pisar una colección ya
            // rastreada por EF Core).
            CreateMap<UserModifyDto, User>()
                .ForMember(dest => dest.FavoriteGenres, opt => opt.Ignore());
        }
    }
}
