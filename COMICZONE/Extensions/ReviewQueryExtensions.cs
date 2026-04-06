using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class ReviewQueryExtensions
    {
        public static IQueryable<ProductReview> ApplyReviewFilters(this IQueryable<ProductReview> query, ReviewSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (ReviewId, Username, Product Name, Content)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(r => 
                    r.Reviewid.ToString() == keyword ||
                    r.User.Username.ToLower().Contains(keyword) ||
                    r.Product.Name.ToLower().Contains(keyword) ||
                    r.Reviewcontent.ToLower().Contains(keyword)
                );
            }

            // 2. Exact Field Matches
            if (search.ReviewId.HasValue)
                query = query.Where(r => r.Reviewid == search.ReviewId.Value);

            if (search.ProductId.HasValue)
                query = query.Where(r => r.Productid == search.ProductId.Value);

            if (search.UserId.HasValue)
                query = query.Where(r => r.Userid == search.UserId.Value);

            // 3. User Context
            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(r => r.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(r => r.User.Email != null && r.User.Email.Contains(search.UserEmail));

            if (!string.IsNullOrWhiteSpace(search.CustomerPhoneNumber))
                query = query.Where(r => r.User.Customer != null && r.User.Customer.Phone != null && r.User.Customer.Phone.Contains(search.CustomerPhoneNumber));

            // 4. Product Context
            if (!string.IsNullOrWhiteSpace(search.ProductName))
                query = query.Where(r => r.Product.Name.Contains(search.ProductName));

            // 5. Ratings
            if (search.Rating.HasValue)
                query = query.Where(r => r.Rating == search.Rating.Value);

            if (search.RatingMin.HasValue)
                query = query.Where(r => r.Rating >= search.RatingMin.Value);

            if (search.RatingMax.HasValue)
                query = query.Where(r => r.Rating <= search.RatingMax.Value);

            // 6. Content Keyword
            if (!string.IsNullOrWhiteSpace(search.ReviewContentKeyword))
                query = query.Where(r => r.Reviewcontent.Contains(search.ReviewContentKeyword));

            // 7. Timelines
            if (search.CreatedFrom.HasValue)
                query = query.Where(r => r.Createdat >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
                query = query.Where(r => r.Createdat <= search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1));

            if (search.UpdatedFrom.HasValue)
                query = query.Where(r => r.Updatedat >= search.UpdatedFrom.Value);

            if (search.UpdatedTo.HasValue)
                query = query.Where(r => r.Updatedat <= search.UpdatedTo.Value.Date.AddDays(1).AddTicks(-1));

            // 8. Flags
            if (search.IsDeleted.HasValue)
                query = query.Where(r => r.Isdeleted == search.IsDeleted.Value);

            if (search.HasReplies == true)
                query = query.Where(r => r.ProductReviewReplies.Any());
            else if (search.HasReplies == false)
                query = query.Where(r => !r.ProductReviewReplies.Any());

            if (search.NoReplyOnly == true)
                query = query.Where(r => !r.ProductReviewReplies.Any());

            if (search.HasAdminReply == true)
                query = query.Where(r => r.ProductReviewReplies.Any(rp => rp.User.Role == "ADMIN"));

            // 9. Counts
            if (search.ReplyCountMin.HasValue)
                query = query.Where(r => r.ProductReviewReplies.Count() >= search.ReplyCountMin.Value);

            if (search.ReplyCountMax.HasValue)
                query = query.Where(r => r.ProductReviewReplies.Count() <= search.ReplyCountMax.Value);

            if (search.LikeCountMin.HasValue)
                query = query.Where(r => r.ProductReviewLikes.Count() >= search.LikeCountMin.Value);

            if (search.LikeCountMax.HasValue)
                query = query.Where(r => r.ProductReviewLikes.Count() <= search.LikeCountMax.Value);

            return query;
        }

        public static IQueryable<ProductReviewReply> ApplyReplyFilters(this IQueryable<ProductReviewReply> query, ReviewReplySearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (ReplyId, ReviewId, Username, Content)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(r => 
                    r.Replyid.ToString() == keyword ||
                    r.Reviewid.ToString() == keyword ||
                    r.User.Username.ToLower().Contains(keyword) ||
                    r.Replycontent.ToLower().Contains(keyword) ||
                    (r.Review.Product.Name != null && r.Review.Product.Name.ToLower().Contains(keyword))
                );
            }

            // 2. IDs
            if (search.ReplyId.HasValue)
                query = query.Where(r => r.Replyid == search.ReplyId.Value);

            if (search.ReviewId.HasValue)
                query = query.Where(r => r.Reviewid == search.ReviewId.Value);

            if (search.ProductId.HasValue)
                query = query.Where(r => r.Review.Productid == search.ProductId.Value);

            if (search.UserId.HasValue)
                query = query.Where(r => r.Userid == search.UserId.Value);

            if (search.ParentReplyId.HasValue)
                query = query.Where(r => r.Parentreplyid == search.ParentReplyId.Value);

            // 3. User Context
            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(r => r.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(r => r.User.Email != null && r.User.Email.Contains(search.UserEmail));

            if (!string.IsNullOrWhiteSpace(search.ProductName))
                query = query.Where(r => r.Review.Product.Name.Contains(search.ProductName));

            // 4. Target User
            if (search.ReplyToUserId.HasValue)
                query = query.Where(r => r.Replytouserid == search.ReplyToUserId.Value);

            if (!string.IsNullOrWhiteSpace(search.ReplyToUsername))
                query = query.Where(r => r.Replytouser != null && r.Replytouser.Username.Contains(search.ReplyToUsername));

            // 5. Hierarchy flags
            if (search.TopLevelOnly == true)
                query = query.Where(r => r.Parentreplyid == null);

            if (search.ChildReplyOnly == true)
                query = query.Where(r => r.Parentreplyid != null);

            if (search.NoChildReplyOnly == true)
                query = query.Where(r => !r.InverseParentreply.Any());

            // 6. Timelines
            if (search.CreatedFrom.HasValue)
                query = query.Where(r => r.Createdat >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
                query = query.Where(r => r.Createdat <= search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1));

            // 7. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(r => r.Isdeleted == search.IsDeleted.Value);

            // 8. Counts
            if (search.LikeCountMin.HasValue)
                query = query.Where(r => r.ProductReviewReplyLikes.Count() >= search.LikeCountMin.Value);

            if (search.LikeCountMax.HasValue)
                query = query.Where(r => r.ProductReviewReplyLikes.Count() <= search.LikeCountMax.Value);

            return query;
        }
    }
}
