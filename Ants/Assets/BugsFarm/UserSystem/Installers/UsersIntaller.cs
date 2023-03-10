using Zenject;

namespace BugsFarm.UserSystem
{
    public class UsersIntaller : Installer<UsersIntaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind(typeof(IUser), typeof(IUserInternal))
                .To<User>()
                .AsSingle();
        }
    }
}