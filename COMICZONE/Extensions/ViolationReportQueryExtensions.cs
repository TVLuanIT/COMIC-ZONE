using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Extensions
{
    public static class ViolationReportQueryExtensions
    {
        public static IQueryable<ViolationReport> ApplyViolationReportSearch(this IQueryable<ViolationReport> query, ViolationReportSearchRequest request)
        {
            if (request == null) return query;

            // 1. Basic Filters
            if (request.Id.HasValue)
            {
                query = query.Where(v => v.Id == request.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string kw = request.Keyword.Trim();
                query = query.Where(v => v.Reason.Contains(kw) ||
                                         v.Id.ToString() == kw ||
                                         v.Targetid.ToString() == kw ||
                                         (v.User != null && v.User.Username.Contains(kw)));
            }

            // 2. Reporter Filters
            if (request.UserId.HasValue)
            {
                query = query.Where(v => v.Userid == request.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                query = query.Where(v => v.User != null && v.User.Username.Contains(request.Username));
            }

            if (!string.IsNullOrWhiteSpace(request.UserEmail))
            {
                query = query.Where(v => v.User != null && v.User.Email.Contains(request.UserEmail));
            }

            if (!string.IsNullOrWhiteSpace(request.CustomerPhoneNumber))
            {
                query = query.Where(v => v.User != null && v.User.Customer != null && v.User.Customer.Phone.Contains(request.CustomerPhoneNumber));
            }

            // 3. Type & Target Filters
            if (request.ReportType.HasValue)
            {
                query = query.Where(v => v.Reporttype == request.ReportType.Value);
            }

            if (request.ReportTypes != null && request.ReportTypes.Any())
            {
                query = query.Where(v => request.ReportTypes.Contains(v.Reporttype));
            }

            if (request.TargetId.HasValue)
            {
                query = query.Where(v => v.Targetid == request.TargetId.Value);
            }

            // 4. Content Filters
            if (!string.IsNullOrWhiteSpace(request.ReasonKeyword))
            {
                query = query.Where(v => v.Reason.Contains(request.ReasonKeyword));
            }

            // 5. Status Filters
            if (request.Status.HasValue)
            {
                query = query.Where(v => v.Status == request.Status.Value);
            }

            if (request.Statuses != null && request.Statuses.Any())
            {
                query = query.Where(v => request.Statuses.Contains(v.Status));
            }

            if (request.UnresolvedOnly)
            {
                query = query.Where(v => v.Status == (int)ReportStatus.Pending);
            }

            // 6. Date & Soft Delete Filters
            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(v => v.Createdat >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                query = query.Where(v => v.Createdat <= request.CreatedTo.Value);
            }

            query = query.Where(v => v.Isdeleted == request.IsDeleted);

            // 7. Specialized Filters
            if (request.HighPriorityOnly)
            {
                // Unresolved reports older than 3 days are high priority
                var threshold = DateTime.Now.AddDays(-3);
                query = query.Where(v => v.Status == (int)ReportStatus.Pending && v.Createdat < threshold);
            }

            if (request.DuplicateTargetOnly)
            {
                // Use a sub-select to find targets with multiple reports
                var duplicateTargets = query.GroupBy(v => new { v.Reporttype, v.Targetid })
                                            .Where(g => g.Count() > 1)
                                            .Select(g => new { g.Key.Reporttype, g.Key.Targetid });

                query = query.Where(v => duplicateTargets.Any(d => d.Reporttype == v.Reporttype && d.Targetid == v.Targetid));
            }

            return query;
        }

        public static IQueryable<ViolationReport> ApplyViolationReportSort(this IQueryable<ViolationReport> query, string? sortColumn, bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return isAscending ? query.OrderBy(v => v.Createdat) : query.OrderByDescending(v => v.Createdat);
            }

            return sortColumn.ToLower() switch
            {
                "id" => isAscending ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
                "userid" => isAscending ? query.OrderBy(v => v.Userid) : query.OrderByDescending(v => v.Userid),
                "username" => isAscending ? query.OrderBy(v => v.User.Username) : query.OrderByDescending(v => v.User.Username),
                "reporttype" => isAscending ? query.OrderBy(v => v.Reporttype) : query.OrderByDescending(v => v.Reporttype),
                "targetid" => isAscending ? query.OrderBy(v => v.Targetid) : query.OrderByDescending(v => v.Targetid),
                "status" => isAscending ? query.OrderBy(v => v.Status) : query.OrderByDescending(v => v.Status),
                "createdat" => isAscending ? query.OrderBy(v => v.Createdat) : query.OrderByDescending(v => v.Createdat),
                _ => isAscending ? query.OrderBy(v => v.Createdat) : query.OrderByDescending(v => v.Createdat)
            };
        }
    }
}
