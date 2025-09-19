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

// 綁定核取方塊事件（移除全選功能，只保留批量離職）
function bindCheckboxEvents() {
    // 單一核取方塊
    $(document).on('change', '.checkbox', function () {
        updateSelectCount();
    });

    // 批量離職事件處理
    $(document).on('click', '#delectAllYes', function () {
        const selectedIds = [];
        $('.checkbox:checked').each(function () {
            const empId = $(this).val();
            if (empId) {
                selectedIds.push(empId);
            }
        });

        if (selectedIds.length === 0) {
            alert('請選擇要離職的員工');
            return;
        }

        console.log('準備批量離職的員工ID:', selectedIds);

        // 顯示載入狀態
        const $btn = $(this);
        const originalText = $btn.html();
        $btn.html('<i class="fas fa-spinner fa-spin"></i> 處理中...');
        $btn.prop('disabled', true);

        deleteSelectedEmps(selectedIds, function () {
            // 恢復按鈕狀態
            $btn.html(originalText);
            $btn.prop('disabled', false);
        });
    });
}

// 綁定詳細資料事件
function bindDetailEvents() {
    // 使用事件委託處理動態載入的詳細按鈕
    $(document).on('click', '.btn-detail', function (e) {
        e.preventDefault();

        console.log('詳細按鈕被點擊');

        const $btn = $(this);
        const empData = {
            id: $btn.data('id'),
            name: $btn.data('name'),
            empcode: $btn.data('empcode'),
            department: $btn.data('department'),
            role: $btn.data('role'),
            hiredate: $btn.data('hiredate'),
            status: $btn.data('status')
        };

        // 載入員工詳細資料到 modal
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

    // 處理 Modal 中的離職按鈕
    $(document).on('click', '#modalDeleteBtn', function (e) {
        e.preventDefault();

        const empId = $(this).data('id');
        const empName = $('#modalName').text();

        if (!empId) {
            console.error('未找到員工ID');
            alert('無法執行離職操作');
            return;
        }

        if (confirm(`確定要將員工「${empName}」設為離職狀態嗎？`)) {
            submitResignation(empId);
        }
    });

    // 處理表格中直接的離職按鈕
    $(document).on('click', '.btn-resign', function (e) {
        e.preventDefault();

        const empId = $(this).data('id');
        const empName = $(this).data('name');

        if (!empId) {
            console.error('未找到員工ID');
            return;
        }

        if (confirm(`確定要將員工「${empName}」設為離職狀態嗎？`)) {
            submitResignation(empId);
        }
    });
}

// 載入員工詳細資料
function loadEmployeeDetails(id, empcode, name, department, role, hiredate, status) {
    console.log('載入員工詳細資料:', { id, empcode, name, department, role, hiredate, status });

    const empData = {
        id: id,
        empcode: empcode,
        name: name,
        department: department,
        role: role,
        hiredate: hiredate,
        status: status
    };

    if (!empData.id) {
        console.error('找不到員工 ID');
        alert('無法載入員工詳細資料');
        return;
    }

    // 填入詳細資料到 modal
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

    // 設定按鈕連結和 data 屬性
    const editUrl = `/Emps/Edit/${empData.id}`;
    $('#modalEditBtn').attr('href', editUrl);
    $('#modalDeleteBtn').data('id', empData.id);

    // 顯示 modal
    $('#detailModal').modal('show');
}

// 提交離職請求
function submitResignation(empId) {
    // 顯示載入狀態
    const $modalBtn = $('#modalDeleteBtn');
    const originalText = $modalBtn.html();
    $modalBtn.html('<i class="fas fa-spinner fa-spin"></i> 處理中...');
    $modalBtn.prop('disabled', true);

    // 獲取 CSRF token
    const token = $('input[name="__RequestVerificationToken"]').val();

    if (!token) {
        console.error('找不到 CSRF token');
        alert('頁面錯誤，請重新整理');
        // 恢復按鈕狀態
        $modalBtn.html(originalText);
        $modalBtn.prop('disabled', false);
        return;
    }

    $.ajax({
        url: `/Emps/Delete/${empId}`,
        type: 'POST',
        data: {
            __RequestVerificationToken: token
        },
        success: function (response) {
            console.log('單一離職成功回應:', response);

            // 恢復按鈕狀態
            $modalBtn.html(originalText);
            $modalBtn.prop('disabled', false);

            // 關閉 modal
            $('#detailModal').modal('hide');

            // 顯示成功訊息
            showSuccessMessage('員工已成功設為離職狀態');

            // 重新載入員工列表
            refreshEmpTable();
        },
        error: function (xhr, status, error) {
            console.error('單一離職失敗:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);

            // 恢復按鈕狀態
            $modalBtn.html(originalText);
            $modalBtn.prop('disabled', false);

            let errorMessage = '離職操作失敗，請稍後再試';

            if (xhr.status === 404) {
                errorMessage = '找不到該員工';
            } else if (xhr.status === 403) {
                errorMessage = '沒有權限執行此操作';
            }

            showErrorMessage(errorMessage);
        }
    });
}

// 批量刪除選取的員工
function deleteSelectedEmps(empIds, callback) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    if (!token) {
        console.error('找不到 CSRF token');
        alert('頁面錯誤，請重新整理');
        if (callback) callback();
        return;
    }

    console.log('發送批量離職請求:', empIds);

    $.ajax({
        url: '/Emps/DeleteMultiple',
        type: 'POST',
        data: JSON.stringify(empIds),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': token
        },
        success: function (response) {
            console.log('批量離職成功回應:', response);

            if (callback) callback();

            if (response && response.success) {
                // 關閉批量刪除 modal
                $('#deleteAllModal').modal('hide');

                // 顯示成功訊息
                showSuccessMessage(`成功將 ${empIds.length} 位員工設為離職狀態`);

                // 清除選取狀態
                $('.checkbox').prop('checked', false);
                updateSelectCount();

                // 重新載入表格
                refreshEmpTable(1);
            } else {
                showErrorMessage(response?.message || '批量離職操作失敗');
            }
        },
        error: function (xhr, status, error) {
            console.error('批量離職失敗:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);

            if (callback) callback();

            let errorMessage = '批量離職操作失敗，請稍後再試';

            if (xhr.status === 403) {
                errorMessage = '沒有權限執行此操作';
            }

            showErrorMessage(errorMessage);
        }
    });
}

// 顯示成功訊息
function showSuccessMessage(message) {
    const alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="fas fa-check-circle"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('main .d-flex.align-items-center.mb-3').after(alertHtml);

    setTimeout(function () {
        $('.alert-success').alert('close');
    }, 5000);
}

// 顯示錯誤訊息
function showErrorMessage(message) {
    const alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-triangle"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('main .d-flex.align-items-center.mb-3').after(alertHtml);

    setTimeout(function () {
        $('.alert-danger').alert('close');
    }, 5000);
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
                console.log('搜尋成功回應:', response);

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
                $('.checkbox').prop('checked', false);
                updateSelectCount();

                // 重新初始化工具提示
                initializeTooltips();

                console.log('表格更新完成');

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

// 顯示載入中
function showLoading() {
    $('#tableBody').html('<tr><td colspan="8" class="text-center py-4"><i class="fas fa-spinner fa-spin"></i> 載入中...</td></tr>');
}

// 隱藏載入中
function hideLoading() {
    // 由 AJAX 成功回調處理
}

// 設為全域函數，供外部調用
window.loadEmployeeDetails = loadEmployeeDetails;
window.refreshEmpTable = refreshEmpTable;