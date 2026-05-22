document.addEventListener('DOMContentLoaded', function () {

    initDataTable({
        tableId: 'Users',
        ajaxUrl: '/Users/GetUsers',
        processingId: 'UsersProcessing',
        columns: [
            { data: 'id', name: 'Id', className: 'd-none' },
            {
                data: 'fullName', name: 'FullName',
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'Full Name');
                }
            },
            {
                data: 'userName', name: 'UserName',
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'User Name');
                }
            },
            {
                data: 'email', name: 'Email', createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'Email');
                }
            },
            {
                data: 'isDeleted',
                name: 'IsDeleted',
                render: (_data, _type, row) =>
                    renderStatusBadge(row.isDeleted, 'Deleted', 'Available'),
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'Deleted');
                }
            },
            {
                data: 'createdOn',
                name: 'CreatedOnUtc',
                render: renderDateTime,
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'Created On');
                }
            },
            {
                data: 'lastUpdatedOn',
                name: 'LastUpdatedOnUtc',
                className: 'js-updated-on',
                render: renderDateTime,
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', 'Last Updated On');
                }
            },
            {
                name: 'Actions',
                orderable: false,
                className: 'text-center hide-before',
                render: (_data, _type, row) => `
                    <a href="#"
                       class="btn btn-light btn-active-light-primary btn-flex btn-center btn-sm w-100 justify-content-center"
                       data-kt-menu-trigger="click"
                       data-kt-menu-placement="bottom-end">
                        Actions
                        <i class="fa-solid fa-angle-down fs-5 ms-2 fa-sm"></i>
                    </a>
                    <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded
                                menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3"
                         data-kt-menu="true">
                          <div class="menu-item px-3">
                            <a href="javascript:;"
                               data-url="/Users/Edit/${row.id}"
                               data-title="Edit User"
                               data-mode="true"
                               class="menu-link flex-stack px-3 js-render-modal">
                                Edit
                            </a>
                        </div>
                        <div class="menu-item px-3">
                            <a href="javascript:;"
                               class="menu-link px-3 js-toggle-status"
                               data-url="/Users/ToggleStatus/${row.id}">
                                Toggle status
                            </a>
                        </div>
                        <div class="menu-item px-3">
                            <a href="javascript:;"
                               data-url="/Users/ResetPassword/${row.id}"
                               data-title="Reset password"
                               data-mode="true"
                               class="menu-link flex-stack px-3 js-render-modal">
                                Reset password
                            </a>
                        </div>
                        <div class="menu-item px-3">
                            <a href="javascript:;"
                               data-url="/Users/Unlock/${row.id}"
                               data-message="Are you sure that you need to unlock that user?"
                               class="menu-link flex-stack px-3 js-confirm">
                                Unlock
                            </a>
                        </div>
                    </div>
                `,
            },
        ],
    });
});