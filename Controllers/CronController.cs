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
                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
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
                        foreach (var order in pendingOrders)
                        {
                            var matched = transactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
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
                    log.Add(new { step = "banking", error = "Banking API error" });
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
                var cthds = _context.CTHDs.Where(c => c.MaHoaDon == order.MaHoaDon).ToList();
                foreach (var cthd in cthds)
                {
                    var ve = _context.Ves.FirstOrDefault(v => v.MaVe == cthd.MaVe);
                    if (ve == null)
                    {
                        skippedDetails.Add(new { cthd.MaVe, Reason = "Không tìm thấy vé" });
                        continue;
                    }
                    // Luôn nhả ghế, không kiểm tra trạng thái 'Đã bán' nữa
                    ve.TrangThai = "Chưa đặt";
                    _context.Ves.Update(ve);
                    if (!string.IsNullOrEmpty(ve.MaGhe))
                    {
                        var ghe = _context.GheNgois.FirstOrDefault(g => g.MaGhe == ve.MaGhe);
                        if (ghe != null)
                        {
                            ghe.TrangThai = "Trống";
                            _context.GheNgois.Update(ghe);
                            totalReleasedSeats++;
                            releasedSeatDetails.Add(new { ve.MaVe, ve.MaGhe, GheTrangThai = ghe.TrangThai });
                        }
                        else
                        {
                            skippedDetails.Add(new { ve.MaVe, ve.MaGhe, Reason = "Không tìm thấy ghế" });
                        }
                    }
                    else
                    {
                        skippedDetails.Add(new { ve.MaVe, Reason = "Vé không có MaGhe" });
                    }
                }
            }
            await _context.SaveChangesAsync();
            log.Add(new { step = "release_cancelled", totalReleasedSeats, releasedSeatDetails, skippedDetails });

            return Ok(new { log });
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