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
// ===================================================================
// Dashboard: biểu đồ Chart.js (doanh thu, người dùng hoạt động, doughnut cổng sạc)
// ===================================================================
function initDashboardCharts() {
    if (typeof Chart === 'undefined' || !window.__dashboardData) return;

    var data = window.__dashboardData;
    var blue = '#2563eb';
    var purple = '#7c3aed';
    Chart.defaults.font.family = "'Inter', -apple-system, BlinkMacSystemFont, sans-serif";
    Chart.defaults.color = '#64748b';

    var revenueChart, activeUserChart, doughnutChart;

    function makeGradient(ctx, colorRgb, alphaTop, alphaBottom) {
        var g = ctx.createLinearGradient(0, 0, 0, 240);
        g.addColorStop(0, 'rgba(' + colorRgb + ',' + (alphaTop || 0.35) + ')');
        g.addColorStop(1, 'rgba(' + colorRgb + ',' + (alphaBottom || 0.0) + ')');
        return g;
    }

    function renderRevenueChart(key) {
        var el = document.getElementById('revenueChart');
        if (!el) return;
        var series = data.revenue[key];
        if (!series) return;
        if (revenueChart) revenueChart.destroy();
        var ctx = el.getContext('2d');
        var isLine = key === 'today' || key === 'd90';

        revenueChart = new Chart(ctx, {
            type: isLine ? 'line' : 'bar',
            data: {
                labels: series.labels,
                datasets: [{
                    label: 'Doanh thu (triệu VNĐ)',
                    data: series.values,
                    borderColor: blue,
                    backgroundColor: isLine ? makeGradient(ctx, '37, 99, 235', 0.28, 0.01) : blue,
                    fill: isLine,
                    tension: 0.35,
                    pointRadius: key === 'today' ? 3 : (key === 'd90' ? 2 : 0),
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: blue,
                    pointBorderWidth: 2,
                    borderWidth: 2.4,
                    borderRadius: isLine ? 0 : 6,
                    maxBarThickness: 28
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 600 },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        intersect: false,
                        mode: 'index',
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        padding: 10,
                        titleFont: { size: 12, weight: 'bold' },
                        bodyFont: { size: 12 },
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx2) {
                                return ' Doanh thu: ' + ctx2.parsed.y.toFixed(2) + ' triệu VNĐ';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { display: false },
                        ticks: { font: { size: 11 } }
                    },
                    y: {
                        grid: { color: '#f1f5f9' },
                        border: { display: false },
                        ticks: {
                            font: { size: 11 },
                            callback: function (v) { return v + 'M'; }
                        }
                    }
                }
            }
        });
    }

    function renderActiveUserChart(key) {
        var el = document.getElementById('activeUserChart');
        if (!el) return;
        var series = data.activeUsers[key];
        if (!series) return;
        if (activeUserChart) activeUserChart.destroy();
        var ctx = el.getContext('2d');

        activeUserChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: series.labels,
                datasets: [{
                    label: 'Người dùng hoạt động',
                    data: series.values,
                    borderColor: purple,
                    backgroundColor: makeGradient(ctx, '124, 58, 237', 0.25, 0.01),
                    fill: true,
                    tension: 0.38,
                    pointRadius: 3,
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: purple,
                    pointBorderWidth: 2,
                    borderWidth: 2.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 600 },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        padding: 10,
                        titleFont: { size: 12, weight: 'bold' },
                        bodyFont: { size: 12 },
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx2) { return ' Khách hàng: ' + ctx2.parsed.y; }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { display: false },
                        ticks: { font: { size: 11 } }
                    },
                    y: {
                        grid: { color: '#f1f5f9' },
                        border: { display: false },
                        ticks: {
                            precision: 0,
                            font: { size: 11 }
                        }
                    }
                }
            }
        });
    }

    function renderDoughnut() {
        var el = document.getElementById('connectorDoughnut');
        if (!el || !data.doughnut) return;
        if (doughnutChart) doughnutChart.destroy();

        doughnutChart = new Chart(el.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: data.doughnut.labels,
                datasets: [{
                    data: data.doughnut.values,
                    backgroundColor: ['#16a34a', '#2563eb', '#ef4444'],
                    hoverBackgroundColor: ['#15803d', '#1d4ed8', '#dc2626'],
                    borderWidth: 3,
                    borderColor: '#ffffff',
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '72%',
                animation: { animateRotate: true, animateScale: true, duration: 800 },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        padding: 10,
                        titleFont: { size: 12, weight: 'bold' },
                        bodyFont: { size: 12 },
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx2) {
                                var total = ctx2.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var pct = total ? (ctx2.parsed / total * 100).toFixed(1) : '0.0';
                                return ' ' + ctx2.label + ': ' + ctx2.parsed + ' cổng (' + pct + '%)';
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

    var rangeMap = { 'today': 'today', '7d': 'd7', '30d': 'd30', '90d': 'd90' };
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
    window.initDashboardCharts = initDashboardCharts;
}

window.initDashboardCharts = initDashboardCharts;
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initDashboardCharts);
} else {
    initDashboardCharts();
}
