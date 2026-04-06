/**
 * admin-reports.js
 * Reports and Charts dashboard logic.
 */

const ReportsDashboard = () => {
    let salesChart = null;
    let statusChart = null;
    let fp = null;
    let currentStartDate = null;
    let currentEndDate = null;
    let apiUrls = null;

    const init = (urls) => {
        apiUrls = urls;
        console.info("Reports Dashboard Init", apiUrls);

        // Initialize Flatpickr
        const pickerEle = document.getElementById("dateRangePicker");
        if (pickerEle && typeof flatpickr !== 'undefined') {
            fp = flatpickr(pickerEle, {
                mode: "range",
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                locale: "vn",
                maxDate: "today",
                defaultDate: [
                    new Date(new Date().setDate(new Date().getDate() - 6)),
                    new Date()
                ],
                onClose: function (selectedDates) {
                    if (selectedDates.length === 2) {
                        updateReports(selectedDates[0], selectedDates[1]);
                        const btn = document.getElementById('btnQuickFilter');
                        if (btn) btn.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> Tùy chọn';
                    }
                }
            });
        }

        // Quick Filters
        document.querySelectorAll('.quick-filter').forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const days = parseInt(this.getAttribute('data-days'));
                const filterText = this.innerText;

                const btn = document.getElementById('btnQuickFilter');
                if (btn) btn.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> ' + filterText;

                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - (days - 1));

                if (fp) fp.setDate([start, end]);
                updateReports(start, end);
            });
        });

        // Default Load (Last 7 days)
        const end = new Date();
        const start = new Date();
        start.setDate(end.getDate() - 6);

        const btnF = document.getElementById('btnQuickFilter');
        if (btnF) btnF.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> 7 ngày qua';
        if (fp) fp.setDate([start, end]);

        updateReports(start, end);

        // Export Excel
        const btnExport = document.getElementById('btnExport');
        if (btnExport) {
            btnExport.addEventListener('click', function () {
                if (currentStartDate && currentEndDate) {
                    window.location.href = `${apiUrls.export}?startDate=${currentStartDate}&endDate=${currentEndDate}`;
                } else {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire('Chú ý', 'Vui lòng chọn khoảng thời gian để xuất báo cáo', 'warning');
                    } else {
                        alert('Vui lòng chọn khoảng thời gian để xuất báo cáo');
                    }
                }
            });
        }
    };

    async function updateReports(dStart, dEnd) {
        currentStartDate = dStart.toISOString().split('T')[0];
        currentEndDate = dEnd.toISOString().split('T')[0];

        const sStr = currentStartDate;
        const eStr = currentEndDate;

        const titleEle = document.getElementById('salesChartTitle');
        if (titleEle) {
            const sFormatted = sStr.split('-').reverse().join('/');
            const eFormatted = eStr.split('-').reverse().join('/');
            titleEle.innerText = `${sFormatted} - ${eFormatted}`;
        }

        toggleLoading(true);

        const query = `?startDate=${sStr}&endDate=${eStr}`;

        try {
            const [resSales, resStatus] = await Promise.all([
                fetch(apiUrls.sales + query).then(r => r.json()),
                fetch(apiUrls.status + query).then(r => r.json())
            ]);

            renderSalesChart(resSales);
            renderStatusChart(resStatus);
        } catch (error) {
            console.error("API Error:", error);
        } finally {
            toggleLoading(false);
        }
    }

    function toggleLoading(isLoading) {
        const sLoad = document.getElementById('salesLoading');
        const tLoad = document.getElementById('statusLoading');
        const display = isLoading ? 'flex' : 'none';
        if (sLoad) sLoad.style.display = display;
        if (tLoad) tLoad.style.display = display;
    }

    function renderSalesChart(data) {
        const ctx = document.getElementById('salesChart');
        if (!ctx || typeof Chart === 'undefined') return;

        const type = data.type || 'line';
        const unit = data.unit || '₫';

        const chartCtx = ctx.getContext('2d');
        let background = 'rgba(78, 115, 223, 0.1)';
        if (type === 'line') {
            const gradient = chartCtx.createLinearGradient(0, 0, 0, 350);
            gradient.addColorStop(0, 'rgba(78, 115, 223, 0.2)');
            gradient.addColorStop(1, 'rgba(78, 115, 223, 0)');
            background = gradient;
        } else {
            background = '#4e73df';
        }

        const existing = Chart.getChart(ctx);
        if (existing) existing.destroy();

        salesChart = new Chart(ctx, {
            type: type,
            data: {
                labels: data.labels,
                datasets: [{
                    label: unit === '₫' ? 'Doanh thu (₫)' : 'Số lượng đơn (đơn)',
                    data: data.values,
                    fill: type === 'line',
                    backgroundColor: background,
                    borderColor: '#4e73df',
                    borderWidth: type === 'line' ? 3 : 1,
                    borderRadius: type === 'bar' ? 4 : 0,
                    pointBackgroundColor: '#fff',
                    pointBorderColor: '#4e73df',
                    pointBorderWidth: 2,
                    pointRadius: type === 'line' ? 5 : 0,
                    tension: 0.4
                }]
            },
            options: {
                maintainAspectRatio: false,
                responsive: true,
                layout: { padding: { top: 5, bottom: 15, left: 10, right: 0 } },
                scales: {
                    y: {
                        beginAtZero: true,
                        min: 0,
                        ticks: {
                            maxTicksLimit: 10,
                            font: { size: 10 },
                            callback: function (value) {
                                if (unit === '₫') {
                                    if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                                    return value.toLocaleString() + ' ₫';
                                }
                                return value;
                            }
                        }
                    },
                    x: { ticks: { maxRotation: 0, minRotation: 0, font: { size: 11 }, padding: 3 } }
                },
                plugins: {
                    tooltip: {
                        enabled: true,
                        callbacks: {
                            label: function (context) {
                                let label = (unit === '₫' ? 'Doanh thu' : 'Số lượng') + ': ';
                                if (context.parsed.y !== null) {
                                    if (unit === '₫') {
                                        label += new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(context.parsed.y);
                                    } else {
                                        label += context.parsed.y + ' đơn';
                                    }
                                }
                                return label;
                            }
                        }
                    }
                }
            }
        });
    }

    function renderStatusChart(data) {
        const ctx = document.getElementById('orderStatusChart');
        const noData = document.getElementById('noDataAlert');
        if (!ctx || !noData || typeof Chart === 'undefined') return;

        const existing = Chart.getChart(ctx);
        if (existing) existing.destroy();

        if (!data || data.length === 0) {
            noData.classList.remove('d-none');
            ctx.style.display = 'none';
            return;
        }

        noData.classList.add('d-none');
        ctx.style.display = 'block';

        statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.status),
                datasets: [{
                    data: data.map(x => x.count),
                    backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'],
                    borderWidth: 2
                }]
            },
            options: {
                maintainAspectRatio: false,
                responsive: true,
                plugins: {
                    legend: { position: 'bottom', labels: { boxWidth: 12, padding: 15 } }
                },
                cutout: '75%'
            }
        });
    }

    return { init };
};
