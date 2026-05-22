document.addEventListener('DOMContentLoaded', function () {

    initDataTable({
        tableId: 'Books',
        ajaxUrl: '/Books/GetBooks',
        processingId: 'BooksProcessing',
        columns: [
            { data: 'id', name: 'Id', className: 'd-none' },
            {
                name: 'Title',
                render: (_data, _type, row) => `
                    <div class="d-flex align-items-center">
                        <div class="symbol symbol-40px overflow-hidden me-3">
                            <a href="/Books/Details/${row.id}">
                                <div class="symbol-label h-60px">
                                    <img src="${row.imageThumbnailUrl ?? '/images/books/no-image-cover.png'}"
                                         alt="${row.title}"
                                         class="object-fit-contain w-45px h-60px rounded">
                                </div>
                            </a>
                        </div>
                        <div class="d-flex flex-column">
                            <a href="/Books/Details/${row.id}"
                               class="text-primary fs-6 fw-bold my-0">
                                ${row.title}
                            </a>
                            <span class="fs-7">${row.authors}</span>
                            <span class="text-dark fs-8">${row.isbn}</span>
                        </div>
                    </div>
                `,
            },
            { data: 'publisher', name: 'Publisher.Name', orderable: false, },
            { data: 'publishingDate', name: 'PublishingDate', render: renderDate },
            { data: 'hall', name: 'Hall' },
            {
                name: 'IsAvailableForRental',
                orderable: false,
                render: (_data, _type, row) =>
                    `<span class="badge badge-light-${row.isAvailableForRental ? 'success' : 'warning'}">
                        ${row.isAvailableForRental ? 'Available' : 'Not Available'}
                     </span>`,
            },
            {
                name: 'IsDeleted',
                orderable: false,
                render: (_data, _type, row) =>
                    renderStatusBadge(row.isDeleted, 'Deleted', 'Available'),
            },
            {
                name: 'Actions',
                orderable: false,
                className: 'text-center',
                render: (_data, _type, row) => `
                    <a href="#"
                       class="btn btn-light btn-active-light-primary btn-flex btn-center btn-sm js-no-export"
                       data-kt-menu-trigger="click"
                       data-kt-menu-placement="bottom-end">
                        Actions
                        <i class="fa-solid fa-angle-down fs-5 ms-2 fa-sm"></i>
                    </a>
                    <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded
                                menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3"
                         data-kt-menu="true">
                        <div class="menu-item px-3">
                            <a href="/Books/Edit/${row.id}" class="menu-link flex-stack px-3">
                                Edit
                            </a>
                        </div>
                        <div class="menu-item px-3">
                            <a href="javascript:;"
                               class="menu-link px-3 js-toggle-status"
                               data-url="/Books/ToggleStatus/${row.id}">
                                Toggle status
                            </a>
                        </div>
                    </div>
                `,
            },
        ],
        onDraw: function (dt) {
            renderMobileBookCards(dt);
        },
    });

});


function renderMobileBookCards(dt) {
    if (window.innerWidth >= 768) return;

    const tableEl = document.getElementById('Books');

    const wrapper = tableEl.closest('.dataTables_wrapper');

    const tableResponsive = wrapper.querySelector('.table-responsive');
    if (tableResponsive) tableResponsive.style.display = 'none';

    wrapper.querySelector('.mobile-book-grid')?.remove();

    const grid = document.createElement('div');
    grid.className = 'mobile-book-grid';

    dt.rows({ page: 'current' }).data().each(function (row) {
        const card = document.createElement('div');
        card.className = 'mobile-book-card';

        card.innerHTML = `
    <div class="mobile-book-cover-wrap">
        <a href="/Books/Details/${row.id}" class="mobile-book-cover">
            <img src="${row.imageThumbnailUrl ?? '/images/books/no-image-cover.png'}"
                 alt="${row.title}"
                 onerror="this.src='/images/books/no-image-cover.png'">
        </a>
        <div class="mobile-book-menu-trigger"
             data-kt-menu-trigger="click"
             data-kt-menu-placement="top-end">
            <i class="fa-solid fa-ellipsis-vertical fa-xl"></i>
        </div>
        <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded
                    menu-gray-800 menu-state-bg-light-primary fw-semibold w-175px py-3"
             data-kt-menu="true">
            <div class="menu-item px-3">
                <a href="/Books/Details/${row.id}" class="menu-link px-3">
                    <i class="fa-solid fa-eye fs-6 me-2"></i> View
                </a>
            </div>
            <div class="menu-item px-3">
                <a href="/Books/Edit/${row.id}" class="menu-link px-3">
                    <i class="fa-solid fa-pen fs-6 me-2"></i> Edit
                </a>
            </div>
            <div class="menu-item px-3">
                <a href="javascript:;"
                   class="menu-link px-3 js-toggle-status"
                   data-url="/Books/ToggleStatus/${row.id}">
                    <i class="fa-solid fa-toggle-on fs-6 me-2"></i> Toggle status
                </a>
            </div>
        </div>
    </div>

    <div class="mobile-book-info">
        <a href="/Books/Details/${row.id}" class="mobile-book-title">${row.title}</a>
        <span class="mobile-book-author">${row.authors}</span>
        <span class="mobile-book-isbn">${row.isbn ?? ''}</span>
    </div>
`;

        grid.appendChild(card);
    });

    const paginationRow = wrapper.querySelector('.row:last-child');
    wrapper.insertBefore(grid, paginationRow);

    KTMenu?.createInstances();
}