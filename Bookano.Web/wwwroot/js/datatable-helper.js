function initDataTable({
    tableId,
    ajaxUrl,
    columns,
    order = [[1, 'asc']],
    processingId
}) {
    const tableEl = document.getElementById(tableId);
    if (!tableEl) return;

    const processingEl = document.getElementById(processingId);

    const setProcessing = (isProcessing) => {
        if (!processingEl) return;
        processingEl.classList.toggle('d-none', !isProcessing);
    };

    setProcessing(true);

    const datatable = new DataTable(tableEl, {
        processing: true,
        serverSide: true,
        stateSave: true,
        language: {
            processing: '<span class="loader"></span>'
        },
        ajax: {
            url: ajaxUrl,
            type: "POST",
            headers: {
                "RequestVerificationToken": getCsrfToken()
            },
        },
        drawCallback: function () {
            if (window.KTMenu) {
                KTMenu.createInstances();
            }
        },
        order,
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns,
    });

    const search = document.querySelector('[data-kt-filter="search"]');
    if (search) {
        search.addEventListener('input', (e) => {
            datatable.search(e.target.value).draw();
        });
    }

    datatable.on('processing.dt', function (e, settings, processing) {
        setProcessing(processing);
    });

    return datatable;
}


function renderDate(data, type) {
    if (!data) return "";

    const date = new Date(data);

    if (type === "display") {
        return date.toLocaleDateString("en-US", {
            month: "short",   
            day: "2-digit",   
            year: "numeric"
        });
    }

    return date.toISOString();
}


function renderDateTime(data, type) {
    if (!data) return "";

    const date = new Date(data);

    if (type === "display") {
        return date.toLocaleString("en-US", {
            month: "short", 
            day: "numeric",   
            year: "numeric",  
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: true      
        });
    }

    // 🔥 keep ISO for correct sorting
    return date.toISOString();
}

function renderStatusBadge(value, trueText, falseText) {
    return `<span class="badge badge-light-${value ? "danger" : "success"} js-status">
                ${value ? trueText : falseText}
            </span>`;
}