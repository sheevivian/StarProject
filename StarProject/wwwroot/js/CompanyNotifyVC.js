document.addEventListener("DOMContentLoaded", function () {
    const items = document.querySelectorAll('#notify-marquee .notify-item');
    const total = items.length;
    const intervalTime = 5000;

    if (total === 0) return;

    // 取得上次索引，若沒有則從 0 開始
    let current = parseInt(localStorage.getItem('notifyCurrentIndex')) || 0;

    // 初始化：隱藏所有，第一筆淡入
    items.forEach(item => item.style.display = 'none');

    const firstItem = items[current];
    firstItem.style.display = 'block';
    firstItem.classList.add('animate__animated', 'animate__fadeInDownShort');

    firstItem.addEventListener('animationend', function handler() {
        firstItem.classList.remove('animate__animated', 'animate__fadeInDownShort');
        firstItem.removeEventListener('animationend', handler);

        // 開始輪播
        setTimeout(showNext, intervalTime);
    }, { once: true });

    function showNext() {
        const currentItem = items[current];
        const nextIndex = (current + 1) % total;
        const nextItem = items[nextIndex];

        currentItem.classList.remove('animate__fadeInDownShort', 'animate__animated');
        currentItem.classList.add('animate__fadeOutDownShort', 'animate__animated');

        currentItem.addEventListener('animationend', function handler() {
            currentItem.style.display = 'none';
            currentItem.classList.remove('animate__fadeOutDownShort', 'animate__animated');
            currentItem.removeEventListener('animationend', handler);

            nextItem.style.display = 'block';
            nextItem.classList.add('animate__animated', 'animate__fadeInDownShort');

            current = nextIndex;
            localStorage.setItem('notifyCurrentIndex', current);

            setTimeout(showNext, intervalTime);
        }, { once: true });
    }
});