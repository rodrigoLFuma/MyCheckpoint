using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<User, UserSumaryDto>();

            CreateMap<UserAddDto, User>();
            CreateMap<UserModifyDto, User>();
        }
    }
}
