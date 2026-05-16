document.addEventListener("DOMContentLoaded", function () {

    formatDates();
    const renewBtn = document.querySelector('.js-renew');
    
    renewBtn.addEventListener("click", async function () {
        const confirmed = await new Promise(resolve => {
            bootbox.confirm({
                message: 'Are you sure you want to renew this subscription?',
                buttons: {
                    confirm: { label: 'Yes', className: 'btn-success' },
                    cancel: { label: 'No', className: 'btn-secondary' }
                },
                callback: resolve
            });
        });

        if (!confirmed) return;

        renewBtn.disabled = true;

        try {

            const addedRow = await postForm(`/Subscribers/RenewSubscription?subscriberKey=${renewBtn.dataset.key}`);
            const tableBody = document.getElementById("subscriptionTable").querySelector("tbody");

            const activeIcon = document.getElementById("ActiveStatusIcon");
            const activeCard = document.getElementById("ActiveCard");
            const badgeStatus = document.getElementById("BadgeStatus");
            const cardStatus = document.getElementById("CardStatus");
            const addRentalBtn = document.getElementById("AddRental")

            activeIcon.classList.remove("fa-triangle-exclamation");
            activeIcon.classList.add("fa-award");

            cardStatus.classList.remove("bg-warning");
            cardStatus.classList.add("bg-success");

            badgeStatus.classList.remove("badge-light-warning");
            badgeStatus.classList.add("badge-light-success");
            badgeStatus.textContent = "Active Subscriber";

            activeCard.textContent = "Active Subscriber";

            addRentalBtn.classList.remove('d-none');


            tableBody.insertAdjacentHTML('afterbegin', addedRow);

            const newRow = tableBody.firstElementChild;
            formatDates(newRow);
            animate(newRow, 'animate__flash');

            showSuccess();
        } catch {
            showError();
        } finally {
            renewBtn.disabled = false;
        }
    });


    document.querySelectorAll(".js-cancel-rental").forEach(btn => {
        btn.addEventListener('click', async function () {
            const confirmed = await new Promise(resolve => {
                bootbox.confirm({
                    message: 'Are you sure you want to cancel this rental?',
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

                const canceledCopies = await postForm(`/Rentals/Cancel/${btn.dataset.id}`);
                const row = btn.closest("tr");

                const totalCopiesEl = document.querySelector(".js-totalCopies");
                totalCopiesEl.textContent = Number(totalCopiesEl.textContent) - canceledCopies;

                row.remove();

                //showSuccess();

                if (document.querySelectorAll("#rentalsTable tbody tr").length === 0) {
                    $("#rentalsTable").fadeOut(function () {
                        $("#Alert").fadeIn();
                    });
                }

            } catch {
                showError();
            } finally {
                renewBtn.disabled = false;
            }
        })
    })
})