document.addEventListener('DOMContentLoaded', function () {
    const offcanvasDetails = document.getElementById('offcanvasDetails');
    const offcanvasBody = document.getElementById('offcanvasBody');
    const offcanvasTitle = document.getElementById('offcanvasDetailsLabel');

    offcanvasDetails.addEventListener('show.bs.offcanvas', function (event) {
        // 取得觸發 offcanvas 的按鈕
        const button = event.relatedTarget;
        // 從按鈕的 data 屬性中取得 LostInfo 的 No
        const lostInfoId = button.getAttribute('data-lostinfo-id');

        // 模擬從伺服器取得資料 (這裡用一個簡單的陣列)
        const lostInfos = @Html.Raw(Json.Serialize(Model));
        const item = lostInfos.find(info => info.No == lostInfoId);

        // 更新 offcanvas 的標題和內容
        if (item) {
            offcanvasTitle.textContent = `${item.Name} 詳細內容`;
            offcanvasBody.innerHTML = `
                    <p><strong>物品照片:</strong> <img src="${item.Image}" alt="物品照片" /></p>
                    <p><strong>建立日期:</strong> ${new Date(item.CreatedDate).toLocaleDateString()}</p>
                    <p><strong>擁有者姓名:</strong> ${item.OwnerName}</p>
                    <p><strong>擁有者電話:</strong> ${item.OwnerPhone}</p>
                    <p><strong>描述:</strong> ${item.Desc}</p>
                `;
        }
    });
});
