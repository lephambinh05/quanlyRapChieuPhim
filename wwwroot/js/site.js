// Cinema Management System JavaScript

// Global functions
window.CinemaApp = {
    // Format currency
    formatCurrency: function(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    },

    // Format date
    formatDate: function(date) {
        return new Date(date).toLocaleDateString('vi-VN');
    },

    // Format time
    formatTime: function(date) {
        return new Date(date).toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Show loading
    showLoading: function(element) {
        var $el = $(element);
        $el.html('<div class="text-center py-3"><div class="spinner-border text-primary" role="status"></div></div>');
    },

    // Show error
    showError: function(element, message) {
        var $el = $(element);
        $el.html('<div class="alert alert-danger text-center"><i class="fas fa-exclamation-triangle me-2"></i>' + message + '</div>');
    },

    // Show success message
    showSuccess: function(message) {
        var alertHtml = '<div class="alert alert-success alert-dismissible fade show" role="alert">' +
            '<i class="fas fa-check-circle me-2"></i>' + message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
            '</div>';
        
        $('main').prepend(alertHtml);
        
        // Auto dismiss after 5 seconds
        setTimeout(function() {
            $('.alert-success').alert('close');
        }, 5000);
    },

    // Confirm dialog
    confirm: function(message, callback) {
        if (confirm(message)) {
            callback();
        }
    },

    // Validate form
    validateForm: function(formSelector) {
        var isValid = true;
        $(formSelector + ' [required]').each(function() {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        return isValid;
    },

    // Initialize rating stars
    initRatingStars: function() {
        $('.rating-stars').each(function() {
            var $container = $(this);
            var $hiddenInput = $container.siblings('#rating-value');
            var $stars = $container.find('.rating-star');
            
            // Handle star click
            $stars.on('click', function() {
                var $clickedStar = $(this);
                var ratingValue = $clickedStar.data('rating');
                
                // Update hidden input
                $hiddenInput.val(ratingValue);
                
                // Update star display
                $stars.removeClass('text-warning');
                $stars.each(function(index) {
                    if (index < ratingValue) {
                        $(this).addClass('text-warning');
                    }
                });
                
                console.log('Rating selected:', ratingValue);
            });
            
            // Handle hover effects
            $stars.on('mouseenter', function() {
                var $hoveredStar = $(this);
                var hoverRating = $hoveredStar.data('rating');
                
                $stars.removeClass('text-warning');
                $stars.each(function(index) {
                    if (index < hoverRating) {
                        $(this).addClass('text-warning');
                    }
                });
            });
            
            $container.on('mouseleave', function() {
                var currentRating = parseInt($hiddenInput.val()) || 0;
                $stars.removeClass('text-warning');
                $stars.each(function(index) {
                    if (index < currentRating) {
                        $(this).addClass('text-warning');
                    }
                });
            });
        });
    }
};

// Document ready
$(document).ready(function() {
    // Add fade-in animation to cards
    $('.card').each(function(index) {
        $(this).delay(index * 100).queue(function() {
            $(this).addClass('fade-in').dequeue();
        });
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert:not(.alert-permanent)').alert('close');
    }, 5000);

    // Tooltip initialization
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Form validation
    $('form').on('submit', function(e) {
        var $form = $(this);
        var isValid = true;

        $form.find('[required]').each(function() {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            e.preventDefault();
        }
    });

    // Remove validation errors on input
    $('input, select, textarea').on('input change', function() {
        $(this).removeClass('is-invalid');
    });

    // Initialize rating stars
    CinemaApp.initRatingStars();

    // Loading button state
    $('.btn[data-loading-text]').on('click', function() {
        var $btn = $(this);
        var loadingText = $btn.data('loading-text');
        var originalText = $btn.html();
        
        $btn.html(loadingText).prop('disabled', true);
        
        // Reset after 10 seconds (fallback)
        setTimeout(function() {
            $btn.html(originalText).prop('disabled', false);
        }, 10000);
    });
});

// Global error handler for AJAX
$(document).ajaxError(function(event, xhr, settings, error) {
    console.error('AJAX Error:', error);
    
    if (xhr.status === 401) {
        alert('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
        window.location.href = '/Auth/Login';
    } else if (xhr.status === 500) {
        alert('Có lỗi hệ thống xảy ra. Vui lòng thử lại sau.');
    } else if (xhr.status === 0) {
        alert('Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.');
    }
});

// Prevent double submission
$('form').on('submit', function() {
    var $form = $(this);
    var $submitBtn = $form.find('button[type="submit"], input[type="submit"]');
    
    setTimeout(function() {
        $submitBtn.prop('disabled', true);
    }, 100);
    
    setTimeout(function() {
        $submitBtn.prop('disabled', false);
    }, 3000);
});