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


// 修改：將 loadEmployeeDetails 函數中的 offcanvas 相關操作改為 modal
function loadEmployeeDetails(id, empcode, name, department, role, hiredate, status) {
    console.log('載入員工詳細資料:', { id, empcode, name, department, role, hiredate, status });

    // 建立員工資料物件
    const empData = {
        id: id,
        empcode: empcode,
        name: name,
        department: department,
        role: role,
        hiredate: hiredate,
        status: status
    };

    // 檢查是否有必要的資料
    if (!empData.id) {
        console.error('找不到員工 ID');
        alert('無法載入員工詳細資料');
        return;
    }

    // 填入詳細資料到 modal (offcanvas的ID已在HTML中改為modal)
    $('#modalName').text(empData.name || '未知');
    $('#modalEmpCode').text(empData.empcode || '未知');
    $('#modalDepartment').text(empData.department || '未知');
    $('#modalRole').text(empData.role || '未知');
    $('#modalHireDate').text(empData.hiredate || '未知');

    // 設定狀態顯示
    let statusText = '';
    if (empData.status === '在職' || empData.status === true || empData.status === 'true') {
        statusText = '在職';
    } else {
        statusText = '離職';
    }
    $('#modalStatus').text(statusText);

    // 設定按鈕連結 (Offcanvas按鈕的ID已在HTML中改為modal)
    const editUrl = `/Emps/Edit/${empData.id}`;
    const deleteUrl = `/Emps/Delete/${empData.id}`;

    $('#modalEditBtn').attr('href', editUrl);
    $('#modalDeleteBtn').attr('data-id', empData.id);

    // 設定刪除表單的 action
    // 注意: 您原程式碼中 offcanvasDetailsBtn 可能不存在，已移除。
    // 注意: deleteOne 表單的 action 會在 deleteModal 彈出時動態更新，這裡不需額外設定
    // $('#deleteOne').attr('action', `/Emps/Delete/${empData.id}`);

    // 顯示 modal
    const detailModal = new bootstrap.Modal(document.getElementById('detailModal'));
    detailModal.show();
}

// 綁定詳細資料事件
function bindDetailEvents() {
    // 使用事件委託處理動態載入的按鈕
    $(document).on('click', '.btn-detail', function (e) {
        e.preventDefault();

        console.log('詳細按鈕被點擊'); // Debug 用

        // 取得按鈕上的 data 屬性
        const $btn = $(this);
        const empData = {
            id: $btn.attr('data-id') || $btn.data('id'),
            name: $btn.attr('data-name') || $btn.data('name'),
            empcode: $btn.attr('data-empcode') || $btn.data('empcode'),
            department: $btn.attr('data-department') || $btn.data('department'),
            role: $btn.attr('data-role') || $btn.data('role'),
            hiredate: $btn.attr('data-hiredate') || $btn.data('hiredate'),
            status: $btn.attr('data-status') || $btn.data('status')
        };

        // 使用統一的載入函數
        loadEmployeeDetails(
            empData.id,
            empData.empcode,
            empData.name,
            empData.department,
            empData.role,
            empData.hiredate,
            empData.status
        );
    });

    // 處理單一刪除按鈕的資料傳遞
    // 注意: offcanvasDeleteBtn 的 ID 已改為 modalDeleteBtn
    $(document).on('click', '#modalDeleteBtn', function () {
        const empId = $(this).attr('data-id');
        $('#deleteOne').attr('action', `/Emps/Delete/${empId}`);
    });
}

// 將 loadEmployeeDetails 設為全域函數，供外部調用
window.loadEmployeeDetails = loadEmployeeDetails;

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

    // 收集部門篩選
    $('#collapseDepartment .form-check-input:checked').each(function () {
        const value = $(this).val();
        if (value && value.trim() !== '') {
            searchData.departments.push(value.trim());
        }
    });

    // 收集職位篩選
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

    console.log('搜尋參數:', searchData);

    // 顯示載入中
    showLoading();

    // 獲取 Anti-Forgery Token
    const token = $('input[name="__RequestVerificationToken"]').val();

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

                // 重置選取狀態
                $('#checkAll').prop('checked', false).prop('indeterminate', false);
                updateSelectCount();

                // 重新初始化工具提示
                initializeTooltips();

                // 重要：重新綁定詳細按鈕事件（因為內容是動態載入的）
                console.log('重新綁定詳細按鈕事件');

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