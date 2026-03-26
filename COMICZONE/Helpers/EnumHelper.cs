namespace COMICZONE.Helpers
{
    public static class EnumHelper
    {
        public static T ParseOrThrow<T>(string? value) where T : struct
        {
            if (Enum.TryParse<T>(value ?? "", true, out var result))
                return result;

            throw new Exception($"Invalid enum value: {value}");
        }
    }
}