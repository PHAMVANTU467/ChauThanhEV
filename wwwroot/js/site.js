// ===================================================================
// Đăng nhập: hiện / ẩn mật khẩu
// ===================================================================
document.querySelectorAll('.toggle-password').forEach(function (btn) {
    btn.addEventListener('click', function () {
        var input = btn.closest('.input-with-icon').querySelector('input');
        input.type = input.type === 'password' ? 'text' : 'password';
    });
});

// ===================================================================
// Thu gọn / mở rộng sidebar
// ===================================================================
var menuToggle = document.querySelector('.menu-toggle');
if (menuToggle) {
    menuToggle.addEventListener('click', function () {
        var sidebar = document.querySelector('.sidebar');
        if (!sidebar) return;
        if (window.matchMedia('(max-width: 900px)').matches) {
            sidebar.classList.toggle('mobile-open');
        } else {
            sidebar.classList.toggle('collapsed');
        }
    });
}

document.querySelectorAll('.nav-parent').forEach(function (button) {
    button.addEventListener('click', function () {
        var group = button.closest('.nav-group');
        var expanded = group.classList.toggle('expanded');
        button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    });
});

// ===================================================================
// Toast thông báo (đọc từ window.__pendingToast do _Layout bơm ra từ TempData)
// ===================================================================
function showToast(message, type) {
    var container = document.getElementById('toastContainer');
    if (!container) return;
    var el = document.createElement('div');
    el.className = 'toast toast-' + (type === 'error' ? 'error' : 'success');
    el.innerHTML = '<span class="toast-icon">' + (type === 'error' ? '⚠' : '✓') + '</span><span>' + message + '</span>';
    container.appendChild(el);
    requestAnimationFrame(function () { el.classList.add('show'); });
    setTimeout(function () {
        el.classList.remove('show');
        setTimeout(function () { el.remove(); }, 300);
    }, 3800);
}
if (window.__pendingToast) {
    showToast(window.__pendingToast.message, window.__pendingToast.type);
}

// ===================================================================
// Modal chung: mở / đóng bằng data-open-modal / data-close-modal
// ===================================================================
document.addEventListener('click', function (e) {
    var openBtn = e.target.closest('[data-open-modal]');
    if (openBtn) {
        var id = openBtn.getAttribute('data-open-modal');
        var modal = document.getElementById(id);
        if (modal) modal.classList.add('open');
        return;
    }
    var closeBtn = e.target.closest('[data-close-modal]');
    if (closeBtn) {
        var overlay = closeBtn.closest('.modal-overlay');
        if (overlay) overlay.classList.remove('open');
        return;
    }
    if (e.target.classList && e.target.classList.contains('modal-overlay')) {
        e.target.classList.remove('open');
    }
});
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay.open').forEach(function (m) { m.classList.remove('open'); });
    }
});

// Xác nhận trước khi xóa
document.addEventListener('submit', function (e) {
    var form = e.target;
    if (form.matches('[data-confirm]')) {
        var msg = form.getAttribute('data-confirm') || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
        if (!confirm(msg)) e.preventDefault();
    }
});

// Đóng bảng bất thường: hiện/ẩn khung nhập số tiền khi chọn hành động
document.querySelectorAll('[data-toggle-fields]').forEach(function (select) {
    select.addEventListener('change', function () {
        var targetSelector = select.getAttribute('data-toggle-fields');
        document.querySelectorAll(targetSelector).forEach(function (el) { el.style.display = 'none'; });
        var current = document.querySelector('[data-fields-for="' + select.value + '"]');
        if (current) current.style.display = 'block';
    });
});

// ===================================================================
// Tab chuyển đổi ở các trang danh sách (Đơn hàng / Vận hành) — điều hướng qua query string
// (các nút này là thẻ <a>, nên chỉ cần CSS active; không cần JS)
// ===================================================================

// ===================================================================
// Xuất dữ liệu CSV / Excel (giả lập) từ một bảng HTML hiện có trên trang
// ===================================================================
function getExportRows(tableId) {
    var table = document.getElementById(tableId);
    if (!table) return null;
    var rows = [];
    table.querySelectorAll('tr').forEach(function (tr) {
        var cells = [];
        tr.querySelectorAll('th, td').forEach(function (cell) {
            if (cell.classList.contains('no-export')) return;
            cells.push(cell.innerText.trim().replace(/\s+/g, ' '));
        });
        if (cells.length) rows.push(cells);
    });
    return rows;
}

function downloadBlob(content, filename, mime) {
    var blob = new Blob([content], { type: mime });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}

function exportTableToCsv(tableId, filename) {
    var rows = getExportRows(tableId);
    if (!rows) return;
    var csv = rows.map(function (r) {
        return r.map(function (v) { return '"' + v.replace(/"/g, '""') + '"'; }).join(',');
    }).join('\r\n');
    downloadBlob('\uFEFF' + csv, filename.endsWith('.csv') ? filename : filename + '.csv', 'text/csv;charset=utf-8;');
    showToast('Đã xuất file CSV thành công.', 'success');
}

function exportTableToExcel(tableId, filename) {
    var rows = getExportRows(tableId);
    if (!rows) return;
    var html = '<table>' + rows.map(function (r) {
        return '<tr>' + r.map(function (v) { return '<td>' + v.replace(/&/g, '&amp;').replace(/</g, '&lt;') + '</td>'; }).join('') + '</tr>';
    }).join('') + '</table>';
    var content = '\uFEFF<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><meta charset="UTF-8"></head><body>' + html + '</body></html>';
    downloadBlob(content, filename.endsWith('.xls') ? filename : filename + '.xls', 'application/vnd.ms-excel');
    showToast('Đã xuất file Excel thành công.', 'success');
}

document.querySelectorAll('[data-export-csv]').forEach(function (btn) {
    btn.addEventListener('click', function () {
        exportTableToCsv(btn.getAttribute('data-export-csv'), btn.getAttribute('data-export-name') || 'du-lieu');
    });
});
document.querySelectorAll('[data-export-excel]').forEach(function (btn) {
    btn.addEventListener('click', function () {
        exportTableToExcel(btn.getAttribute('data-export-excel'), btn.getAttribute('data-export-name') || 'du-lieu');
    });
});

// ===================================================================
// Dashboard: biểu đồ Chart.js (doanh thu, người dùng hoạt động, doughnut cổng sạc)
// ===================================================================
(function () {
    if (typeof Chart === 'undefined' || !window.__dashboardData) return;

    var data = window.__dashboardData;
    var blue = '#2563eb';
    Chart.defaults.font.family = "'Inter', sans-serif";
    Chart.defaults.color = '#667085';

    var revenueChart, activeUserChart, doughnutChart;

    function makeGradient(ctx, color, alphaTop) {
        var g = ctx.createLinearGradient(0, 0, 0, 220);
        g.addColorStop(0, color.replace('ALPHA', alphaTop));
        g.addColorStop(1, color.replace('ALPHA', '0.02'));
        return g;
    }

    function renderRevenueChart(key) {
        var el = document.getElementById('revenueChart');
        if (!el) return;
        var series = data.revenue[key];
        if (revenueChart) revenueChart.destroy();
        var ctx = el.getContext('2d');
        var isToday = key === 'today';
        revenueChart = new Chart(ctx, {
            type: isToday ? 'line' : 'bar',
            data: {
                labels: series.labels,
                datasets: [{
                    data: series.values,
                    borderColor: blue,
                    backgroundColor: isToday ? makeGradient(ctx, 'rgba(37,99,235,ALPHA)', 0.28) : blue,
                    fill: isToday,
                    tension: 0.4,
                    pointRadius: 0,
                    borderWidth: 2.2,
                    borderRadius: isToday ? 0 : 5,
                    maxBarThickness: 26
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        intersect: false, mode: 'index',
                        callbacks: { label: function (ctx2) { return ' Doanh thu: ' + ctx2.parsed.y.toFixed(1) + ' triệu đ'; } }
                    }
                },
                scales: {
                    x: { grid: { display: false }, border: { display: false } },
                    y: { grid: { color: '#eef1f6' }, border: { display: false }, ticks: { callback: function (v) { return v + 'M'; } } }
                }
            }
        });
    }

    function renderActiveUserChart(key) {
        var el = document.getElementById('activeUserChart');
        if (!el) return;
        var series = data.activeUsers[key];
        if (activeUserChart) activeUserChart.destroy();
        activeUserChart = new Chart(el.getContext('2d'), {
            type: 'line',
            data: {
                labels: series.labels,
                datasets: [{
                    data: series.values,
                    borderColor: '#7c3aed',
                    backgroundColor: '#7c3aed',
                    tension: 0.4,
                    pointRadius: 3,
                    pointBackgroundColor: '#7c3aed',
                    borderWidth: 2.2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: function (ctx2) { return ' Người dùng: ' + ctx2.parsed.y; } } }
                },
                scales: {
                    x: { grid: { display: false }, border: { display: false } },
                    y: { grid: { color: '#eef1f6' }, border: { display: false }, ticks: { precision: 0 } }
                }
            }
        });
    }

    function renderDoughnut() {
        var el = document.getElementById('connectorDoughnut');
        if (!el || !data.doughnut) return;
        doughnutChart = new Chart(el.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: data.doughnut.labels,
                datasets: [{
                    data: data.doughnut.values,
                    backgroundColor: ['#16a34a', '#2563eb', '#e4463c'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (ctx2) {
                                var total = ctx2.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var pct = total ? (ctx2.parsed / total * 100).toFixed(2) : '0.00';
                                return ' ' + ctx2.label + ': ' + ctx2.parsed + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });
    }

    renderRevenueChart('today');
    renderActiveUserChart('today');
    renderDoughnut();

    var rangeMap = { 'today': 'today', '7d': 'd7', '30d': 'd30' };
    document.querySelectorAll('.range-tabs').forEach(function (group) {
        var target = group.getAttribute('data-target');
        group.querySelectorAll('button').forEach(function (btn) {
            btn.addEventListener('click', function () {
                group.querySelectorAll('button').forEach(function (b) { b.classList.remove('active'); });
                btn.classList.add('active');
                var key = rangeMap[btn.getAttribute('data-range')];
                if (target === 'revenue') renderRevenueChart(key);
                if (target === 'users') renderActiveUserChart(key);
            });
        });
    });
})();
