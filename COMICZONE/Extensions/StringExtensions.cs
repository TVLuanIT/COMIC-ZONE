namespace COMICZONE.Extensions
{
    public static class StringExtensions
    {
        public static string AvatarOrDefault(this string? path, string defaultPath = "/uploads/avatar/default-avatar.png")
        {
            return string.IsNullOrEmpty(path)
                ? defaultPath
                : path;
        }
    }
}
