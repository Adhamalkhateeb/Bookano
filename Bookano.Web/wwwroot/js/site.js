let datatable;
let pendingRow = null;

let modal;
let modalEl;


// ============================================================
// GLOBAL FORM HANDLER
// ============================================================



document.addEventListener('submit', handleFormSubmit);

function handleFormSubmit(e) {
    const form = e.target;

    if (form.dataset.submitted === 'true') {
        e.preventDefault();
        return;
    }

    if (window.$ && $(form).data('validator')) {
        if (!$(form).valid()) {
            return;
        }
    }

    lockForm(form);

}

function lockForm(container) {
    const form = container.tagName === 'FORM'
        ? container
        : container.querySelector('form');

    if (!form) return;

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

    form.dataset.submitting = 'false';


    form.querySelectorAll('[type="submit"]').forEach(btn => {
        btn.disabled = false;
        btn.setAttribute('data-kt-indicator', 'off');
    });
}

// ============================================================
// DataTable
// ============================================================

function initTable() {
    const tableEl = document.querySelector('.js-datatable');
    if (!tableEl) return;

    formatDates(tableEl);

    datatable = new DataTable(tableEl, {
        pageLength: 10,
        info: false,
        stateSave: true,
        "columnDefs": [
            {
                "targets": -1,       
                "orderable": false   
            },
        ],

        drawCallback: function () {
            KTMenu.createInstances();
            formatDates(this.api().table().node());
        }
    });

    const search = document.querySelector('[data-kt-filter="search"]');
    if (search) {
        search.addEventListener('input', () => {
            datatable.search(search.value).draw();
        });
    }

    attachExportButtons(tableEl);
}

function attachExportButtons(tableEl) {
    const title = tableEl.dataset.exportTitle ?? '';

    const cols = Array.from(tableEl.querySelectorAll('th'))
        .map((th, i) => ({ th, i }))
        .filter(x => !x.th.classList.contains('js-no-export'))
        .map(x => x.i);

    const exportTypes = ['copyHtml5', 'excelHtml5', 'csvHtml5', 'pdfHtml5'];

    const buttons = exportTypes.map(t => ({
        extend: t,
        title,
        exportOptions: { columns: cols }
    }));

    new DataTable.Buttons(datatable, { buttons }).container()
        .appendTo(document.querySelector('#kt_datatable_example_buttons'));

    document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]')
        .forEach(btn => {
            btn.addEventListener('click', e => {
                e.preventDefault();
                const exportValue = e.currentTarget.dataset.ktExport;
                document.querySelector(`.dt-buttons .buttons-${exportValue}`)?.click();
            });
        });
}



function upsertRow(html, oldRow = null) {
    if (!datatable) return;

    const template = document.createElement('template');
    template.innerHTML = html.trim();

    const newRow = template.content.firstElementChild;
    if (!newRow) return;

    formatDates(newRow);

    if (oldRow) datatable.row(oldRow).remove();

    const row = datatable.row.add(newRow);
    datatable.draw(false);

    const node = row.node();
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

    if (titleEl) titleEl.textContent = btn.dataset.title ?? '';

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

// ============================================================
// Modal callbacks (AJAX)
// ============================================================

function onModalSuccess(rowHtml) {
    showSuccess();
    getModal().hide();
    upsertRow(rowHtml, pendingRow);

    if (typeof onRowAdded === 'function' && !pendingRow) {
        onRowAdded(); 
    }

    pendingRow = null;
    resetFormState(modalEl);
}


function onModalComplete() {
    const form = modalEl?.querySelector('form');
    if (form && form.dataset.submitted === 'true') {
        resetFormState(modalEl);
    }
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
            const updatedDate = new Date(updatedOn);

            if (!isNaN(updatedDate)) {
                const sortValue = updatedDate.toISOString();
                updatedEl.setAttribute('data-utc', sortValue);
                updatedEl.setAttribute('data-order', sortValue);
                updatedEl.textContent = updatedDate.toLocaleString();
            } else {
                updatedEl.textContent = updatedOn;
            }

            if (datatable) {
                datatable.row(row).invalidate();
                datatable.draw(false);
            }
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
// TinyMCE
// ============================================================

function initTinyMCE() {
    if (!document.querySelector('.js-tinymce')) return;

    const options = {
        selector: '.js-tinymce',
        height: 513,
        setup: function (editor) {

            editor.on('input', function () {
                const textarea = document.getElementById(editor.id);
                if (!textarea) return;

                textarea.value = editor.getContent();

                if (window.$ && $(textarea).closest('form').data('validator')) {
                    $(textarea).valid();
                }
            });

        }
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

    instance.on("kt.imageinput.changed", function () {
        removeInput.value = "false";
    });

    instance.on("kt.imageinput.canceled", function () {
        wrapper.style.backgroundImage = `none`;
        el.classList.add('image-input-empty');       
        el.classList.remove('image-input-filled');       
        removeInput.value = "false";
    });

    instance.on("kt.imageinput.removed", function () {
        wrapper.style.backgroundImage = `none`;
        el.classList.remove('image-input-filled');       
        removeInput.value = "true";
    });
}
// ============================================================
// Init
// ============================================================

document.addEventListener('DOMContentLoaded', () => {


    document.querySelectorAll('form[data-submitted="true"]').forEach(f => {
        resetFormState(f.parentElement ?? document.body);
    });

    // ----------------------------------------------------------
    // Modal
    // ----------------------------------------------------------
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

    document.body.addEventListener('click', e => {
        const modalBtn = e.target.closest('.js-render-modal');
        if (modalBtn) openModal(modalBtn);

        const toggleBtn = e.target.closest('.js-toggle-status');
        if (toggleBtn) toggleStatus(toggleBtn);
    });

    // ----------------------------------------------------------
    // Select2
    // ----------------------------------------------------------
    $('.js-select2').select2().on('change', function () {
        $(this).valid();
    });

    // ----------------------------------------------------------
    // Datepicker
    // ----------------------------------------------------------
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate: new Date(),
        showDropdowns: true,
    });

    // ----------------------------------------------------------
    // TinyMCE
    // ----------------------------------------------------------
    KTUtil.onDOMContentLoaded(function () {
        initTinyMCE();

        KTThemeMode.on('kt.thememode.change', function () {
            initTinyMCE();
        });
    });

    
  


    initImageInputSync();



    

});


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


function formatDates(root = document) {
    const scope = root instanceof HTMLElement ? root : document;

    scope.querySelectorAll("[data-utc]").forEach(el => {
        const utc = el.getAttribute("data-utc");
        if (!utc) return;

        const date = new Date(utc);
        if (isNaN(date)) return;

        const sortValue = date.toISOString();

        el.setAttribute("data-order", sortValue); 
        el.textContent = date.toLocaleString();
    });
}