// 分頁功能
function refreshPromotionTable(pageNumber) {
    const searchKeyword = $('#searchInput').val();
    const pageSize = $('#pageSizeSelect').val();
    const sortBy = $('#sortBy').val() || 'Name';
    const sortOrder = $('#sortOrder').val() || 'asc';

    $.ajax({
        url: '/Promotion/Index',
        type: 'GET',
        data: {
            page: pageNumber,
            pageSize: pageSize,
            searchKeyword: searchKeyword,
            sortBy: sortBy,
            sortOrder: sortOrder
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (data) {
            $('#tableBody').html(data);
            updatePaginationInfo(pageNumber);
        },
        error: function () {
            alert('載入失敗，請重試');
        }
    });
}

// 更新分頁資訊
function updatePaginationInfo(currentPage) {
    $('#paginationInfo').text(currentPage);
}

// 每頁筆數變更
$('#pageSizeSelect').change(function () {
    refreshPromotionTable(1);
});

// 搜尋功能
$('#btnSearch').click(function () {
    refreshPromotionTable(1);
});

// Enter 鍵搜尋
$('#searchInput').keypress(function (e) {
    if (e.which == 13) {
        refreshPromotionTable(1);
        return false;
    }
});

// 全選功能
$('#checkAll').change(function () {
    $('.checkbox').prop('checked', $(this).prop('checked'));
    updateSelectCount();
});

// 更新選取數量
function updateSelectCount() {
    const checkedCount = $('.checkbox:checked').length;
    if (checkedCount > 0) {
        $('#selectNum').text('已選取' + checkedCount + '項').show();
        $('#delectAll').show();
    } else {
        $('#selectNum').hide();
        $('#delectAll').hide();
    }
}

// 單項勾選
$(document).on('change', '.checkbox', function () {
    updateSelectCount();
});

// 詳細內容按鈕
$(document).on('click', '.btn-detail', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');

    // 更新 offcanvas 內容
    $('#offcanvasName').text(name);
    $('#offcanvasEditBtn').attr('href', '/Promotion/Edit/' + id);
    $('#offcanvasDeleteBtn').data('id', id);
});

// 刪除單項
$('#deleteOnebtn').click(function () {
    const id = $('#offcanvasDeleteBtn').data('id');
    // 實作刪除邏輯
});

// 批量刪除
$('#delectAllYes').click(function () {
    const selectedIds = [];
    $('.checkbox:checked').each(function () {
        selectedIds.push($(this).val());
    });

    // 實作批量刪除邏輯
    console.log('刪除選取項目:', selectedIds);
});