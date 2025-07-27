using System.Net.Mail;
using System.Net;
using System.Text;

namespace CinemaManagement.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
        Task<bool> SendPasswordResetSuccessEmailAsync(string toEmail, string userName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"] ?? "";
            _smtpPassword = _configuration["Email:Password"] ?? "";
            _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
            _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
            _fromName = _configuration["Email:FromName"] ?? "Cinema Management System";
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            var subject = "Đặt lại mật khẩu - Hệ thống Quản lý Rạp Chiếu Phim";
            var body = GeneratePasswordResetEmailBody(resetLink, userName);
            
            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendPasswordResetSuccessEmailAsync(string toEmail, string userName)
        {
            var subject = "Mật khẩu đã được đặt lại thành công - Cinema Management";
            var body = GeneratePasswordResetSuccessEmailBody(userName);
            
            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = _enableSsl;

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8
                };

                message.To.Add(new MailAddress(toEmail));

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [EMAIL_ERROR] {ex.Message}\n";
                System.IO.File.AppendAllText(logPath, logLine);
                return false;
            }
        }

        private string GeneratePasswordResetEmailBody(string resetLink, string userName)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Đặt lại mật khẩu - Cinema Management</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px 0;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        
        .header-content {{
            position: relative;
            z-index: 1;
        }}
        
        .logo {{
            font-size: 3rem;
            margin-bottom: 15px;
            display: block;
        }}
        
        .header h1 {{
            font-size: 2rem;
            font-weight: 700;
            margin-bottom: 10px;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
        }}
        
        .header p {{
            font-size: 1.1rem;
            opacity: 0.9;
            font-weight: 300;
        }}
        
        .content {{
            padding: 40px 30px;
            background: white;
        }}
        
        .greeting {{
            margin-bottom: 30px;
        }}
        
        .greeting h2 {{
            color: #2c3e50;
            font-size: 1.8rem;
            margin-bottom: 10px;
            font-weight: 600;
        }}
        
        .greeting p {{
            color: #7f8c8d;
            font-size: 1.1rem;
            margin-bottom: 25px;
        }}
        
        .reset-button-container {{
            text-align: center;
            margin: 35px 0;
            padding: 20px;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 15px;
            border: 2px solid #e9ecef;
        }}
        
        .reset-button {{
            display: inline-block;
            padding: 18px 36px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 50px;
            font-size: 1.1rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }}
        
        .warning-box {{
            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
            border: 2px solid #ffc107;
            border-radius: 15px;
            padding: 25px;
            margin: 30px 0;
            position: relative;
        }}
        
        .warning-box::before {{
            content: '⚠️';
            position: absolute;
            top: -15px;
            left: 20px;
            background: white;
            padding: 5px 10px;
            border-radius: 20px;
            font-size: 1.2rem;
        }}
        
        .warning-box h3 {{
            color: #856404;
            font-size: 1.3rem;
            margin-bottom: 15px;
            font-weight: 600;
        }}
        
        .warning-box ul {{
            list-style: none;
            padding-left: 0;
        }}
        
        .warning-box li {{
            color: #856404;
            margin-bottom: 8px;
            padding-left: 25px;
            position: relative;
        }}
        
        .warning-box li::before {{
            content: '🔒';
            position: absolute;
            left: 0;
            top: 0;
        }}
        
        .link-fallback {{
            background: #f8f9fa;
            border: 2px dashed #dee2e6;
            border-radius: 10px;
            padding: 20px;
            margin: 25px 0;
            text-align: center;
        }}
        
        .link-fallback p {{
            color: #6c757d;
            margin-bottom: 10px;
            font-weight: 500;
        }}
        
        .link-text {{
            background: #e9ecef;
            padding: 15px;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
            font-size: 0.9rem;
            word-break: break-all;
            color: #495057;
            border: 1px solid #ced4da;
        }}
        
        .footer {{
            background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        
        .footer-content {{
            max-width: 400px;
            margin: 0 auto;
        }}
        
        .footer p {{
            margin-bottom: 10px;
            font-size: 0.9rem;
            opacity: 0.8;
        }}
        
        .footer .copyright {{
            font-size: 0.8rem;
            opacity: 0.6;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
        }}
        
        .security-badges {{
            display: flex;
            justify-content: center;
            gap: 15px;
            margin: 20px 0;
        }}
        
        .security-badge {{
            background: rgba(255, 255, 255, 0.1);
            padding: 8px 15px;
            border-radius: 20px;
            font-size: 0.8rem;
            display: flex;
            align-items: center;
            gap: 5px;
        }}
        
        @media (max-width: 600px) {{
            .email-container {{
                margin: 10px;
                border-radius: 15px;
            }}
            
            .header {{
                padding: 30px 20px;
            }}
            
            .content {{
                padding: 30px 20px;
            }}
            
            .header h1 {{
                font-size: 1.5rem;
            }}
            
            .greeting h2 {{
                font-size: 1.5rem;
            }}
            
            .reset-button {{
                padding: 15px 30px;
                font-size: 1rem;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='header-content'>
                <span class='logo'>🎬</span>
                <h1>Cinema Management</h1>
                <p>Hệ thống Quản lý Rạp Chiếu Phim</p>
            </div>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                <h2>Xin chào {userName}! 👋</h2>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            </div>
            
            <div class='reset-button-container'>
                <p style='margin-bottom: 20px; color: #6c757d; font-weight: 500;'>
                    <i class='fas fa-shield-alt'></i> Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu:
                </p>
                <a href='{resetLink}' class='reset-button'>
                    🔐 Đặt lại mật khẩu
                </a>
            </div>
            
            <div class='warning-box'>
                <h3>Lưu ý quan trọng về bảo mật</h3>
                <ul>
                    <li>Link này chỉ có hiệu lực trong <strong>30 phút</strong></li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                    <li>Để bảo mật, vui lòng không chia sẻ link này với bất kỳ ai</li>
                    <li>Link này chỉ có thể sử dụng <strong>một lần duy nhất</strong></li>
                </ul>
            </div>
            
            <div class='link-fallback'>
                <p><strong>Nếu nút không hoạt động, bạn có thể copy link sau vào trình duyệt:</strong></p>
                <div class='link-text'>{resetLink}</div>
            </div>
        </div>
        
        <div class='footer'>
            <div class='footer-content'>
                <div class='security-badges'>
                    <div class='security-badge'>
                        <span>🔒</span>
                        <span>Bảo mật SSL</span>
                    </div>
                    <div class='security-badge'>
                        <span>⏰</span>
                        <span>30 phút</span>
                    </div>
                    <div class='security-badge'>
                        <span>🔄</span>
                        <span>1 lần sử dụng</span>
                    </div>
                </div>
                
                <p><i class='fas fa-info-circle'></i> Email này được gửi tự động từ hệ thống</p>
                <p><i class='fas fa-exclamation-triangle'></i> Vui lòng không trả lời email này</p>
                <p><i class='fas fa-envelope'></i> Nếu có thắc mắc, vui lòng liên hệ hỗ trợ</p>
                
                <div class='copyright'>
                    © 2024 Cinema Management System. All rights reserved.
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetSuccessEmailBody(string userName)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mật khẩu đã được đặt lại thành công</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px 0;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        
        .header-content {{
            position: relative;
            z-index: 1;
        }}
        
        .success-icon {{
            font-size: 4rem;
            margin-bottom: 15px;
            display: block;
            animation: bounce 2s infinite;
        }}
        
        @keyframes bounce {{
            0%, 20%, 50%, 80%, 100% {{
                transform: translateY(0);
            }}
            40% {{
                transform: translateY(-10px);
            }}
            60% {{
                transform: translateY(-5px);
            }}
        }}
        
        .header h1 {{
            font-size: 2rem;
            font-weight: 700;
            margin-bottom: 10px;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
        }}
        
        .header p {{
            font-size: 1.1rem;
            opacity: 0.9;
            font-weight: 300;
        }}
        
        .content {{
            padding: 40px 30px;
            background: white;
        }}
        
        .success-message {{
            text-align: center;
            margin-bottom: 30px;
        }}
        
        .success-message h2 {{
            color: #28a745;
            font-size: 1.8rem;
            margin-bottom: 15px;
            font-weight: 600;
        }}
        
        .success-message p {{
            color: #7f8c8d;
            font-size: 1.1rem;
            margin-bottom: 25px;
        }}
        
        .info-box {{
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            border: 2px solid #28a745;
            border-radius: 15px;
            padding: 25px;
            margin: 30px 0;
            position: relative;
        }}
        
        .info-box::before {{
            content: '✅';
            position: absolute;
            top: -15px;
            left: 20px;
            background: white;
            padding: 5px 10px;
            border-radius: 20px;
            font-size: 1.2rem;
        }}
        
        .info-box h3 {{
            color: #155724;
            font-size: 1.3rem;
            margin-bottom: 15px;
            font-weight: 600;
        }}
        
        .info-box ul {{
            list-style: none;
            padding-left: 0;
        }}
        
        .info-box li {{
            color: #155724;
            margin-bottom: 8px;
            padding-left: 25px;
            position: relative;
        }}
        
        .info-box li::before {{
            content: '✓';
            position: absolute;
            left: 0;
            top: 0;
            color: #28a745;
            font-weight: bold;
        }}
        
        .login-section {{
            background: #f8f9fa;
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 25px;
            margin: 30px 0;
            text-align: center;
        }}
        
        .login-button {{
            display: inline-block;
            padding: 15px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 50px;
            font-size: 1.1rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
            transition: all 0.3s ease;
            margin-top: 15px;
        }}
        
        .login-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 12px 30px rgba(102, 126, 234, 0.6);
        }}
        
        .security-tips {{
            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
            border: 2px solid #ffc107;
            border-radius: 15px;
            padding: 25px;
            margin: 30px 0;
        }}
        
        .security-tips h3 {{
            color: #856404;
            font-size: 1.3rem;
            margin-bottom: 15px;
            font-weight: 600;
        }}
        
        .security-tips ul {{
            list-style: none;
            padding-left: 0;
        }}
        
        .security-tips li {{
            color: #856404;
            margin-bottom: 8px;
            padding-left: 25px;
            position: relative;
        }}
        
        .security-tips li::before {{
            content: '🔒';
            position: absolute;
            left: 0;
            top: 0;
        }}
        
        .footer {{
            background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        
        .footer-content {{
            max-width: 400px;
            margin: 0 auto;
        }}
        
        .footer p {{
            margin-bottom: 10px;
            font-size: 0.9rem;
            opacity: 0.8;
        }}
        
        .footer .copyright {{
            font-size: 0.8rem;
            opacity: 0.6;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
        }}
        
        .success-badges {{
            display: flex;
            justify-content: center;
            gap: 15px;
            margin: 20px 0;
        }}
        
        .success-badge {{
            background: rgba(255, 255, 255, 0.1);
            padding: 8px 15px;
            border-radius: 20px;
            font-size: 0.8rem;
            display: flex;
            align-items: center;
            gap: 5px;
        }}
        
        @media (max-width: 600px) {{
            .email-container {{
                margin: 10px;
                border-radius: 15px;
            }}
            
            .header {{
                padding: 30px 20px;
            }}
            
            .content {{
                padding: 30px 20px;
            }}
            
            .header h1 {{
                font-size: 1.5rem;
            }}
            
            .success-message h2 {{
                font-size: 1.5rem;
            }}
            
            .login-button {{
                padding: 12px 25px;
                font-size: 1rem;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='header-content'>
                <span class='success-icon'>🎉</span>
                <h1>Thành công!</h1>
                <p>Mật khẩu đã được đặt lại</p>
            </div>
        </div>
        
        <div class='content'>
            <div class='success-message'>
                <h2>Xin chào {userName}! 🎊</h2>
                <p>Mật khẩu của bạn đã được đặt lại thành công.</p>
            </div>
            
            <div class='info-box'>
                <h3>Thông tin cập nhật</h3>
                <ul>
                    <li>Mật khẩu mới đã được lưu vào hệ thống</li>
                    <li>Tài khoản của bạn đã sẵn sàng để sử dụng</li>
                    <li>Bạn có thể đăng nhập ngay bây giờ</li>
                    <li>Phiên đăng nhập trước đó đã được đăng xuất</li>
                </ul>
            </div>
            
            <div class='login-section'>
                <h3 style='color: #2c3e50; margin-bottom: 15px;'>Sẵn sàng đăng nhập?</h3>
                <p style='color: #7f8c8d; margin-bottom: 20px;'>
                    Bạn có thể đăng nhập ngay bây giờ với mật khẩu mới
                </p>
                <a href='#' class='login-button'>
                    🚀 Đăng nhập ngay
                </a>
            </div>
            
            <div class='security-tips'>
                <h3>💡 Mẹo bảo mật</h3>
                <ul>
                    <li>Không chia sẻ mật khẩu với bất kỳ ai</li>
                    <li>Sử dụng mật khẩu mạnh và khó đoán</li>
                    <li>Đăng xuất khi sử dụng máy tính công cộng</li>
                    <li>Bật xác thực 2 yếu tố nếu có thể</li>
                </ul>
            </div>
        </div>
        
        <div class='footer'>
            <div class='footer-content'>
                <div class='success-badges'>
                    <div class='success-badge'>
                        <span>✅</span>
                        <span>Hoàn thành</span>
                    </div>
                    <div class='success-badge'>
                        <span>🔐</span>
                        <span>Bảo mật</span>
                    </div>
                    <div class='success-badge'>
                        <span>⚡</span>
                        <span>Sẵn sàng</span>
                    </div>
                </div>
                
                <p><i class='fas fa-info-circle'></i> Email này được gửi tự động từ hệ thống</p>
                <p><i class='fas fa-shield-alt'></i> Tài khoản của bạn đã được bảo vệ</p>
                <p><i class='fas fa-envelope'></i> Nếu có thắc mắc, vui lòng liên hệ hỗ trợ</p>
                
                <div class='copyright'>
                    © 2024 Cinema Management System. All rights reserved.
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
} 