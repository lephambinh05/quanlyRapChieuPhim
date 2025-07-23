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

        [HttpGet("check-banking")]
        public async Task<IActionResult> GetBankingCronStatus()
        {
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();

            var apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                return StatusCode(502, "Banking API error");
            var json = await response.Content.ReadAsStringAsync();
            List<BankingTransaction> transactions = new List<BankingTransaction>();
            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                {
                    transactions = JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                }
            }
            var matchedOrders = new List<object>();
            int updated = 0;
            foreach (var order in pendingOrders)
            {
                var matched = transactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    order.TrangThai = "Đã thanh toán";
                    updated++;
                    matchedOrders.Add(new {
                        order.MaHoaDon,
                        order.TongTien,
                        order.ThoiGianTao,
                        order.MaKhachHang,
                        MatchedTransaction = matched
                    });
                }
            }
            if (updated > 0)
            {
                await _context.SaveChangesAsync();
            }
            return Ok(new {
                count = matchedOrders.Count,
                updated,
                matchedOrders
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