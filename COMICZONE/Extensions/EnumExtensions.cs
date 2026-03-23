using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var member = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First();

            var attribute = member
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .Cast<DisplayAttribute>()
                .FirstOrDefault();

            return attribute?.Name ?? enumValue.ToString();
        }
    }
}
