namespace BugsFarm.Utility
{
    public static class PathConstants
    {
        public const string UserPath = "users/{0}";
        public const string UserDataPath = "usersData/{0}";

        public static string GetUserPath(string userId)
        {
            return string.Format(UserPath, userId);
        }
        
        public static string GetUserDataPath(string userId)
        {
            return string.Format(UserDataPath, userId);
        }
    }
}