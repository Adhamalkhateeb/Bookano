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
            { data: 'publisher', name: 'Publisher.Name' },
            { data: 'publishingDate', name: 'PublishingDate', render: renderDate },
            { data: 'hall', name: 'Hall' },
            {
                name: 'IsAvailableForRental',
                render: (_data, _type, row) =>
                    `<span class="badge badge-light-${row.isAvailableForRental ? 'success' : 'warning'}">
                        ${row.isAvailableForRental ? 'Available' : 'Not Available'}
                     </span>`,
            },
            {
                name: 'IsDeleted',
                render: (_data, _type, row) =>
                    renderStatusBadge(row.isDeleted, 'Deleted', 'Available'),
            },
            {
                name: 'Actions',
                orderable: false,
                className: 'text-center',
                render: (_data, _type, row) => `
                    <a href="#"
                       class="btn btn-light btn-active-light-primary btn-flex btn-center btn-sm"
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
    });

});