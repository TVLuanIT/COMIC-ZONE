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

        public static string ToVnd(this decimal value)
        {
            return string.Format("{0:N0} ₫", value);
        }
    }
}
