using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace COMICZONE.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum? enumValue)
        {
            if (enumValue == null)
                return "Không xác định";

            var member = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First();

            var attribute = member
                .GetCustomAttribute<DisplayAttribute>();

            return attribute?.Name ?? enumValue.ToString();
        }
    }
}