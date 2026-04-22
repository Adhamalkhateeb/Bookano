document.addEventListener("DOMContentLoaded", function () {

    const tableEl = document.getElementById("Books");
    const datatable = new DataTable(tableEl, {
        processing: true,
        serverSide: true,
        stateSave: true,
        language: {
            processing: '<span class="loader"></span>'
        },
        ajax: {
            url: "/Books/GetBooks",
            type: "POST"
        },
        drawCallback: function () {
            KTMenu.createInstances();
        },
        order: [[1, 'asc']],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns: [
            { "data": "id", "name": "Id", "className": "d-none", "searchable": true },
            {
                "name": "Title",
                "render": function (data, type, row) {
                    return `
                                        <div class="d-flex align-items-center">
                                            <div class="symbol symbol-50px  overflow-hidden me-3">
                                                <a href="/Books/Details/${row.id}">
                                                    <div class="symbol-label h-75px">
                                                        <img src="${row.imageThumbnailUrl ?? '/images/books/no-image-cover.png'}"
                                                             alt="${row.title}"
                                                             class="object-fit-contain w-60px h-75px rounded">
                                                    </div>
                                                </a>
                                            </div>

                                            <div class="d-flex flex-column">
                                            <a href="/Books/Details/${row.id}"
                                                   class="text-primary fs-3 fw-bolder my-0">
                                                   ${row.title}
                                                </a>
                                                <span class="fs-5">
                                                ${row.authors}
                                                </span>
                                                <span class="text-dark fs-9">${row.isbn}</span>

                                            </div>
                                        </div>
                                    `;
                }
            },
            { "data": "publisher", "name": "Publisher.Name" },
            {
                "name": "PublishingDate",
                "render": function (data, type, row) {

                    const date = new Date(row.publishingDate);

                    return date.toLocaleDateString("en-GB", {
                        day: "2-digit",
                        month: "short",
                        year: "numeric"
                    });
                }
            },
            { "data": "hall", "name": "Hall" },
            //{ "data": "categories", "name": "Categories", "orderable": false },
            {
                "name": "IsAvailableForRental",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${row.isAvailableForRental ? "success" : "warning"}">
                                    ${row.isAvailableForRental ? "Available" : "Not Available"}
                               </span>`
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${row.isDeleted ? "danger" : "success"} js-status">
                                    ${row.isDeleted ? "Deleted" : "Available"}
                               </span>`
                }
            },
            {

                "name": "Actions",
                "orderable": false,
                "className": "text-center",
                "render": function (data, type, row) {
                    return `
                                <a href="#" class="btn btn-light btn-active-light-primary btn-flex btn-center btn-sm" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end">
                                    Actions
                                    <i class="fa-solid fa-angle-down fs-5 ms-2 fa-sm"></i>
                                </a>
                                <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3" data-kt-menu="true" style="">
                                    <!--begin::Menu item-->
                                    <div class="menu-item px-3">
                                        <a href="/Books/Edit/${row.id}"
                                        class="menu-link flex-stack px-3 ">
                                            Edit
                                        </a>
                                    </div>
                                    <!--end::Menu item-->
                                    <!--begin::Menu item-->
                                    <div class="menu-item px-3">
                                        <a href="javascript:;" class="menu-link px-3 js-toggle-status"
                                               data-url="/Books/ToggleStatus/${row.id}">
                                            Toggle status
                                        </a>
                                    </div>
                                    <!--end::Menu item-->
                                </div>`;
                }
            },


        ]
    });

    const search = document.querySelector('[data-kt-filter="search"]');
    if (search) {
        search.addEventListener('input', (e) => {
            datatable.search(e.target.value).draw();
        });
    }
});