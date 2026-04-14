using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class MarketplacePost
    {
        [NotMapped]
        public MarketplacePostCondition ConditionEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Condition))
                    return MarketplacePostCondition.Acceptable;

                try
                {
                    return EnumHelper.ParseOrThrow<MarketplacePostCondition>(Condition);
                }
                catch
                {
                    return MarketplacePostCondition.Acceptable;
                }
            }
            set => Condition = value.ToString();
        }

        [NotMapped]
        public MarketplacePostCategory CategoryEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Category))
                    return MarketplacePostCategory.Others;

                try
                {
                    return EnumHelper.ParseOrThrow<MarketplacePostCategory>(Category);
                }
                catch
                {
                    return MarketplacePostCategory.Others;
                }
            }
            set => Category = value.ToString();
        }

        [NotMapped]
        public MarketplacePostStatus StatusEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status))
                    return MarketplacePostStatus.Pending;

                try
                {
                    return EnumHelper.ParseOrThrow<MarketplacePostStatus>(Status);
                }
                catch
                {
                    return MarketplacePostStatus.Pending;
                }
            }
            set => Status = value.ToString();
        }
    }
}
