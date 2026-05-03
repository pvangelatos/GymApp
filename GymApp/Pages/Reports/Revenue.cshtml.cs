using GymApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Reports
{
    public class RevenueModel : PageModel
    {
        private readonly AppDbContext _context;

        public RevenueModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MonthlyRevenue> MonthlyData { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal AverageMonthlyRevenue { get; set; }

        public async Task OnGetAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Subscription)
                .ToListAsync();

            MonthlyData = payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(p => p.Amount),
                    SubscriptionCount = g.Count()
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            TotalRevenue = MonthlyData.Sum(m => m.Revenue);
            AverageMonthlyRevenue = MonthlyData.Any()
                ? TotalRevenue / MonthlyData.Count
                : 0;
        }

        public async Task<IActionResult> OnGetExportAsync()
        {

            var payments = await _context.Payments
                .Include(p => p.Subscription)
                .ToListAsync();

            var monthlyData = payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1)
                        .ToString("MMMM yyyy", new System.Globalization.CultureInfo("el-GR")),
                    Count = g.Count(),
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderByDescending(m => m.MonthName)
                .ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Έσοδα ανά Μήνα");

            // Headers
            worksheet.Cell(1, 1).Value = "Μήνας";
            worksheet.Cell(1, 2).Value = "Αριθμός Συνδρομών";
            worksheet.Cell(1, 3).Value = "Έσοδα (€)";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkGreen;
            headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            // Data
            for (int i = 0; i < monthlyData.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = monthlyData[i].MonthName;
                worksheet.Cell(i + 2, 2).Value = monthlyData[i].Count;
                worksheet.Cell(i + 2, 3).Value = (double)monthlyData[i].Revenue;
                worksheet.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0.00 €";
            }

            // Total row
            var lastRow = monthlyData.Count + 2;
            worksheet.Cell(lastRow, 1).Value = "Σύνολο";
            worksheet.Cell(lastRow, 2).Value = monthlyData.Sum(m => m.Count);
            worksheet.Cell(lastRow, 3).Value = (double)monthlyData.Sum(m => m.Revenue);
            worksheet.Cell(lastRow, 3).Style.NumberFormat.Format = "#,##0.00 €";

            var totalRange = worksheet.Range(lastRow, 1, lastRow, 3);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Εσοδα_{DateTime.Today:yyyy-MM-dd}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
        public string MonthName => new DateTime(Year, Month, 1)
            .ToString("MMMM yyyy", new System.Globalization.CultureInfo("el-GR"));
    }
}
