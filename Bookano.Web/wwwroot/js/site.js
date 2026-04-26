let pendingRow = null;
let modal = null;
let modalEl = null;


// ============================================================
// DataTable — accessor & helpers
// ============================================================

function getAppDatatable() {
    return window.appDatatable ?? null;
}

function isServerSideDatatable(dt) {
    return !!dt?.settings?.()[0]?.oFeatures?.bServerSide;
}

function reloadDatatable() {
    const dt = getAppDatatable();
    if (!dt) return;
    dt.ajax?.reload(null, false);
}

// ============================================================
// DataTable (Client-Side)
// ============================================================

function initTable() {
    const tableEl = document.querySelector('.js-datatable');
    if (!tableEl) return;

    formatDates(tableEl);

    const datatable = new DataTable(tableEl, {
        pageLength: 10,
        info: false,
        stateSave: true,
        columnDefs: [{ targets: -1, orderable: false }],
        drawCallback: function () {
            KTMenu?.createInstances();
            formatDates(this.api().table().node());
        },
    });

    window.appDatatable = datatable;

    const search = document.querySelector('[data-kt-filter="search"]');
    search?.addEventListener('input', () => datatable.search(search.value).draw());

    attachExportButtons(tableEl);
}

function attachExportButtons(tableEl) {
    const title = tableEl.dataset.exportTitle ?? '';

    const cols = Array.from(tableEl.querySelectorAll('th'))
        .map((th, i) => ({ th, i }))
        .filter(({ th }) => !th.classList.contains('js-no-export'))
        .map(({ i }) => i);

    const buttons = ['copyHtml5', 'excelHtml5', 'csvHtml5', 'pdfHtml5'].map(t => ({
        extend: t,
        title,
        exportOptions: { columns: cols },
    }));

    new DataTable.Buttons(window.appDatatable, { buttons })
        .container()
        .appendTo(document.querySelector('#kt_datatable_example_buttons'));

    document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]')
        .forEach(btn => {
            btn.addEventListener('click', e => {
                e.preventDefault();
                const val = e.currentTarget.dataset.ktExport;
                document.querySelector(`.dt-buttons .buttons-${val}`)?.click();
            });
        });
}

// ============================================================
// Row upsert 
// ============================================================
function upsertRow(html, oldRow = null) {
    const dt = getAppDatatable();
    if (!dt) return;

    if (isServerSideDatatable(dt)) {
        dt.ajax.reload(null, false);
        return;
    }

    const template = document.createElement('template');
    template.innerHTML = html.trim();
    const newRow = template.content.firstElementChild;
    if (!newRow) return;

    formatDates(newRow);

    if (oldRow) dt.row(oldRow).remove();

    const row = dt.row.add(newRow);
    dt.draw(false);

    const node = row.node();
    animate(node, 'animate__flash');
}



// ============================================================
// GLOBAL FORM HANDLER
// ============================================================


document.addEventListener('submit', function (e) {
    if (e.target.id === 'SignOut') return;
    handleFormSubmit(e);
});

function handleFormSubmit(e) {
    const form = e.target;

    if (form.dataset.submitted === 'true') {
        e.preventDefault();
        return;
    }

    if (window.$ && $(form).data('validator') && !$(form).valid()) {
        e.preventDefault();
        return;
    }

    lockForm(form);
}

function lockForm(container) {
    const form = container.tagName === 'FORM'
        ? container
        : container.querySelector('form');

    if (!form) return;

    form.dataset.submitted = 'true';
    form.querySelectorAll('[type="submit"]').forEach(btn => {
        btn.disabled = true;
        btn.setAttribute('data-kt-indicator', 'on');
    });
}

function resetFormState(container) {
    const form = container.tagName === 'FORM'
        ? container
        : container.querySelector('form');

    if (!form) return;

    delete form.dataset.submitted;
    form.querySelectorAll('[type="submit"]').forEach(btn => {
        btn.disabled = false;
        btn.setAttribute('data-kt-indicator', 'off');
    });
}


// ============================================================
// Modal
// ============================================================

function getModal() {
    if (!modalEl) return null;
    if (!modal) modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    return modal;
}

async function openModal(btn) {
    if (!modalEl) return;

    const titleEl = modalEl.querySelector('#ModalLabel');
    const bodyEl = modalEl.querySelector('.modal-body');

    if (titleEl) titleEl.textContent = btn.dataset.title ?? '';

    pendingRow = btn.dataset.mode ? btn.closest('tr') : null;

    try {
        const html = await fetchHtml(btn.dataset.url);
        if (bodyEl) bodyEl.innerHTML = html;

        applySelect2();

        if (window.$?.validator) $.validator.unobtrusive.parse(modalEl);

        getModal()?.show();
    } catch {
        showError();
    }
}

// ============================================================
// Modal AJAX callbacks
// ============================================================

function onModalSuccess(rowHtml) {
    showSuccess();
    getModal()?.hide();

    const dt = getAppDatatable();
    if (dt && isServerSideDatatable(dt)) {
        dt.ajax.reload(null, false);
    } else {
        upsertRow(rowHtml, pendingRow);
        if (typeof onRowAdded === 'function' && !pendingRow) onRowAdded();
    }

    pendingRow = null;
    resetFormState(modalEl);
}

function onModalComplete() {
    resetFormState(modalEl);
}

function showErrorMessage(xhr) {
    resetFormState(modalEl);
    const message = xhr?.responseText?.trim() || 'Something went wrong!';
    showError(message);
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
        const dt = getAppDatatable();

        if (dt && isServerSideDatatable(dt)) {
            // Server-side table Just reload
            dt.ajax.reload(null, false);
            showSuccess();
            return;
        }

        const row = btn.closest('tr');
        const statusEl = row?.querySelector('.js-status');
        const updatedEl = row?.querySelector('.js-updated-on');

        if (!statusEl) {
            showSuccess();
            return;
        }

        const wasDeleted = statusEl.textContent.trim() === 'Deleted';

        statusEl.textContent = wasDeleted ? 'Available' : 'Deleted';
        statusEl.classList.toggle('badge-light-danger', !wasDeleted);
        statusEl.classList.toggle('badge-light-success', wasDeleted);

        if (updatedEl && updatedOn) {
            updatedEl.setAttribute('data-utc', updatedOn);
            updatedEl.setAttribute('data-order', updatedOn);
            formatDates(row);
        }

        if (dt && row) {
            dt.row(row).invalidate().draw(false);
        }

        animate(row, 'animate__flash');
        showSuccess();

    } catch {
        showError();
    } finally {
        btn.disabled = false;
    }
}

// ============================================================
// TinyMCE
// ============================================================

function initTinyMCE() {
    if (!document.querySelector('.js-tinymce')) return;

    const options = {
        selector: '.js-tinymce',
        height: 513,
        setup(editor) {
            editor.on('input', () => {
                const textarea = document.getElementById(editor.id);
                if (!textarea) return;
                textarea.value = editor.getContent();
                if (window.$ && $(textarea).closest('form').data('validator')) {
                    $(textarea).valid();
                }
            });
        },
    };

    if (KTThemeMode.getMode() === 'dark') {
        options.skin = 'oxide-dark';
        options.content_css = 'dark';
    }

    tinymce.remove();
    tinymce.init(options);
}


// ============================================================
// Metronic Image Input — RemoveImage
// ============================================================

function initImageInputSync(container = document) {

    const el = container.querySelector('#book_image_input');
    if (!el) return;

    const instance = KTImageInput.getInstance(el) || new KTImageInput(el);
    const wrapper = el.querySelector('.image-input-wrapper');
    const removeInput = container.querySelector('[name="RemoveImage"]');

    if (!wrapper || !removeInput) return;

    instance.on('kt.imageinput.changed', () => { removeInput.value = 'false'; });

    instance.on('kt.imageinput.canceled', () => {
        wrapper.style.backgroundImage = 'none';
        el.classList.add('image-input-empty');
        el.classList.remove('image-input-filled');
        removeInput.value = 'false';
    });

    instance.on('kt.imageinput.removed', () => {
        wrapper.style.backgroundImage = 'none';
        el.classList.remove('image-input-filled');
        removeInput.value = 'true';
    });
}


// ============================================================
// Select2
// ============================================================

function applySelect2() {
    $('.js-select2').select2().on('change', function () { $(this).valid(); });
}



// ============================================================
// Helpers
// ============================================================



const getCsrfToken = () =>
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';


const animate = (el, animation) => {
    if (!el) return;

    el.classList.remove('animate__animated', animation);
    void el.offsetWidth; // reflow to restart animation
    el.classList.add('animate__animated', animation);

    const done = () => {
        el.classList.remove('animate__animated', animation);
        el.removeEventListener('animationend', done);
    };
    el.addEventListener('animationend', done);
};

const showSuccess = (msg = 'Saved successfully!') =>
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: msg,
        customClass: { confirmButton: 'btn btn-primary' },
    });

const showError = (err = 'Something went wrong!') =>
    Swal.fire({
        icon: 'error',
        title: 'Oops.. 😟',
        text: err?.responseText ?? err,
        customClass: { confirmButton: 'btn btn-primary' },
    });


async function fetchHtml(url) {
    const res = await fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
    });

    if (!res.ok) { showError(); throw new Error(`HTTP ${res.status}`); }

    return res.text();
}

async function postForm(url) {
    const body = new URLSearchParams({
        __RequestVerificationToken: getCsrfToken(),
    });

    const res = await fetch(url, { method: 'POST', body });
    if (!res.ok) { showError(); throw new Error(`HTTP ${res.status}`); }

    return res.text();
}


function formatDates(root = document) {
    const scope = root instanceof HTMLElement ? root : document;
    scope.querySelectorAll('[data-utc]').forEach(el => {

        const utc = el.getAttribute('data-utc');
        if (!utc) return;

        const date = new Date(utc);
        if (isNaN(date)) return;

        el.setAttribute('data-order', date.toISOString());
        el.textContent = date.toLocaleString();
    });
}



// ============================================================
// Bootstrap
// ============================================================

document.addEventListener('DOMContentLoaded', () => {

    // Clean up any stale submitted-form state from previous navigation
    document.querySelectorAll('form[data-submitted="true"]').forEach(f => {
        resetFormState(f.parentElement ?? document.body);
    });

    // Modal setup
    modalEl = document.querySelector('#Modal');
    if (modalEl) {
        modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modalEl.addEventListener('hidden.bs.modal', () => {
            pendingRow = null;
            resetFormState(modalEl);
        });
    }

    KTUtil.onDOMContentLoaded(() => {
        initTable();
        formatDates();
    });

    // Delegated click handler — single listener for the whole page
    document.body.addEventListener('click', e => {
        const modalBtn = e.target.closest('.js-render-modal');
        const toggleBtn = e.target.closest('.js-toggle-status');
        if (modalBtn) openModal(modalBtn);
        if (toggleBtn) toggleStatus(toggleBtn);
    });

    // Select2
    applySelect2();

    // Datepicker
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate: new Date(),
        showDropdowns: true,
    });

    // TinyMCE
    KTUtil.onDOMContentLoaded(() => {
        initTinyMCE();
        KTThemeMode.on('kt.thememode.change', initTinyMCE);
    });

    // Sign-out button
    document.querySelector('.js-signout')
        ?.addEventListener('click', () => document.getElementById('SignOut').submit());

    initImageInputSync();
});
