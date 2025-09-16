let pageNumber = parseInt($('#pageSizeSelect').val()) || 10; // 預設每頁數量

$(function () {

	// checkbox 勾選
	$(document).on('click', '.checkbox', function () {
		let checked = $(this).prop('checked');

		let count = $('.checkbox:checked').length;

		if (count > 0) {
			$('#delectAll, #selectNum').show();
			$('#selectNum').text(`已選取${count}項`);
		} else {
			$('#delectAll, #selectNum').hide();
		}

		if (checked) {
			$(this).closest('tr').addClass('active');
		} else {
			$(this).closest('tr').removeClass('active');
		}

		$('#checkAll').prop('checked', $('.checkbox').length === $('.checkbox:checked').length);
	});

	// 全選
	$('#checkAll').click(function () {

		if ($(this).prop('checked')) {
			$('.checkbox').prop('checked', true);
			$('tbody tr').addClass('active');
			$('#delectAll, #selectNum').show();

			// 計算勾選數量
			let count = $('.checkbox:checked').length;

			if (count > 0) {
				$('#selectNum').text(`已選取${count}項`);
			}
		}
		else {
			$('.checkbox').prop('checked', false);
			$('tbody tr').removeClass('active');
			$('#delectAll, #selectNum').hide();
		}
	})

	// 批次刪除
	$('#delectAllYes').click(function () {
		// 收集所有勾選的 checkbox 的 value (假設 value 存 id)
		let ids = $('.checkbox:checked').map(function () {
			return $(this).val();
		}).get();

		if (ids.length === 0) return; // 沒有選擇不動作

		// ✅ 取得 CSRF token
		var token = $('input[name="__RequestVerificationToken"]').val();

		$.ajax({
			url: '/LostInfo/DeleteMultiple', // 後端批次刪除 Action
			type: 'POST',
			data: {
				__RequestVerificationToken: token, // CSRF 驗證
				ids: ids // 傳 id 陣列
			},
			traditional: true, // 陣列傳送格式
			success: function () {
				let page = parseInt($('#paginationInfo').text()) || 1; // 取得目前頁碼
				refreshTable(page);
				$('.checkbox:checked').closest('tr').remove();
				$('#delectAll, #selectNum').hide();
				$('#checkAll').prop('checked', false);
			},
			error: function () {
				alert("刪除失敗！");
			}
		});
	})


	// 點搜尋按鈕
	$('#btnSearch').on("click", () => {
		console.log($('#searchInput').val());
		refreshTable(1);
		updateSelectedFilters()
	});

	// 也可以輸入框 keyup 即時觸發
	$('#searchInput').on("keyup", () => {
		console.log($('#searchInput').val());
		refreshTable(1);
		updateSelectedFilters()
	});

	$('#filterFinsh').click(function () {
		updateFilterCount();
		updateSelectedFilters()
		//讀取目前進階條件
		let categories = [...document.querySelectorAll("#collapseCategory input:checked")].map(cb => cb.value);
		let statuses = [...document.querySelectorAll("#collapseStatus input:checked")].map(cb => cb.value);
		let dateFrom = $("#dateFrom").val();
		let dateTo = $("#dateTo").val();

		let filters = { categories, statuses, dateFrom, dateTo };

		refreshTable(1);

		// 找到最接近的 dropdown-toggle
		var toggleBtn = $(this).closest('.dropdown').find('.dropdown-toggle')[0];

		// 取得 bootstrap dropdown instance
		var dropdown = bootstrap.Dropdown.getInstance(toggleBtn);

		// 沒有的話就新建
		if (!dropdown) dropdown = new bootstrap.Dropdown(toggleBtn);

		// 呼叫官方的 hide 方法來關閉
		dropdown.hide();
	});


	// 搜尋清除按鈕
	document.querySelector(".btn-outline-search").addEventListener("click", function () {
		document.querySelectorAll("input[type=checkbox]").forEach(cb => cb.checked = false);
		document.getElementById("dateFrom").value = "";
		document.getElementById("dateTo").value = "";
		document.getElementById("searchInput").value = "";
		updateSelectedFilters();
		updateFilterCount()
		refreshTable(1);
	});

	// 日期區間邏輯
	const dateFrom = document.getElementById("dateFrom");
	const dateTo = document.getElementById("dateTo");

	dateFrom.addEventListener("change", () => {
		if (dateFrom.value) {
			dateTo.min = dateFrom.value; // 動態限制
			if (dateTo.value && dateTo.value < dateFrom.value) {
				dateTo.value = ""; // 自動清空不合法日期
			}
		} else {
			dateTo.min = ""; // 移除限制
		}
	});


	// 更新篩選顯示文字
	function updateSelectedFilters() {
		let selected = [];

		// 物品分類
		let categories = $("#collapseCategory input:checked").map(function () { return $(this).next("label").text(); }).get();
		if (categories.length) selected.push("<strong>分類：</strong>" + categories.join(", "));

		// 領取狀態
		let statuses = $("#collapseStatus input:checked").map(function () { return $(this).next("label").text(); }).get();
		if (statuses.length) selected.push("<strong>狀態：</strong>" + statuses.join(", "));

		// 日期區間
		let from = $("#dateFrom").val();
		let to = $("#dateTo").val();
		if (from || to) selected.push("<strong>日期：</strong>" + (from ? from : "不限") + " ~ " + (to ? to : "不限"));

		// 顯示或隱藏 div
		if (selected.length) {
			$("#selectedFiltersDiv").show();
			$("#selectedFiltersText").html(selected.join("&nbsp;&nbsp;&nbsp;&nbsp;"));
		} else {
			$("#selectedFiltersDiv").hide();
			$("#selectedFiltersText").html("");
		}
	}

	function updateFilterCount() {
		let count = 0;

		// 物品分類
		if ($("#collapseCategory input:checked").length > 0) count++;

		// 領取狀態
		if ($("#collapseStatus input:checked").length > 0) count++;

		// 拾獲日期 (任意一個日期不空算1)
		if ($("#dateFrom").val() || $("#dateTo").val()) count++;

		// 更新右上角 tooltip
		if (count > 0) {
			$("#filterCount").show();
			$("#filterCount").text(`${count}`);
		} else {
			$("#filterCount").hide();
		}
	}

	// 分頁按鈕
	$(".pagination").on("click", ".pagination .page-link", function (e) {
		e.preventDefault();
		let page = $(this).data("page");
		refreshTable(page);
	});


	// Offcanvas 詳細資料
	$(document).on('click', '.btn-detail', function () {
		var btn = $(this);

		$('#offcanvasName').text(btn.data('name'));
		$('#offcanvasDesc').text(btn.data('desc'));
		$('#offcanvasfounddate').text(btn.data('founddate'));
		$('#offcanvasCreatedDate').text(btn.data('createddate'));
		$('#offcanvasStatus').text(btn.data('status'));
		$('#offcanvasImage').attr("src", btn.data('image'));

		// 顯示擁有者資訊 (只在狀態為已領取時)
		if (btn.data('status') === "已領取") {
			$('#ownerNameWrapper').show();
			$('#ownerPhoneWrapper').show();
			$('#offcanvasOwnerName').text(btn.data('ownername'));
			$('#offcanvasOwnerPhone').text(btn.data('ownerphone'));
		} else {
			$('#ownerNameWrapper').hide();
			$('#ownerPhoneWrapper').hide();
		}

		// 🔑 取得這筆資料的 ID
		var id = btn.data('id');

		// 1️⃣ 設定「編輯」按鈕連結
		$('#offcanvasEditBtn').attr("href", "/LostInfo/Edit/" + id);

		// 2️⃣ 設定「刪除」按鈕 data-id
		$('#offcanvasDeleteBtn').attr("data-id", id);

		// 3️⃣ 刪除 Modal 開啟時，動態修改表單 action
		$('#deleteModal').on('show.bs.modal', function () {
			$('#deleteOne').attr("action", "/LostInfo/Delete/" + id);
		});

		// 打開 Offcanvas
		var offcanvasEl = document.getElementById('offcanvasDetail');
		var offcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
		offcanvas.show();
	});

	//單一刪除分頁更新
	$(document).on('click', '#deleteOnebtn', async function (e) {
		e.preventDefault();

		// 從 form 的 action 拿 id
		let form = $(this).closest('form');
		let action = form.attr('action'); // /LostInfo/Delete/123
		let id = action.split('/').pop(); // 拿最後一段

		if (!id) return;

		let token = $('input[name="__RequestVerificationToken"]').val();
		let page = parseInt($('#paginationInfo').text()) || 1;

		try {
			await $.ajax({
				url: '/LostInfo/Delete',  // 改成 Delete
				type: 'POST',
				data: { id: id, __RequestVerificationToken: token }
			});

			// 刪除後刷新列表
			refreshTable(page);

			// 關閉 Offcanvas
			var offcanvasEl = document.getElementById('offcanvasDetail');
			var offcanvas = bootstrap.Offcanvas.getInstance(offcanvasEl);
			if (offcanvas) offcanvas.hide();

			// 關閉 Modal
			$('#deleteModal').modal('hide');

		} catch (err) {
			alert("刪除失敗：" + (err.responseText || err.statusText));
		}
	});

	// 當 select 改變時
	$('#pageSizeSelect').on('change', function () {
		pageNumber = parseInt($(this).val()); // 更新 pageNumber
		console.log("pageNumber 更新為:", pageNumber);
		refreshTable(1);
	});

});

// 統一刷新列表
async function refreshTable(page = 1, filters = {}) {
	try {

		filters.Page = page;
		filters.PageSize = pageNumber;
		filters.keyword = $('#searchInput').val();
		filters.Categories = [...document.querySelectorAll("#collapseCategory input:checked")].map(cb => cb.value);
		filters.Statuses = [...document.querySelectorAll("#collapseStatus input:checked")].map(cb => cb.value);
		filters.DateFrom = $("#dateFrom").val();
		filters.DateTo = $("#dateTo").val();

		// 取得 CSRF Token
		filters.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();

		let response = await $.ajax({
			url: '/LostInfo/SearchSelect', // 或 DeleteConfirmed 也可以共用
			type: 'POST',
			data: JSON.stringify(filters),
			contentType: "application/json"
		});

		// 如果本頁沒有資料且頁數大於 1，自動回前一頁
		if (response.tableHtml.trim() === "" && page > 1) {
			return await refreshTable(page - 1, filters);
		}

		// 更新 tableBody
		$("#tableBody").html(response.tableHtml);

		// 更新 pagination
		$("#pagination").html(response.paginationHtml);

		// 更新分頁顯示
		$('#paginationInfo').text(page);

		$('#delectAll, #selectNum').hide();
		$('#checkAll').prop('checked', false);

	} catch (err) {
		alert("更新列表失敗：" + err.responseText || err.statusText);
	}
}