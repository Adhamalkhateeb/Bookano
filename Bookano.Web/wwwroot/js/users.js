document.addEventListener("DOMContentLoaded", function () {

    initDataTable({
        tableId: "Users",
        ajaxUrl: "/Users/GetUsers",
        processingId: "UsersProcessing",
        columns: [
            { data: "id", name: "Id", className: "d-none" },
            { data: "fullName", name: "FullName" },
            { data: "userName", name: "UserName" },
            { data: "email", name: "Email" },

            {
                name: "IsDeleted",
                render: (data, type, row) =>
                    renderStatusBadge(row.isDeleted, "Deleted", "Available")
            },

            {
                data: "createdOn",
                name: "LastUpdatedOnUtc",
                render: renderDateTime
            },

            {
                data: "lastUpdatedOn",
                name: "LastUpdatedOnUtc",
                render: renderDateTime
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
            }
        ]
    });

});