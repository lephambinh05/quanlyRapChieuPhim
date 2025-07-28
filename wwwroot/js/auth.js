// ===== AUTHENTICATION JAVASCRIPT =====

class AuthManager {
    constructor() {
        this.initializeEventListeners();
        this.setupFormValidation();
        this.setupQuickLogin();
        this.setupPasswordStrength();
        this.setupAnimations();
    }

    // ===== INITIALIZATION =====
    initializeEventListeners() {
        // Form submission
        const loginForm = document.getElementById('loginForm');
        const registerForm = document.getElementById('registerForm');
        
        if (loginForm) {
            loginForm.addEventListener('submit', (e) => this.handleLoginSubmit(e));
        }
        
        if (registerForm) {
            registerForm.addEventListener('submit', (e) => this.handleRegisterSubmit(e));
        }

        // Real-time validation
        this.setupRealTimeValidation();
    }

    // ===== FORM VALIDATION =====
    setupFormValidation() {
        // Email validation
        const emailInputs = document.querySelectorAll('input[type="email"]');
        emailInputs.forEach(input => {
            input.addEventListener('input', (e) => this.validateEmail(e.target));
            input.addEventListener('blur', (e) => this.validateEmail(e.target));
        });

        // Password validation
        const passwordInputs = document.querySelectorAll('input[type="password"]');
        passwordInputs.forEach(input => {
            input.addEventListener('input', (e) => this.validatePassword(e.target));
            input.addEventListener('blur', (e) => this.validatePassword(e.target));
        });

        // Phone number validation
        const phoneInputs = document.querySelectorAll('input[name="sdt"]');
        phoneInputs.forEach(input => {
            input.addEventListener('input', (e) => this.validatePhone(e.target));
            input.addEventListener('blur', (e) => this.validatePhone(e.target));
        });

        // Name validation
        const nameInputs = document.querySelectorAll('input[name="hoTen"]');
        nameInputs.forEach(input => {
            input.addEventListener('input', (e) => this.validateName(e.target));
            input.addEventListener('blur', (e) => this.validateName(e.target));
        });
    }

    // ===== REAL-TIME VALIDATION =====
    setupRealTimeValidation() {
        // Add input event listeners for real-time feedback
        const inputs = document.querySelectorAll('.form-control');
        inputs.forEach(input => {
            input.addEventListener('input', (e) => {
                this.removeValidationClasses(e.target);
                this.showValidationFeedback(e.target);
            });
        });
    }

    // ===== EMAIL VALIDATION =====
    validateEmail(input) {
        const email = input.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        const isValid = emailRegex.test(email) && email.length > 0;
        
        this.updateValidationState(input, isValid, 'email');
        return isValid;
    }

    // ===== PASSWORD VALIDATION =====
    validatePassword(input) {
        const password = input.value;
        const minLength = 6;
        const isValid = password.length >= minLength;
        
        this.updateValidationState(input, isValid, 'password');
        return isValid;
    }

    // ===== PHONE VALIDATION =====
    validatePhone(input) {
        const phone = input.value.trim();
        const phoneRegex = /^[0-9]{10,11}$/;
        const isValid = phoneRegex.test(phone);
        
        this.updateValidationState(input, isValid, 'phone');
        return isValid;
    }

    // ===== NAME VALIDATION =====
    validateName(input) {
        const name = input.value.trim();
        const nameRegex = /^[a-zA-ZÀ-ỹ\s]+$/;
        const isValid = nameRegex.test(name) && name.length > 0;
        
        this.updateValidationState(input, isValid, 'name');
        return isValid;
    }

    // ===== VALIDATION STATE MANAGEMENT =====
    updateValidationState(input, isValid, type) {
        const feedbackId = this.getFeedbackId(input, type);
        const validFeedback = document.getElementById(feedbackId + 'Valid');
        const invalidFeedback = document.getElementById(feedbackId + 'Feedback');
        
        if (input.value.length > 0) {
            if (isValid) {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
                if (invalidFeedback) invalidFeedback.style.display = 'none';
                if (validFeedback) validFeedback.style.display = 'block';
            } else {
                input.classList.remove('is-valid');
                input.classList.add('is-invalid');
                if (validFeedback) validFeedback.style.display = 'none';
                if (invalidFeedback) invalidFeedback.style.display = 'block';
            }
        } else {
            input.classList.remove('is-valid', 'is-invalid');
            if (validFeedback) validFeedback.style.display = 'none';
            if (invalidFeedback) invalidFeedback.style.display = 'none';
        }
    }

    getFeedbackId(input, type) {
        const inputId = input.id;
        switch (type) {
            case 'email': return 'email';
            case 'password': return 'password';
            case 'phone': return 'sdt';
            case 'name': return 'hoTen';
            default: return inputId;
        }
    }

    removeValidationClasses(input) {
        input.classList.remove('is-valid', 'is-invalid');
    }

    showValidationFeedback(input) {
        // This will be called by real-time validation
        const type = this.getInputType(input);
        switch (type) {
            case 'email':
                this.validateEmail(input);
                break;
            case 'password':
                this.validatePassword(input);
                break;
            case 'phone':
                this.validatePhone(input);
                break;
            case 'name':
                this.validateName(input);
                break;
        }
    }

    getInputType(input) {
        if (input.type === 'email') return 'email';
        if (input.name === 'sdt') return 'phone';
        if (input.name === 'hoTen') return 'name';
        if (input.type === 'password') return 'password';
        return 'text';
    }

    // ===== PASSWORD STRENGTH =====
    setupPasswordStrength() {
        const passwordInput = document.getElementById('password');
        const confirmPasswordInput = document.getElementById('confirmPassword');
        
        if (passwordInput) {
            passwordInput.addEventListener('input', (e) => {
                this.updatePasswordStrength(e.target.value);
            });
        }
        
        if (confirmPasswordInput) {
            confirmPasswordInput.addEventListener('input', (e) => {
                this.updatePasswordMatch(e.target.value);
            });
        }
    }

    updatePasswordStrength(password) {
        const strengthDiv = document.getElementById('passwordStrength');
        if (!strengthDiv) return;

        const strength = this.calculatePasswordStrength(password);
        
        if (password.length > 0) {
            strengthDiv.className = `password-strength ${strength.class}`;
            strengthDiv.innerHTML = `<i class="fas fa-shield-alt"></i> ${strength.text}`;
        } else {
            strengthDiv.innerHTML = '';
        }
    }

    calculatePasswordStrength(password) {
        let strength = 0;
        const checks = [
            password.length >= 6,
            /[a-z]/.test(password),
            /[A-Z]/.test(password),
            /[0-9]/.test(password),
            /[^A-Za-z0-9]/.test(password)
        ];
        
        strength = checks.filter(Boolean).length;
        
        if (strength < 2) {
            return { class: 'strength-weak', text: 'Mật khẩu yếu' };
        } else if (strength < 4) {
            return { class: 'strength-medium', text: 'Mật khẩu trung bình' };
        } else {
            return { class: 'strength-strong', text: 'Mật khẩu mạnh' };
        }
    }

    updatePasswordMatch(confirmPassword) {
        const password = document.getElementById('password')?.value || '';
        const matchDiv = document.getElementById('passwordMatch');
        
        if (!matchDiv) return;
        
        if (confirmPassword.length > 0) {
            if (password === confirmPassword) {
                matchDiv.className = 'password-strength strength-strong';
                matchDiv.innerHTML = '<i class="fas fa-check"></i> Mật khẩu khớp';
            } else {
                matchDiv.className = 'password-strength strength-weak';
                matchDiv.innerHTML = '<i class="fas fa-times"></i> Mật khẩu không khớp';
            }
        } else {
            matchDiv.innerHTML = '';
        }
    }

    // ===== QUICK LOGIN =====
    setupQuickLogin() {
        const quickLoginButtons = document.querySelectorAll('.quick-login-btn');
        
        quickLoginButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                this.handleQuickLogin(e);
            });
        });
    }

    handleQuickLogin(event) {
        const button = event.currentTarget;
        const email = button.dataset.email;
        const password = button.dataset.password;
        
        if (!email || !password) {
            this.showAlert('Thông tin đăng nhập không hợp lệ', 'danger');
            return;
        }
        
        // Fill form
        const emailInput = document.getElementById('email');
        const passwordInput = document.getElementById('password');
        
        if (emailInput && passwordInput) {
            emailInput.value = email;
            passwordInput.value = password;
            
            // Trigger validation
            this.validateEmail(emailInput);
            this.validatePassword(passwordInput);
            
            // Show loading state
            this.setButtonLoadingState(button, true);
            
            // Submit form after delay
            setTimeout(() => {
                const form = document.getElementById('loginForm');
                if (form) {
                    form.submit();
                }
            }, 800);
        }
    }

    // ===== FORM SUBMISSION =====
    handleLoginSubmit(event) {
        const form = event.target;
        const email = form.querySelector('#email')?.value;
        const password = form.querySelector('#password')?.value;
        
        // Reset validation states
        this.resetValidationStates();
        
        let hasError = false;
        
        // Validate email
        if (!email || !this.validateEmail(form.querySelector('#email'))) {
            hasError = true;
        }
        
        // Validate password
        if (!password || !this.validatePassword(form.querySelector('#password'))) {
            hasError = true;
        }
        
        if (hasError) {
            event.preventDefault();
            this.showAlert('Vui lòng kiểm tra lại thông tin đăng nhập', 'danger');
            return false;
        }
        
        // Show loading state
        const submitBtn = form.querySelector('#loginBtn');
        if (submitBtn) {
            this.setButtonLoadingState(submitBtn, true);
        }
    }

    handleRegisterSubmit(event) {
        const form = event.target;
        const email = form.querySelector('#email')?.value;
        const password = form.querySelector('#password')?.value;
        const confirmPassword = form.querySelector('#confirmPassword')?.value;
        const hoTen = form.querySelector('#hoTen')?.value;
        const sdt = form.querySelector('#sdt')?.value;
        
        // Reset validation states
        this.resetValidationStates();
        
        let hasError = false;
        
        // Validate all fields
        if (!this.validateEmail(form.querySelector('#email'))) hasError = true;
        if (!this.validatePassword(form.querySelector('#password'))) hasError = true;
        if (!this.validateName(form.querySelector('#hoTen'))) hasError = true;
        if (!this.validatePhone(form.querySelector('#sdt'))) hasError = true;
        
        // Check password match
        if (password !== confirmPassword) {
            hasError = true;
            this.showAlert('Mật khẩu xác nhận không khớp', 'danger');
        }
        
        // Check terms agreement
        const agreeTerms = form.querySelector('#agreeTerms');
        if (!agreeTerms?.checked) {
            hasError = true;
            this.showAlert('Vui lòng đồng ý với điều khoản sử dụng', 'danger');
        }
        
        if (hasError) {
            event.preventDefault();
            return false;
        }
        
        // Show loading state
        const submitBtn = form.querySelector('#submitBtn');
        if (submitBtn) {
            this.setButtonLoadingState(submitBtn, true);
        }
    }

    // ===== UTILITY FUNCTIONS =====
    resetValidationStates() {
        const inputs = document.querySelectorAll('.form-control');
        inputs.forEach(input => {
            input.classList.remove('is-valid', 'is-invalid');
        });
        
        const feedbacks = document.querySelectorAll('.invalid-feedback, .valid-feedback');
        feedbacks.forEach(feedback => {
            feedback.style.display = 'none';
        });
    }

    setButtonLoadingState(button, isLoading) {
        if (isLoading) {
            button.disabled = true;
            const originalText = button.innerHTML;
            button.dataset.originalText = originalText;
            button.innerHTML = '<span class="spinner"></span> Đang xử lý...';
        } else {
            button.disabled = false;
            const originalText = button.dataset.originalText;
            if (originalText) {
                button.innerHTML = originalText;
            }
        }
    }

    showAlert(message, type = 'info') {
        // Create alert element
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.innerHTML = `
            <i class="fas fa-${this.getAlertIcon(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        // Insert at the top of the form
        const form = document.querySelector('form');
        if (form) {
            form.parentNode.insertBefore(alertDiv, form);
        }
        
        // Auto dismiss after 5 seconds
        setTimeout(() => {
            if (alertDiv.parentNode) {
                alertDiv.remove();
            }
        }, 5000);
    }

    getAlertIcon(type) {
        switch (type) {
            case 'success': return 'check-circle';
            case 'danger': return 'exclamation-triangle';
            case 'warning': return 'exclamation-triangle';
            case 'info': return 'info-circle';
            default: return 'info-circle';
        }
    }

    // ===== ANIMATIONS =====
    setupAnimations() {
        // Add hover effects
        const buttons = document.querySelectorAll('.btn, .quick-login-btn');
        buttons.forEach(button => {
            button.addEventListener('mouseenter', (e) => {
                e.target.style.transform = 'translateY(-2px)';
            });
            
            button.addEventListener('mouseleave', (e) => {
                e.target.style.transform = 'translateY(0)';
            });
        });
        
        // Add focus effects
        const inputs = document.querySelectorAll('.form-control');
        inputs.forEach(input => {
            input.addEventListener('focus', (e) => {
                e.target.parentNode.classList.add('focused');
            });
            
            input.addEventListener('blur', (e) => {
                e.target.parentNode.classList.remove('focused');
            });
        });
    }

    // ===== EMAIL VALIDATION HELPER =====
    validateEmailFormat(email) {
        const input = document.createElement('input');
        input.type = 'email';
        input.value = email;
        return input.validity.valid && email.length > 0;
    }
}

// ===== INITIALIZATION =====
document.addEventListener('DOMContentLoaded', function() {
    new AuthManager();
});

// ===== EXPORT FOR GLOBAL USE =====
window.AuthManager = AuthManager; 