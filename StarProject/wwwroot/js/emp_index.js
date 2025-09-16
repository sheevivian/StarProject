// 員工列表頁面 JavaScript
$(document).ready(function() {
    // 初始化工具提示
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 搜尋功能
    $('#btnSearch').click(function() {
        performSearch();
    });

    $('#searchInput').keypress(function(e) {
        if (e.which == 13) {
            performSearch();
        }
    });

    // 每頁筆數變更
    $('#pageSizeSelect').change(function() {
        refreshEmpTable(1);
    });

    // 全選功能
    $('#checkAll').change(function() {
        $('.checkbox').prop('checked', $(this).is(':checked'));
        updateSelectedCount();
    });

    // 個別選取功能
    $(document).on('change', '.checkbox', function() {
        updateSelectedCount();
        updateCheckAllState();
    });

    // 詳細內容按鈕事件
    $(document).on('click', '.btn-detail', function() {
        const empData = $(this).data();
        showEmpDetail(empData);
    });

    // 進階篩選完成
    $('#filterFinsh').click(function() {
        applyAdvancedFilters();
    });

    // 篩選清除
    $('.btn-outline-search').click(function() {
        clearAllFilters();
    });

    // 刪除單一員工
    $(document).on('click', '#offcanvasDeleteBtn', function() {
        const empId = $(this).data('id');
        $('#deleteOne').attr('action', '/Emp/Delete/' + empId);
    });

    // 批量刪除
    $('#delectAllYes').click(function() {
        const selectedIds = getSelectedIds();
        if (selectedIds.length > 0) {
            batchDeleteEmps(selectedIds);
        }
    });
});

// 執行搜尋
function performSearch() {
    const searchTerm = $('#searchInput').val();
    refreshEmpTable(1, searchTerm);
}

// 重新載入員工表格
function refreshEmpTable(page = 1, searchTerm = '') {
    const pageSize = $('#pageSizeSelect').val();
    const filters = getActiveFilters();
    
    $.ajax({
        url: '/Emp/GetEmpList',
        type: 'GET',
        data: {
            page: page,
            pageSize: pageSize,
            searchTerm: searchTerm,
            ...filters
        },
        success: function(data) {
            $('#tableBody').html(data.rows);
            $('#pagination').html(data.pagination);
            $('#paginationInfo').text(page);
        },
        error: function() {
            alert('載入資料時發生錯誤');
        }
    });
}

// 顯示員工詳細資料
function showEmpDetail(empData) {
    $('#offcanvasName').text(empData.name);
    $('#offcanvasEmpCode').text(empData.empcode);
    $('#offcanvasDepartment').text(empData.department);
    $('#offcanvasRole').text(empData.role);
    $('#offcanvasHireDate').text(empData.hiredate);
    
    const statusBadge = empData.status === '在職' ? 
        '<span class="badge bg-success">在職</span>' : 
        '<span class="badge bg-danger">離職</span>';
    $('#offcanvasStatus').html(statusBadge);
    
    // 設定編輯和查看按鈕連結
    $('#offcanvasEditBtn').attr('href', '/Emp/Edit/' + empData.id);
    $('#offcanvasDetailsBtn').attr('href', '/Emp/Details/' + empData.id);
    $('#offcanvasDeleteBtn').data('id', empData.id);
}

// 更新選取數量
function updateSelectedCount() {
    const checkedCount = $('.checkbox:checked').length;
    if (checkedCount > 0) {
        $('#selectNum').show().text(`已選取${checkedCount}項`);
        $('#delectAll').show();
    } else {
        $('#selectNum').hide();
        $('#delectAll').hide();
    }
}

// 更新全選狀態
function updateCheckAllState() {
    const totalCheckboxes = $('.checkbox').length;
    const checkedCheckboxes = $('.checkbox:checked').length;
    
    if (checkedCheckboxes === 0) {
        $('#checkAll').prop('indeterminate', false).prop('checked', false);
    } else if (checkedCheckboxes === totalCheckboxes) {
        $('#checkAll').prop('indeterminate', false).prop('checked', true);
    } else {
        $('#checkAll').prop('indeterminate', true);
    }
}

// 取得已選取的ID
function getSelectedIds() {
    return $('.checkbox:checked').map(function() {
        return $(this).val();
    }).get();
}

// 取得啟用的篩選條件
function getActiveFilters() {
    const filters = {};
    
    // 部門篩選
    const selectedDepts = $('input[id^="dept"]:checked').map(function() {
        return $(this).val();