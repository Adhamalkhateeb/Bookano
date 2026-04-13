
let updatedRow;
//animate.css
function animate(el, animation) {
    el.classList.remove("animate__animated", animation);
    void el.offsetWidth; // restart animation
    el.classList.add("animate__animated", animation);
}

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
function onModalSuccess(item) {

    showSuccessMessage();
    $("#Modal").modal('hide');

    if (updatedRow) {
        $(updatedRow).replaceWith(item);
        updatedRow = undefined;
    } else {
        $('tbody').append(item);
    }

    KTMenu.init();
    KTMenu.initHandlers();
}

$(document).ready(function () {
    const message = $('#Message').text().trim();
    if (message !== '') {
        showSuccessMessage(message);
    }


    //Bootstrap Modal


    $('body').delegate('.js-render-modal', 'click', function () {
        const btn = $(this);
        const modal = $("#Modal");

        if (btn.data('mode') !== undefined) {
            updatedRow = btn.parents('tr');
        }

        modal.find('#ModalLabel').text(btn.data("title"));

        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(form);
            },
            error: function () {
                showErrorMessage();
            }
        });

        modal.modal('show');

    })
})






