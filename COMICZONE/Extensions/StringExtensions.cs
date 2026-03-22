namespace COMICZONE.Extensions
{
    public static class StringExtensions
    {
        public static string AvatarOrDefault(this string? path, string defaultPath = "/images/no-image.png")
        {
            return string.IsNullOrEmpty(path)
                ? defaultPath
                : path;
        }
    }
}
