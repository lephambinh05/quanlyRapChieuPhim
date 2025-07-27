using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels
{
    public class TwoFactorSetupViewModel
    {
        public string? Secret { get; set; }
        public string? QrCodeImage { get; set; }
        public string? Email { get; set; }
        public string? ManualEntryKey { get; set; }
        // BackupCodes đã được xóa
    }

    public class TwoFactorVerifyViewModel
    {
        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác thực chỉ được chứa số")]
        public string? Code { get; set; }

        public bool UseBackupCode { get; set; } = false;
        public string? ReturnUrl { get; set; }
    }

    public class TwoFactorEnableViewModel
    {
        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác thực chỉ được chứa số")]
        public string? Code { get; set; }

        public string? Secret { get; set; }
        public string? QrCodeImage { get; set; }
        public string? Email { get; set; }
        public string? ManualEntryKey { get; set; }
        // BackupCodes đã được xóa
    }

    public class TwoFactorDisableViewModel
    {
        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác thực chỉ được chứa số")]
        public string? Code { get; set; }
    }

    public class TwoFactorStatusViewModel
    {
        public bool IsEnabled { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? SetupDate { get; set; }
        // Backup codes properties đã được xóa
    }
} 