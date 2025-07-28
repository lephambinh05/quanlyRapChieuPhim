using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using CinemaManagement.Services;

namespace CinemaManagement.Controllers
{
    [ApiController]
    [Route("api/cron")]
    [AllowAnonymous]
    public class CronController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly BankingService _bankingService;

        public CronController(CinemaDbContext context, BankingService bankingService)
        {
            _context = context;
            _bankingService = bankingService;
        }

        [HttpGet("")]
        public async Task<IActionResult> RunAllCrons()
        {
            var log = new List<object>();
            int totalPaid = 0, totalCancelled = 0, totalReleasedSeats = 0;
            var releasedSeatDetails = new List<object>();
            var skippedDetails = new List<object>();

            // 1. Kiểm tra thanh toán ngân hàng
            var bankingResult = await _bankingService.CheckBankingTransactionsAsync();
            totalPaid = bankingResult.UpdatedOrders;
            log.Add(new { step = "banking", result = bankingResult });

            // 2. Hủy đơn quá hạn
            var now = DateTime.Now;
            var expiredOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản" && EF.Functions.DateDiffMinute(h.ThoiGianTao, now) >= 2)
                .ToListAsync();
            foreach (var order in expiredOrders)
            {
                order.TrangThai = "Đã hủy";
                totalCancelled++;
                var tempItems = _context.TempGioHangItems.Where(x => x.MaHoaDon == order.MaHoaDon);
                _context.TempGioHangItems.RemoveRange(tempItems);
            }
            if (totalCancelled > 0)
                await _context.SaveChangesAsync();
            log.Add(new { step = "cancel_expired", totalCancelled });

            // 3. Nhả ghế cho tất cả hóa đơn đã hủy
            var cancelledOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Đã hủy")
                .ToListAsync();
            foreach (var order in cancelledOrders)
            {
                var tempItems = await _context.TempGioHangItems
                    .Where(x => x.MaHoaDon == order.MaHoaDon)
                    .ToListAsync();
                foreach (var item in tempItems)
                {
                    var ghe = await _context.GheNgois.FirstOrDefaultAsync(g => g.MaGhe == item.MaGhe);
                    if (ghe != null)
                    {
                        ghe.TrangThai = "Trống";
                        totalReleasedSeats++;
                        releasedSeatDetails.Add(new { seat = item.SoGhe, order = order.MaHoaDon });
                    }
                }
                _context.TempGioHangItems.RemoveRange(tempItems);
            }
            if (totalReleasedSeats > 0)
                await _context.SaveChangesAsync();
            log.Add(new { step = "release_seats", totalReleasedSeats, releasedSeatDetails });

            return Ok(new { log, totalPaid, totalCancelled, totalReleasedSeats });
        }

        [HttpGet("banking-check")]
        public async Task<IActionResult> BankingCheckOnly()
        {
            var result = await _bankingService.CheckBankingTransactionsAsync();
            return Ok(new { 
                message = "Banking check completed. Check logs for details.",
                result = result
            });
        }

        [HttpGet("banking-history")]
        public async Task<IActionResult> GetBankingHistory()
        {
            var result = await _bankingService.CheckBankingTransactionsAsync();
            return Ok(new { 
                message = "Banking history retrieved successfully. Check logs for details.",
                result = result
            });
        }

        [HttpGet("run-banking-cron")]
        public async Task<IActionResult> RunBankingCron()
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === CHẠY CRON CHECK BANKING ===");
            
            var result = await _bankingService.CheckBankingTransactionsAsync();
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Kết quả:");
            Console.WriteLine($"  - Tổng giao dịch: {result.TotalTransactions}");
            Console.WriteLine($"  - Giao dịch khớp: {result.MatchedOrders}");
            Console.WriteLine($"  - Đơn hàng cập nhật: {result.UpdatedOrders}");
            if (result.Errors.Any())
            {
                Console.WriteLine($"  - Lỗi: {string.Join(", ", result.Errors)}");
            }
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === HOÀN THÀNH CRON CHECK BANKING ===");
            
            return Ok(new { 
                message = "Banking cron completed successfully.",
                timestamp = result.Timestamp,
                totalTransactions = result.TotalTransactions,
                matchedOrders = result.MatchedOrders,
                updatedOrders = result.UpdatedOrders,
                errors = result.Errors
            });
        }

        public class BankingTransaction
        {
            [JsonPropertyName("transactionID")]
            [JsonConverter(typeof(FlexibleStringConverter))]
            public string TransactionID { get; set; } = string.Empty;

            [JsonPropertyName("amount")]
            public decimal Amount { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("transactionDate")]
            public string TransactionDate { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
        }
        public class FlexibleStringConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                    return reader.GetString() ?? "";
                if (reader.TokenType == JsonTokenType.Number)
                    return reader.GetInt64().ToString();
                return "";
            }
            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
} 