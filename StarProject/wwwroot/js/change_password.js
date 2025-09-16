/**
 * 修改密碼頁面 JavaScript
 * 檔案位置: wwwroot/js/change-password.js
 */

// 切換密碼顯示/隱藏功能
function togglePassword(fieldId) {
    const passwordField = document.getElementById(fieldId);
    const icon = document.getElementById(fieldId + '-icon');
    
    if (passwordField && icon) {
        if (passwordField.type === 'password') {
            passwordField.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            passwordField.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    }
}

// 密碼強度檢查功能
function initPasswordStrengthChecker() {
    const newPasswordField = document.getElementById('NewPassword');
    
    if (!newPasswordField) {
        console.warn('找不到新密碼輸入欄位');
        return;
    }

    newPasswordField.addEventListener('input', function() {
        const password = this.value;
        const strengthBar = document.getElementById('password-strength-bar');
        const strengthText = document.getElementById('password-strength-text');
        
        // 如果找不到強度指示器元素，就不執行
        if (!strengthBar || !strengthText) {
            return;
        }
        
        let strength = 0;
        let feedback = [];
        
        // 長度檢查
        if (password.length >= 6) {
            strength++;
        } else if (password.length > 0) {
            feedback.push('至少6個字元');
        }
        
        // 大寫字母檢查
        if (/[A-Z]/.test(password)) {
            strength++;
        } else if (password.length > 0) {
            feedback.push('包含大寫字母');
        }
        
        // 小寫字母檢查
        if (/[a-z]/.test(password)) {
            strength++;
        } else if (password.length > 0) {
            feedback.push('包含小寫字母');
        }
        
        // 數字檢查
        if (/\d/.test(password)) {
            strength++;
        } else if (password.length > 0) {
            feedback.push('包含數字');
        }
        
        // 特殊字元檢查（額外加分）
        if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
            strength++;
        }
        
        // 強度等級和顏色設定
        const strengthLevels = ['很弱', '弱', '普通', '強', '很強'];
        const strengthColors = ['bg-danger', 'bg-warning', 'bg-info', 'bg-success', 'bg-success'];
        
        // 更新強度指示器
        if (password.length === 0) {
            // 清空時重置
            strengthBar.style.width = '0%';
            strengthBar.className = 'progress-bar';
            strengthText.textContent = '';
        } else {
            const level = Math.min(strength, 4);
            const percentage = Math.max((strength / 5) * 100, 10); // 至少顯示 10%
            
            strengthBar.style.width = percentage + '%';
            strengthBar.className = `progress-bar ${strengthColors[level]}`;
            strengthText.textContent = `密碼強度: ${strengthLevels[level]}`;
            
            // 顯示改善建議
            if (feedback.length > 0) {
                strengthText.textContent += ` (需要: ${feedback.join(', ')})`;
            }
        }
    });
}

// 表單驗證增強
function initFormValidation() {
    const form = document.querySelector('form');
    const newPasswordField = document.getElementById('NewPassword');
    const confirmPasswordField = document.getElementById('ConfirmPassword');
    
    if (!form || !newPasswordField || !confirmPasswordField) {
        return;
    }
    
    // 即時密碼確認驗證
    confirmPasswordField.addEventListener('input', function() {
        const newPassword = newPasswordField.value;
        const confirmPassword = this.value;
        const confirmValidation = document.querySelector('[data-valmsg-for="ConfirmPassword"]');
        
        if (confirmPassword && newPassword !== confirmPassword) {
            this.classList.add('is-invalid');
            if (confirmValidation) {
                confirmValidation.textContent = '密碼確認不相符';
                confirmValidation.classList.add('text-danger');
            }
        } else {
            this.classList.remove('is-invalid');
            if (confirmValidation) {
                confirmValidation.textContent = '';
                confirmValidation.classList.remove('text-danger');
            }
        }
    });
    
    // 表單提交前最終驗證
    form.addEventListener('submit', function(e) {
        const newPassword = newPasswordField.value;
        const confirmPassword = confirmPasswordField.value;
        
        // 基本驗證
        if (!newPassword || !confirmPassword) {
            e.preventDefault();
            alert('請填寫所有密碼欄位');
            return false;
        }
        
        if (newPassword !== confirmPassword) {
            e.preventDefault();
            alert('新密碼與確認密碼不相符');
            confirmPasswordField.focus();
            return false;
        }
        
        // 密碼強度驗證
        if (newPassword.length < 6) {
            e.preventDefault();
            alert('密碼長度必須至少6個字元');
            newPasswordField.focus();
            return false;
        }
        
        if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(newPassword)) {
            e.preventDefault();
            alert('密碼必須包含大小寫字母和數字');
            newPasswordField.focus();
            return false;
        }
    });
}

// 頁面載入完成後初始化所有功能
document.addEventListener('DOMContentLoaded', function() {
    console.log('修改密碼頁面初始化中...');
    
    try {
        initPasswordStrengthChecker();
        initFormValidation();
        console.log('修改密碼頁面初始化完成');
    } catch (error) {
        console.error('修改密碼頁面初始化失敗:', error);
    }
});