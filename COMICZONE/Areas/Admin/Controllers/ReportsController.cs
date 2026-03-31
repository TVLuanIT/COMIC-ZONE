using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using COMICZONE.ViewModels.Admin.Reports;
using System;
using System.Linq;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public ReportsController(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var summary = new DashboardSummaryViewModel
            {
                TotalUsers = await _context.Orders
                    .Where(o => o.Status != "Cancelled")
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending")
            };

            // Tạm thời chưa tính RevenueGrowth, để logic sau
            summary.RevenueGrowth = 15.5m; // Ví dụ

            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesChartData(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now.Date;
            var start = startDate ?? end.AddDays(-6); // Mặc định 7 ngày gần nhất

            var salesData = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end.AddDays(1) && o.Status == "Completed")
                .GroupBy(o => o.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Đảm bảo đủ các ngày trong khoảng (kể cả những ngày không có doanh thu)
            var labels = new List<string>();
            var values = new List<decimal>();

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                labels.Add(date.ToString("dd/MM"));
                var daySales = salesData.FirstOrDefault(s => s.Date == date);
                values.Add(daySales?.Revenue ?? 0);
            }

            return Json(new { labels, values });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderStatusData(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now.Date;
            var start = startDate ?? end.AddDays(-30); // Mặc định 30 ngày cho trạng thái đơn

            var stats = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end.AddDays(1))
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now.Date;
            var start = startDate ?? end.AddDays(-30);

            var orders = await _context.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.Customer)
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end.AddDays(1))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Báo cáo doanh thu");
                var currentRow = 1;

                // Tiêu đề báo cáo
                worksheet.Cell(currentRow, 1).Value = "BÁO CÁO DOANH THU CHI TIẾT";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Từ ngày: {start:dd/MM/yyyy} - Đến ngày: {end:dd/MM/yyyy}";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow += 2;

                // Headers
                var headers = new[] { "STT", "Mã đơn", "Ngày đặt", "Khách hàng", "Tổng tiền", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(currentRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }

                // Data
                int stt = 1;
                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = stt++;
                    worksheet.Cell(currentRow, 2).Value = "#" + order.OrderId;
                    worksheet.Cell(currentRow, 3).Value = order.CreatedAt?.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(currentRow, 4).Value = order.User?.Customer?.Fullname ?? order.User?.Username ?? "Khách vãng lai";
                    worksheet.Cell(currentRow, 5).Value = order.TotalAmount;
                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0 \"₫\"";
                    worksheet.Cell(currentRow, 6).Value = order.Status;
                    
                    // Kẻ khung
                    worksheet.Range(currentRow, 1, currentRow, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }

                // Tính tổng cộng
                currentRow += 2;
                worksheet.Cell(currentRow, 4).Value = "TỔNG CỘNG:";
                worksheet.Cell(currentRow, 4).Style.Font.SetBold();
                var total = orders.Sum(o => o.TotalAmount);
                worksheet.Cell(currentRow, 5).Value = total;
                worksheet.Cell(currentRow, 5).Style.Font.SetBold().NumberFormat.Format = "#,##0 \"₫\"";

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Bao-cao-doanh-thu-{DateTime.Now:yyyyMMddHHmm}.xlsx");
                }
            }
        }
    }
}
