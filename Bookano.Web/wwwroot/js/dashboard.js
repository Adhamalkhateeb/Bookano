let rentalsChart = null;

drawMembersChart();

async function drawMembersChart() {
    var ctx = document.getElementById('SubscribersPerGovernorate');

    // Define colors
    var primaryColor = KTUtil.getCssVariableValue('--kt-primary');
    var dangerColor = KTUtil.getCssVariableValue('--kt-danger');
    var successColor = KTUtil.getCssVariableValue('--kt-success');
    var warningColor = KTUtil.getCssVariableValue('--kt-warning');
    var infoColor = KTUtil.getCssVariableValue('--kt-info');

    // Define fonts
    var fontFamily = KTUtil.getCssVariableValue('--bs-font-sans-serif');


    const response = await fetch("/Dashboard/GetSubscribersPerGovernorate");
    if (!response.ok) return;

    const figures = await response.json();


    // Chart labels
    const labels = [...figures.map(i => i.label)]

    // Chart data
    const data = {
        labels: labels,
        datasets: [{
            data: figures.map(i => i.value),
            backgroundColor: [
                infoColor,
                successColor,
                warningColor,
                primaryColor,
                dangerColor,
                "#5F91B6",
                "#D3F6FC",
                "#C8B0D2"
            ],
            borderRadius:8
        }]
    };

    // Chart config
    const config = {
        type: 'doughnut',
        data: data,
        options: {
            plugins: {
                title: {
                    display: false,
                }
            },
            responsive: true,
        },
        defaults: {
            global: {
                defaultFont: fontFamily
            }
        }
    };

    var myChart = new Chart(ctx, config);
}

async function drawRentalsChart(startDate = null, endDate = null) {
    var element = document.getElementById('RentalsPerDay');
    if (!element) return;

    const start = startDate ? moment(startDate).format('YYYY-MM-DD') : null;
    const end = endDate ? moment(endDate).format('YYYY-MM-DD') : null;

    var height = parseInt(KTUtil.css(element, 'height'));
    var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
    var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
    var baseColor = KTUtil.getCssVariableValue('--kt-info');
    var lightColor = KTUtil.getCssVariableValue('--kt-info-light');

    let url = '/Dashboard/GetRentalsPerDay';
    if (start && end) {
        url += `?startDate=${start}&endDate=${end}`;
    }

    const response = await fetch(url);
    if (!response.ok) return;

    const data = await response.json();

    var options = {
        series: [{
            name: 'Rentals',
            data: data.map(i => i.value)
        }],
        chart: {
            fontFamily: 'inherit',
            type: 'area',
            height: height,
            toolbar: { show: false }
        },
        legend: { show: false },
        dataLabels: { enabled: false },
        fill: { type: 'solid', opacity: 1 },
        stroke: {
            curve: 'smooth',
            show: true,
            width: 3,
            colors: [baseColor]
        },
        xaxis: {
            categories: data.map(i => i.label),
            axisBorder: { show: false },
            axisTicks: { show: false },
            labels: {
                style: { colors: labelColor, fontSize: '12px' }
            },
            crosshairs: {
                position: 'front',
                stroke: { color: baseColor, width: 1, dashArray: 3 }
            },
            tooltip: {
                enabled: true,
                style: { fontSize: '12px' }
            }
        },
        yaxis: {
            tickAmount: data.length > 0 ? Math.max(...data.map(i => i.value)) : 5,
            min: 0,
            labels: {
                style: { colors: labelColor, fontSize: '12px' }
            }
        },
        states: {
            normal: { filter: { type: 'none', value: 0 } },
            hover: { filter: { type: 'none', value: 0 } },
            active: {
                allowMultipleDataPointsSelection: false,
                filter: { type: 'none', value: 0 }
            }
        },
        tooltip: { style: { fontSize: '12px' } },
        colors: [lightColor],
        grid: {
            borderColor: borderColor,
            strokeDashArray: 4,
            yaxis: { lines: { show: true } }
        },
        markers: { strokeColor: baseColor, strokeWidth: 3 }
    };

    if (rentalsChart) {
        rentalsChart.updateOptions(options);
    } else {
        rentalsChart = new ApexCharts(element, options);
        rentalsChart.render();
    }
}

document.addEventListener("DOMContentLoaded", function () {
    var start = moment().subtract(29, "days");
    var end = moment();

    function cb(start, end) {
        $("#DateRange").html(
            start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY")
        );
        drawRentalsChart(start, end);
    }

    $("#DateRange").daterangepicker({
        startDate: start,
        endDate: end,
        ranges: {
            "Today": [moment(), moment()],
            "Yesterday": [moment().subtract(1, "days"), moment().subtract(1, "days")],
            "Last 7 Days": [moment().subtract(6, "days"), moment()],
            "Last 30 Days": [moment().subtract(29, "days"), moment()],
            "This Month": [moment().startOf("month"), moment().endOf("month")],
            "Last Month": [
                moment().subtract(1, "month").startOf("month"),
                moment().subtract(1, "month").endOf("month")
            ]
        }
    }, cb);

    cb(start, end);
});