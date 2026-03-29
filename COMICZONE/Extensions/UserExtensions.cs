using COMICZONE.Models;

namespace COMICZONE.Extensions
{
    public static class UserExtensions
    {
        public static string GetDisplayUsername(this User? user)
        {
            if (user == null) return "Người dùng ẩn danh";
            if (user.Isdeleted) return "Người dùng đã xóa";
            
            return string.IsNullOrEmpty(user.Username) ? "Người dùng ẩn danh" : user.Username;
        }

        public static string GetDisplayAvatar(this User? user)
        {
            if (user == null || user.Isdeleted) 
                return "/uploads/avatar/delete-avatar.webp";

            return user.Avatar.AvatarOrDefault();
        }
    }
}
