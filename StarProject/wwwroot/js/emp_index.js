// 員工列表篩選搜尋功能
$(document).ready(function () {
    // 初始化工具提示
    initializeTooltips();

    // 綁定事件
    bindSearchEvents();
    bindFilterEvents();
    bindCheckboxEvents();
    bindDetailEvents();

    // 初始載入數據
    refreshEmpTable(1);
});

// 初始化工具提示
function initializeTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// 綁定搜尋相關事件
function bindSearchEvents() {
    // 搜尋按鈕點擊
    $('#btnSearch').click(function () {
        refreshEmpTable(1);
    });

    // 搜尋框按 Enter
    $('#searchInput').keypress(function (e) {
        if (e.which == 13) {
            refreshEmpTable(1);
        }
    });

    // 每頁筆數變更
    $('#pageSizeSelect').change(function () {
        refreshEmpTable(1);
    });
}

// 綁定篩選相關事件
function bindFilterEvents() {
    // 篩選完成按鈕
    $('#filterFinsh').click(function () {
        updateFilterCount();
        refreshEmpTable(1);
        $('#advancedFilter').dropdown('hide');
    });

    // 篩選清除按鈕
    $('.btn-outline-search').click(function () {
        clearAllFilters();
        updateFilterCount();
        refreshEmpTable(1);
    });

    // 篩選項目變更時更新計數
    $('.form-check-input, #dateFrom, #dateTo').on('change', function () {
        updateFilterCount();
    });
}

// 綁定核取方塊事件
function bindCheckboxEvents() {
    // 全選功能
    $(document).on('change', '#checkAll', function () {
        const isChecked = $(this).prop('checked');
        $('.checkbox').prop('checked', isChecked);
        updateSelectCount();
    });

    // 單一核取方塊
    $(document).on('change', '.checkbox', function () {
        updateSelectCount();

        // 更新全選狀態
        const totalCheckboxes = $('.checkbox').length;
        const checkedCheckboxes = $('.checkbox:checked').length;

        if (checkedCheckboxes === 0) {
            $('#checkAll').prop('indeterminate', false);
            $('#checkAll').prop('checked', false);
        } else if (checkedCheckboxes === totalCheckboxes) {
            $('#checkAll').prop('indeterminate', false);
            $('#checkAll').prop('checked', true);
        } else {
            $('#checkAll').prop('indeterminate', true);
        }
    });

    // 批量刪除
    $('#delectAllYes').click(function () {
        const selectedIds = [];
        $('.checkbox:checked').each(function () {
            selectedIds.push($(this).val());
        });

        if (selectedIds.length > 0) {
            deleteSelectedEmps(selectedIds);
        }
    });
}

// 綁定詳細內容事件
function bindDetailEvents() {
    $(document).on('click', '.btn-detail', function () {
        const data = $(this).data();

        $('#offcanvasName').text(data.name);
        $('#offcanvasEmpCode').text(data.empcode);
        $('#offcanvasDepartment').text(data.department);
        $('#offcanvasRole').text(data.role);
        $('#offcanvasHireDate').text(data.hiredate);
        $('#offcanvasStatus').html(data.status === '在職'
            ? '<span class="badge bg-success">在職</span>'
            : '<span class="badge bg-danger">離職</span>');

        // 設定按鈕連結
        const editUrl = `/Emps/Edit/${data.id}`;
        const detailUrl = `/Emps/Details/${data.id}`;

        $('#offcanvasEditBtn').attr('href', editUrl);
        $('#offcanvasDetailsBtn').attr('href', detailUrl);
        $('#offcanvasDeleteBtn').attr('data-id', data.id);
        $('#deleteOne').attr('action', `/Emps/Delete/${data.id}`);
    });
}

// 更新篩選計數
function updateFilterCount() {
    let count = 0;

    // 計算部門篩選
    count += $('#collapseDepartment .form-check-input:checked').length;

    // 計算職位篩選
    count += $('#collapseRole .form-check-input:checked').length;

    // 計算在職狀態篩選
    count += $('#collapseStatus .form-check-input:checked').length;

    // 計算日期篩選
    if ($('#dateFrom').val() || $('#dateTo').val()) {
        count += 1;
    }

    const $filterCount = $('#filterCount');
    if (count > 0) {
        $filterCount.text(count).show();
        updateSelectedFiltersText();
    } else {
        $filterCount.hide();
        $('#selectedFiltersDiv').hide();
    }
}

// 更新已選篩選條件文字
function updateSelectedFiltersText() {
    const filters = [];

    // 部門篩選
    const selectedDepts = [];
    $('#collapseDepartment .form-check-input:checked').each(function () {
        selectedDepts.push($(this).val());
    });
    if (selectedDepts.length > 0) {
        filters.push(`部門: ${selectedDepts.join(', ')}`);
    }

    // 職位篩選
    const selectedRoles = [];
    $('#collapseRole .form-check-input:checked').each(function () {
        selectedRoles.push($(this).val());
    });
    if (selectedRoles.length > 0) {
        filters.push(`職位: ${selectedRoles.join(', ')}`);
    }

    // 在職狀態篩選
    const selectedStatus = [];
    $('#collapseStatus .form-check-input:checked').each(function () {
        selectedStatus.push($(this).val());
    });
    if (selectedStatus.length > 0) {
        filters.push(`狀態: ${selectedStatus.join(', ')}`);
    }

    // 日期篩選
    const dateFrom = $('#dateFrom').val();
    const dateTo = $('#dateTo').val();
    if (dateFrom || dateTo) {
        let dateText = '入職日期: ';
        if (dateFrom && dateTo) {
            dateText += `${dateFrom} ~ ${dateTo}`;
        } else if (dateFrom) {
            dateText += `${dateFrom} 之後`;
        } else if (dateTo) {
            dateText += `${dateTo} 之前`;
        }
        filters.push(dateText);
    }

    if (filters.length > 0) {
        $('#selectedFiltersText').text(`已套用篩選: ${filters.join(' | ')}`);
        $('#selectedFiltersDiv').show();
    } else {
        $('#selectedFiltersDiv').hide();
    }
}

// 清除所有篩選
function clearAllFilters() {
    $('.form-check-input').prop('checked', false);
    $('#dateFrom, #dateTo').val('');
    $('#selectedFiltersDiv').hide();
}

// 更新選取計數
function updateSelectCount() {
    const checkedCount = $('.checkbox:checked').length;
    const $selectNum = $('#selectNum');
    const $delectAll = $('#delectAll');

    if (checkedCount > 0) {
        $selectNum.text(`已選取${checkedCount}項`).show();
        $delectAll.show();
    } else {
        $selectNum.hide();
        $delectAll.hide();
    }
}
// 刷新員工表格
function refreshEmpTable(page = 1) {
    // 收集搜尋參數
    const searchData = {
        keyword: $('#searchInput').val()?.trim() || '',
        page: page,
        pageSize: parseInt($('#pageSizeSelect').val()) || 10,
        departments: [],
        roles: [],
        statuses: [],
        dateFrom: $('#dateFrom').val() || '',
        dateTo: $('#dateTo').val() || ''
    };

    // 收集部門篩選 - 確保取得正確的值
    $('#collapseDepartment .form-check-input:checked').each(function () {
        const value = $(this).val();
        if (value && value.trim() !== '') {
            searchData.departments.push(value.trim());
        }
    });

    // 收集職位篩選 - 確保取得正確的值  
    $('#collapseRole .form-check-input:checked').each(function () {
        const value = $(this).val();
        if (value && value.trim() !== '') {
            searchData.roles.push(value.trim());
        }
    });

    // 收集狀態篩選
    $('#collapseStatus .form-check-input:checked').each(function () {
        const value = $(this).val();
        if (value && value.trim() !== '') {
            searchData.statuses.push(value.trim());
        }
    });

    // Debug: 輸出搜尋參數
    console.log('搜尋參數:', searchData);

    // 顯示載入中
    showLoading();

    // 獲取 Anti-Forgery Token
    const token = $('input[name="__RequestVerificationToken"]').val();

    // 檢查是否找到 token
    if (!token) {
        console.error('找不到 Anti-Forgery Token');
        hideLoading();
        alert('頁面初始化錯誤，請重新整理頁面');
        return;
    }

    // 發送 AJAX 請求
    $.ajax({
        url: '/Emps/SearchEmps',
        type: 'POST',
        data: JSON.stringify(searchData),
        contentType: 'application/json',
        beforeSend: function (xhr) {
            xhr.setRequestHeader('RequestVerificationToken', token);
        },
        success: function (response) {
            try {
                console.log('後端回應:', response);

                if (response.success === false) {
                    alert(response.message || '搜尋失敗');
                    return;
                }

                // 更新表格內容
                if (response.empRows) {
                    $('#tableBody').html(response.empRows);
                } else {
                    $('#tableBody').html('<tr><td colspan="8" class="text-center py-4">沒有找到符合條件的資料</td></tr>');
                }

                // 更新分頁
                if (response.pagination) {
                    $('#pagination').html(response.pagination);
                }

                // 更新分頁資訊
                if (response.totalCount !== undefined) {
                    $('#paginationInfo').text(response.currentPage || page);
                }

                // 重置選取狀態
                $('#checkAll').prop('checked', false).prop('indeterminate', false);
                updateSelectCount();

                // 重新初始化工具提示
                initializeTooltips();

            } catch (error) {
                console.error('處理響應時發生錯誤:', error);
                console.error('原始回應:', response);
                alert('資料處理失敗');
            } finally {
                hideLoading();
            }
        },
        error: function (xhr, status, error) {
            console.error('搜尋失敗:', error);
            console.error('HTTP Status:', xhr.status);
            console.error('Response Text:', xhr.responseText);

            hideLoading();

            let errorMessage = '搜尋失敗，請稍後再試';

            try {
                const errorResponse = JSON.parse(xhr.responseText);
                if (errorResponse && errorResponse.message) {
                    errorMessage = errorResponse.message;
                }
            } catch (e) {
                // 無法解析 JSON，使用預設錯誤訊息
            }

            if (xhr.status === 400) {
                errorMessage = '請求參數錯誤';
            } else if (xhr.status === 403) {
                errorMessage = '沒有權限執行此操作';
            } else if (xhr.status === 500) {
                errorMessage = '伺服器內部錯誤';
            }

            alert(errorMessage);
        }
    });
}

// 更新篩選計數 - 修復計數邏輯
function updateFilterCount() {
    let count = 0;

    // 計算部門篩選 - 確保選擇器正確
    const deptChecked = $('#collapseDepartment .form-check-input:checked');
    console.log('部門篩選數量:', deptChecked.length);
    count += deptChecked.length;

    // 計算職位篩選
    const roleChecked = $('#collapseRole .form-check-input:checked');
    console.log('職位篩選數量:', roleChecked.length);
    count += roleChecked.length;

    // 計算在職狀態篩選
    const statusChecked = $('#collapseStatus .form-check-input:checked');
    console.log('狀態篩選數量:', statusChecked.length);
    count += statusChecked.length;

    // 計算日期篩選
    const dateFrom = $('#dateFrom').val();
    const dateTo = $('#dateTo').val();
    if (dateFrom || dateTo) {
        count += 1;
    }

    console.log('總篩選條件數量:', count);

    const $filterCount = $('#filterCount');
    if (count > 0) {
        $filterCount.text(count).show();
        updateSelectedFiltersText();
    } else {
        $filterCount.hide();
        $('#selectedFiltersDiv').hide();
    }
}

// 更新已選篩選條件文字 - 修復取值邏輯
function updateSelectedFiltersText() {
    const filters = [];

    // 部門篩選
    const selectedDepts = [];
    $('#collapseDepartment .form-check-input:checked').each(function () {
        const label = $(this).closest('.form-check').find('label').text().trim();
        if (label) {
            selectedDepts.push(label);
        }
    });
    if (selectedDepts.length > 0) {
        filters.push(`部門: ${selectedDepts.join(', ')}`);
    }

    // 職位篩選
    const selectedRoles = [];
    $('#collapseRole .form-check-input:checked').each(function () {
        const label = $(this).closest('.form-check').find('label').text().trim();
        if (label) {
            selectedRoles.push(label);
        }
    });
    if (selectedRoles.length > 0) {
        filters.push(`職位: ${selectedRoles.join(', ')}`);
    }

    // 在職狀態篩選
    const selectedStatus = [];
    $('#collapseStatus .form-check-input:checked').each(function () {
        const label = $(this).closest('.form-check').find('label').text().trim();
        if (label) {
            selectedStatus.push(label);
        }
    });
    if (selectedStatus.length > 0) {
        filters.push(`狀態: ${selectedStatus.join(', ')}`);
    }

    // 日期篩選
    const dateFrom = $('#dateFrom').val();
    const dateTo = $('#dateTo').val();
    if (dateFrom || dateTo) {
        let dateText = '入職日期: ';
        if (dateFrom && dateTo) {
            dateText += `${dateFrom} ~ ${dateTo}`;
        } else if (dateFrom) {
            dateText += `${dateFrom} 之後`;
        } else if (dateTo) {
            dateText += `${dateTo} 之前`;
        }
        filters.push(dateText);
    }

    if (filters.length > 0) {
        $('#selectedFiltersText').text(`已套用篩選: ${filters.join(' | ')}`);
        $('#selectedFiltersDiv').show();
    } else {
        $('#selectedFiltersDiv').hide();
    }
}

// 刪除選取的員工
function deleteSelectedEmps(empIds) {
    $.ajax({
        url: '/Emps/DeleteMultiple',
        type: 'POST',
        data: JSON.stringify(empIds),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                // 重新載入表格
                refreshEmpTable(1);
                alert('刪除成功');
            } else {
                alert('刪除失敗: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('刪除失敗:', error);
            alert('刪除失敗，請稍後再試');
        }
    });
}

// 顯示載入中
function showLoading() {
    $('#tableBody').html('<tr><td colspan="8" class="text-center py-4"><i class="fas fa-spinner fa-spin"></i> 載入中...</td></tr>');
}

// 隱藏載入中
function hideLoading() {
    // 由 AJAX 成功回調處理
}

// 全域函數：刷新表格（供分頁按鈕使用）
window.refreshEmpTable = refreshEmpTable;