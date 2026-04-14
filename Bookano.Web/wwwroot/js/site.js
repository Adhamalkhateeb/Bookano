
var table;
var datatable;
let updatedRow;
let exportedCols = [];

//Sweet Alert
function showSuccessMessage(message = 'Saved successfully!') {
    return Swal.fire({
        icon: 'success',
        title: 'Success',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function showErrorMessage(message = 'Something went wrong!') {
    return Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

//Modal
function onModalSuccess(row) {

    showSuccessMessage();
    $("#Modal").modal('hide');

    const newRow = $(row);

    const rowData = newRow.find('td').map(function () {
        return $(this).html();
    }).get();

    if (updatedRow) {
        datatable.row(updatedRow).data(rowData).draw(false);
        const rowNode = datatable.row(updatedRow).node();
        updatedRow = undefined;

    } else {
        const addedRow = datatable.row.add(rowData).draw(false);
    }
}


//DataTables
let headers = $('th');
$.each(headers, function (idx) {
    let col = $(this);
    if (!col.hasClass("js-no-export")) exportedCols.push(idx);
})

var KTDatatables = function () {

    var initDatatable = function () {

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'order': [],
            'pageLength': 10,
            drawCallback: function () {
                KTMenu.createInstances();
            }
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatable').data('export-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatable');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();



$(document).ready(function () {




    //SweetAlters
    const message = $('#Message').text().trim();
    if (message !== '') {
        showSuccessMessage(message);
    }



    //DataTables
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });




    //Bootstrap Modal
    $('body').delegate('.js-render-modal', 'click', function () {
        var btn = $(this);
        var modal = $("#Modal");

        modal.find('#ModalLabel').text(btn.data("title"));

        if (btn.data('mode')) {
            updatedRow = btn.parents('tr');
        }


        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
            },
            error: function () {
                showErrorMessage();
            }
        });

        modal.modal('show');

    });

    //Toggle Status 


    $('body').delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);

        bootbox.confirm({
            message: 'Are you sure you want to toggle this item status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (!result) return;

                btn.disabled = true;

                $.post(
                    {
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (lastUpdatedOn) {
                            var row = btn.parents("tr"); 
                            var status = row.find('.js-status');
                            const newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
                            status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            row.find('.js-updated-on').html(lastUpdatedOn);
                            status.addClass('animate__animated animate__flipInX');


                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    })
            }
        })
    })
})







