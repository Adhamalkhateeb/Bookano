// ============================================================
// DataTable Helper
// ============================================================
function initDataTable({
    tableId,
    ajaxUrl,
    columns,
    order = [[1, 'asc']],
    processingId,
    onDraw = null
}) {
    const tableEl = document.getElementById(tableId);
    if (!tableEl) return null;

    const processingEl = document.getElementById(processingId);

    const setProcessing = (isProcessing) => {
        processingEl?.classList.toggle('d-none', !isProcessing);
    };

    setProcessing(true);

    const datatable = new DataTable(tableEl, {
        processing: true,
        serverSide: !!ajaxUrl,
        stateSave: true,
        language: {
            processing: '<span class="loader"></span>'
        },
        ajax: ajaxUrl
            ? {
                url: ajaxUrl,
                type: 'POST',
                headers: { 'RequestVerificationToken': getCsrfToken() },
            }
            : undefined,
        drawCallback: function () {
            KTMenu?.createInstances();
            formatDates(this.api().table().node());
            setTimeout(() => {
                initMobileSortBars();
                if (typeof onDraw === 'function') {
                    onDraw(this.api());
                }
            }, 50);
        },
        order,
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns,
    });

    // Single authoritative reference
    window.appDatatable = datatable;

    const searchEl = document.querySelector('[data-kt-filter="search"]');

    if (searchEl) {
        const savedState = datatable.state.loaded();

        if (savedState?.search?.search) {
            searchEl.value = savedState.search.search;
        }

        searchEl.addEventListener('input', (e) => {
            datatable.search(e.target.value).draw();
        });
    }
 

    datatable.on('processing.dt', (_e, _settings, processing) => {
        setProcessing(processing);
    });

    return datatable;
}

// ============================================================
// Render helpers
// ============================================================

function renderDate(data, type) {
    if (!data) return '';

    const date = new Date(data);
    if (isNaN(date)) return '';

    if (type === 'display') {
        return date.toLocaleDateString('en-US', {
            month: 'short',
            day: '2-digit',
            year: 'numeric',
        });
    }

    return date.toISOString();
}

const renderDateTime = (data, type) => {
    if (!data) return '';

    if (type === 'sort' || type === 'type') return data;

    if (type === 'filter') return new Date(data).toLocaleString();

    // display: formatDates() picks this up after draw
    return `<span data-utc="${data}" data-order="${data}"></span>`;
};

function renderStatusBadge(value, trueText, falseText) {
    return `<span class="badge badge-light-${value ? 'danger' : 'success'} js-status">
                ${value ? trueText : falseText}
            </span>`;
}