// 生成星空背景
function createStars() {
    const starsContainer = document.getElementById('stars');
    const numberOfStars = 100;

    for (let i = 0; i < numberOfStars; i++) {
        const star = document.createElement('div');
        star.className = 'star';
        
        // 隨機位置
        star.style.left = Math.random() * 100 + '%';
        star.style.top = Math.random() * 100 + '%';
        
        // 隨機大小
        const size = Math.random() * 3 + 1;
        star.style.width = size + 'px';
        star.style.height = size + 'px';
        
        // 隨機動畫延遲
        star.style.animationDelay = Math.random() * 3 + 's';
        star.style.webkitAnimationDelay = Math.random() * 3 + 's';
        
        starsContainer.appendChild(star);
    }
}

// 聯繫管理員功能
function contactAdmin() {
    // 這裡可以改為實際的聯繫方式或彈出模態框
    alert('請聯繫系統管理員：\n📧 admin@observatory.tw\n📞 (02) 1234-5678');
}

// 頁面載入時生成星空
document.addEventListener('DOMContentLoaded', function() {
    createStars();
    
    // 鍵盤快捷鍵
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = '/';
            }
        }
    });
});