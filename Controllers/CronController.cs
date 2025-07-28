using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace CinemaManagement.Controllers
{
    [ApiController]
    [Route("api/cron")]
    [AllowAnonymous]
    public class CronController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        public CronController(CinemaDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> RunAllCrons()
        {
            var log = new List<object>();
            int totalPaid = 0, totalCancelled = 0, totalReleasedSeats = 0;
            var releasedSeatDetails = new List<object>();
            var skippedDetails = new List<object>();

            // 1. Kiểm tra thanh toán ngân hàng
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();
            var apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            using (var httpClient = new HttpClient())
            {
                // Thêm timeout và retry logic
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    var response = await httpClient.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        List<BankingTransaction> transactions = new List<BankingTransaction>();
                        if (string.IsNullOrWhiteSpace(json) || (!json.TrimStart().StartsWith("{") && !json.TrimStart().StartsWith("[")))
                        {
                            log.Add(new { step = "banking", error = "Empty or invalid JSON response from banking API" });
                        }
                        else
                        {
                            using (var doc = JsonDocument.Parse(json))
                            {
                                if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                                {
                                    transactions = JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                                }
                            }
                            // Lọc transaction chỉ lấy ngày hiện tại
                            var today = DateTime.Now.Date;
                            var todayStr = today.ToString("dd/MM/yyyy");
                            var validOrderCodes = pendingOrders.Select(o => o.MaHoaDon).ToList();
                            var todayTransactions = transactions
                                .Where(t => t.TransactionDate == todayStr &&
                                            validOrderCodes.Any(code => t.Description != null && t.Description.Contains(code, StringComparison.OrdinalIgnoreCase)))
                                .ToList();
                            log.Add(new { step = "banking", todayTransactions });
                            foreach (var order in pendingOrders)
                            {
                                var matched = todayTransactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
                                if (matched != null)
                                {
                                    order.TrangThai = "Đã thanh toán";
                                    totalPaid++;
                                }
                            }
                            if (totalPaid > 0)
                                await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        log.Add(new { step = "banking", error = $"Banking API error: {response.StatusCode}" });
                    }
                }
                catch (HttpRequestException ex)
                {
                    log.Add(new { step = "banking", error = $"Connection error: {ex.Message}" });
                }
                catch (TaskCanceledException ex)
                {
                    log.Add(new { step = "banking", error = $"Timeout error: {ex.Message}" });
                }
                catch (Exception ex)
                {
                    log.Add(new { step = "banking", error = $"Unexpected error: {ex.Message}" });
                }
            }
            log.Add(new { step = "banking", totalPaid });

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
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === BẮT ĐẦU KIỂM TRA BANKING ===");
            
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Tìm thấy {pendingOrders.Count} đơn hàng chờ thanh toán:");
            foreach (var order in pendingOrders)
            {
                Console.WriteLine($"  - {order.MaHoaDon}: {order.TrangThai} (tạo lúc {order.ThoiGianTao:HH:mm:ss})");
            }

            var apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Gọi API: {apiUrl}");
            
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    var response = await httpClient.GetAsync(apiUrl);
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Response Status: {response.StatusCode}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] JSON Response: {json}");
                        
                        List<BankingTransaction> transactions = new List<BankingTransaction>();
                        if (string.IsNullOrWhiteSpace(json) || (!json.TrimStart().StartsWith("{") && !json.TrimStart().StartsWith("[")))
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Empty or invalid JSON response");
                        }
                        else
                        {
                            using (var doc = JsonDocument.Parse(json))
                            {
                                if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                                {
                                    transactions = JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Tổng số giao dịch: {transactions.Count}");
                                }
                            }
                            
                            // Lấy tất cả giao dịch có chứa mã đơn hợp lệ
                            var validOrderCodes = pendingOrders.Select(o => o.MaHoaDon).ToList();
                            var matchingTransactions = transactions
                                .Where(t => t.Description != null &&
                                            validOrderCodes.Any(code => t.Description.Contains(code, StringComparison.OrdinalIgnoreCase)))
                                .ToList();
                            
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Mã đơn hợp lệ: {string.Join(", ", validOrderCodes)}");
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Giao dịch có chứa mã đơn: {matchingTransactions.Count}");
                            foreach (var tx in matchingTransactions)
                            {
                                Console.WriteLine($"  - {tx.TransactionID}: {tx.Amount} VND - {tx.Description} (ngày: {tx.TransactionDate})");
                            }
                            
                            int totalPaid = 0;
                            foreach (var order in pendingOrders)
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Kiểm tra đơn {order.MaHoaDon}...");
                                var matched = matchingTransactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
                                if (matched != null)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✓ Tìm thấy thanh toán cho đơn {order.MaHoaDon}: {matched.Amount} VND");
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cập nhật trạng thái từ '{order.TrangThai}' thành 'Đã thanh toán'");
                                    order.TrangThai = "Đã thanh toán";
                                    totalPaid++;
                                }
                                else
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✗ Chưa có thanh toán cho đơn {order.MaHoaDon}");
                                }
                            }
                            
                            if (totalPaid > 0)
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lưu thay đổi vào database...");
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✓ Đã cập nhật {totalPaid} đơn hàng thành công");
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không có đơn hàng nào được thanh toán");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Banking API error - {response.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Connection error - {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Timeout error - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Unexpected error - {ex.Message}");
                }
            }
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === KẾT THÚC KIỂM TRA BANKING ===");
            return Ok(new { message = "Banking check completed. Check console for details." });
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