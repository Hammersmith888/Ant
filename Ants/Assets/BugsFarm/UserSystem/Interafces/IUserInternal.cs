namespace BugsFarm.UserSystem
{
    public interface IUserInternal : IUser
    {
        void InitializeInternal(UserDto userDto);
    }
}