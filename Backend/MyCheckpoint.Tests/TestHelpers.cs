using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Mapping;

namespace MyCheckpoint.Tests
{

    internal static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserGameProfile>();
                cfg.AddProfile<FollowProfile>();
                cfg.AddProfile<GameListProfile>();
                cfg.AddProfile<GameListItemProfile>();
                cfg.AddProfile<GameProfile>();
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<NotificationProfile>();
            }, NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        public static Mock<UserManager<User>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        public static Mock<RoleManager<Role>> CreateRoleManagerMock()
        {
            var store = new Mock<IRoleStore<Role>>();
            return new Mock<RoleManager<Role>>(store.Object, null, null, null, null);
        }
    }
}
