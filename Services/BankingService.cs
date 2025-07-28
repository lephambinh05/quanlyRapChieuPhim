using CinemaManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CinemaManagement.Services
{
    public class BankingService
    {
        private readonly CinemaDbContext _context;
        private readonly string _logFilePath;
        private readonly string _apiUrl;

        public BankingService(CinemaDbContext context)
        {
            _context = context;
            _logFilePath = Path.Combine("logs", "banking_log.txt");
            _apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            
            // Đảm bảo thư mục logs tồn tại
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir!);
            }
        }

        public async Task<BankingCheckResult> CheckBankingTransactionsAsync()
        {
            var result = new BankingCheckResult
            {
                Timestamp = DateTime.Now,
                TotalTransactions = 0,
                MatchedOrders = 0,
                UpdatedOrders = 0,
                Errors = new List<string>()
            };

            try
            {
                await LogToFile($"=== BẮT ĐẦU KIỂM TRA BANKING {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

                // Lấy danh sách đơn hàng chờ thanh toán
                var pendingOrders = await _context.HoaDons
                    .Where(h => h.TrangThai == "Chờ chuyển khoản")
                    .ToListAsync();

                await LogToFile($"Tìm thấy {pendingOrders.Count} đơn hàng chờ thanh toán:");
                foreach (var order in pendingOrders)
                {
                    await LogToFile($"  - {order.MaHoaDon}: {order.TrangThai} (tạo lúc {order.ThoiGianTao:HH:mm:ss})");
                }

                // Gọi API ngân hàng
                await LogToFile($"Gọi API: {_apiUrl}");
                var transactions = await GetBankingTransactionsAsync();
                result.TotalTransactions = transactions.Count;

                if (transactions.Any())
                {
                    await LogToFile($"Tổng số giao dịch: {transactions.Count}");
                    
                    // In ra tất cả lịch sử giao dịch
                    await LogToFile("=== LỊCH SỬ GIAO DỊCH NGÂN HÀNG ===");
                    foreach (var tx in transactions)
                    {
                        await LogToFile($"  - ID: {tx.TransactionID}");
                        await LogToFile($"    Số tiền: {tx.Amount:N0} VND");
                        await LogToFile($"    Nội dung: {tx.Description}");
                        await LogToFile($"    Ngày: {tx.TransactionDate}");
                        await LogToFile($"    Loại: {tx.Type}");
                        await LogToFile("    ---");
                    }

                    // Tìm các giao dịch khớp với đơn hàng (tìm kiếm tương đối)
                    var validOrderCodes = pendingOrders.Select(o => o.MaHoaDon.Trim()).ToList();
                    await LogToFile($"Mã đơn hợp lệ cần tìm: [{string.Join("], [", validOrderCodes)}]");
                    
                    var matchingTransactions = new List<BankingTransaction>();
                    foreach (var tx in transactions)
                    {
                        if (tx.Description != null)
                        {
                                                    foreach (var orderCode in validOrderCodes)
                        {
                            if (tx.Description.Contains(orderCode, StringComparison.OrdinalIgnoreCase))
                            {
                                await LogToFile($"✓ Tìm thấy mã '[{orderCode}]' trong nội dung: '{tx.Description}'");
                                matchingTransactions.Add(tx);
                                break; // Chỉ thêm một lần nếu giao dịch chứa nhiều mã
                            }
                        }
                        }
                    }

                    result.MatchedOrders = matchingTransactions.Count;
                    await LogToFile($"Tổng giao dịch khớp: {matchingTransactions.Count}");

                    foreach (var tx in matchingTransactions)
                    {
                        await LogToFile($"  - {tx.TransactionID}: {tx.Amount:N0} VND - {tx.Description} (ngày: {tx.TransactionDate})");
                    }

                    // Cập nhật trạng thái đơn hàng
                    foreach (var order in pendingOrders)
                    {
                        var orderCode = order.MaHoaDon.Trim();
                        await LogToFile($"Kiểm tra đơn [{orderCode}]...");
                        var matched = matchingTransactions.FirstOrDefault(t => 
                            t.Description != null && 
                            t.Description.Contains(orderCode, StringComparison.OrdinalIgnoreCase));

                        if (matched != null)
                        {
                            await LogToFile($"✓ Tìm thấy thanh toán cho đơn [{orderCode}]: {matched.Amount:N0} VND");
                            await LogToFile($"Nội dung giao dịch: {matched.Description}");
                            await LogToFile($"Cập nhật trạng thái từ '{order.TrangThai}' thành 'Đã thanh toán'");
                            
                            order.TrangThai = "Đã thanh toán";
                            result.UpdatedOrders++;
                        }
                        else
                        {
                            await LogToFile($"✗ Chưa có thanh toán cho đơn [{orderCode}]");
                        }
                    }

                    // Lưu thay đổi vào database
                    if (result.UpdatedOrders > 0)
                    {
                        await LogToFile("Lưu thay đổi vào database...");
                        await _context.SaveChangesAsync();
                        await LogToFile($"✓ Đã cập nhật {result.UpdatedOrders} đơn hàng thành công");
                    }
                    else
                    {
                        await LogToFile("Không có đơn hàng nào được thanh toán");
                    }
                }
                else
                {
                    await LogToFile("Không có giao dịch nào từ API ngân hàng");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"ERROR: {ex.Message}";
                await LogToFile(errorMsg);
                result.Errors.Add(errorMsg);
            }

            await LogToFile($"=== KẾT THÚC KIỂM TRA BANKING {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            await LogToFile("");

            return result;
        }

        private async Task<List<BankingTransaction>> GetBankingTransactionsAsync()
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                var response = await httpClient.GetAsync(_apiUrl);
                await LogToFile($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    await LogToFile($"JSON Response: {json}");

                    if (string.IsNullOrWhiteSpace(json) || (!json.TrimStart().StartsWith("{") && !json.TrimStart().StartsWith("[")))
                    {
                        await LogToFile("ERROR: Empty or invalid JSON response");
                        return new List<BankingTransaction>();
                    }

                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                    {
                        return JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                    }
                }
                else
                {
                    await LogToFile($"ERROR: Banking API error - {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                await LogToFile($"ERROR: Connection error - {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                await LogToFile($"ERROR: Timeout error - {ex.Message}");
            }
            catch (Exception ex)
            {
                await LogToFile($"ERROR: Unexpected error - {ex.Message}");
            }

            return new List<BankingTransaction>();
        }

        private async Task LogToFile(string message)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                await File.AppendAllTextAsync(_logFilePath, logMessage + Environment.NewLine);
                Console.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR writing to log file: {ex.Message}");
            }
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

        public class BankingCheckResult
        {
            public DateTime Timestamp { get; set; }
            public int TotalTransactions { get; set; }
            public int MatchedOrders { get; set; }
            public int UpdatedOrders { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
} 