using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Mapping
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.Actor, opt => opt.MapFrom(src => src.ActorUser))
                .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.UserGame != null ? src.UserGame.Game.Name : null));
        }
    }
}
