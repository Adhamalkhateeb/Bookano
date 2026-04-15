let datatable;
let pendingRow = null;

let modal;
let modalEl;

// ============================================================
// DataTable
// ============================================================
function initTable() {
    const tableEl = document.querySelector('.js-datatable');
    if (!tableEl) return;


    datatable = new DataTable(tableEl, {
        pageLength: 10,
        info: false,
        drawCallback: () => KTMenu.createInstances()
    });


    const search = document.querySelector('[data-kt-filter="search"]');
    if (search) {
        search.addEventListener('input', () => {
            datatable.search(search.value).draw();
        });
    }
}

function upsertRow(html, oldRow = null) {
    if (!datatable) return;

    const template = document.createElement('template');
    template.innerHTML = html.trim();
    const newRow = template.content.firstElementChild;

    if (oldRow) datatable.row(oldRow).remove();

    const node = datatable.row.add(newRow).draw(false).node();
    animate(node, 'animate__flash');
}

// ============================================================
// Modal
// ============================================================
function getModal() {
    if (!modal) {
        modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    }
    return modal;
}

async function openModal(btn) {
    if (!modalEl) return;

    const titleEl = modalEl.querySelector('#ModalLabel');
    const bodyEl = modalEl.querySelector('.modal-body');

    if (titleEl) {
        titleEl.textContent = btn.dataset.title ?? '';
    }

    pendingRow = btn.dataset.mode ? btn.closest('tr') : null;

    try {
        const html = await fetchHtml(btn.dataset.url);

        if (bodyEl) bodyEl.innerHTML = html;
        
        if (window.$?.validator) $.validator.unobtrusive.parse(modalEl);
        
        getModal().show();
    } catch {
        showError();
    }
}

// called from Razor
function onModalBegin() {
    modalEl?.querySelectorAll('[type="submit"]')
        .forEach(b => (b.disabled = true));
}

function onModalSuccess(rowHtml) {
    showSuccess();
    getModal().hide();
    upsertRow(rowHtml, pendingRow);
    pendingRow = null;
}

function onModalComplete() {
    modalEl?.querySelectorAll('[type="submit"]')
        .forEach(b => (b.disabled = false));
}

// ============================================================
// Toggle Status
// ============================================================
async function toggleStatus(btn) {
    const confirmed = await new Promise(resolve => {
        bootbox.confirm({
            message: 'Are you sure you want to toggle this item status?',
            buttons: {
                confirm: { label: 'Yes', className: 'btn-danger' },
                cancel: { label: 'No', className: 'btn-secondary' }
            },
            callback: resolve
        });
    });

    if (!confirmed) return;

    btn.disabled = true;

    try {
        const updatedOn = await postForm(btn.dataset.url);

        const row = btn.closest('tr');
        const statusEl = row.querySelector('.js-status');
        const updatedEl = row.querySelector('.js-updated-on');

        if (!statusEl) return;

        const isDeleted = statusEl.textContent.trim() === 'Deleted';

        statusEl.textContent = isDeleted ? 'Available' : 'Deleted';

        statusEl.classList.toggle('badge-light-danger', !isDeleted);
        statusEl.classList.toggle('badge-light-success', isDeleted);

        if (updatedEl) {
            updatedEl.textContent = updatedOn;
        }

        animate(statusEl, 'animate__flipInX');
        showSuccess();

    } catch {
        showError();
    } finally {
        btn.disabled = false;
    }
}

// ============================================================
// Events
// ============================================================
document.addEventListener('DOMContentLoaded', () => {

    modalEl = document.querySelector('#Modal');

    if (modalEl) {
        modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    }

    KTUtil.onDOMContentLoaded(initTable);

    document.body.addEventListener('click', e => {
        const modalBtn = e.target.closest('.js-render-modal');
        if (modalBtn) openModal(modalBtn);

        const toggleBtn = e.target.closest('.js-toggle-status');
        if (toggleBtn) toggleStatus(toggleBtn);
    });
});



// ============================================================
// Start Helpers
// ============================================================
const getCsrfToken = () =>
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';


const animate = (el, animation) => {
    if (!el) return;

    el.classList.remove('animate__animated', animation);
    void el.offsetWidth;
    el.classList.add('animate__animated', animation);

    const handleAnimationEnd = () => {
        el.classList.remove('animate__animated', animation);
        el.removeEventListener('animationend', handleAnimationEnd);
    };

    el.addEventListener('animationend', handleAnimationEnd);
};


const showSuccess = (msg = 'Saved successfully!') =>
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: msg,
        customClass: { confirmButton: 'btn btn-primary' }
    });

const showError = (msg = 'Something went wrong!') =>
    Swal.fire({
        icon: 'error',
        title: 'Oops.. 😟',
        text: msg,
        customClass: { confirmButton: 'btn btn-primary' }
    });


async function fetchHtml(url) {
    const res = await fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });

    if (!res.ok) {
        showError();
        throw new Error(`HTTP ${res.status}`);
    }

    return res.text();
}



async function postForm(url) {
    const body = new URLSearchParams({
        __RequestVerificationToken: getCsrfToken()
    });

    const res = await fetch(url, {
        method: 'POST',
        body
    });

    if (!res.ok) {
        showError();
        throw new Error(`HTTP ${res.status}`);
    }

    return res.text();
}
