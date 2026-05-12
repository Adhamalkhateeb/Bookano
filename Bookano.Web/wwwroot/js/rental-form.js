const selectedCopies = [];

document.addEventListener('DOMContentLoaded', function () {

    const searchBtn = document.querySelector('.js-search');

    searchBtn.addEventListener('click', function (e) {
        e.preventDefault();

        const serialNo = document.getElementById("Value")?.value;
        if (!serialNo) return;

        if (selectedCopies.some(c => c.serial === serialNo)) {
            showError("You already added that book");
            return;
        }

        if (selectedCopies.length >= maxAllowedCopies) {
            showError(`You cannot add more than ${maxAllowedCopies} book(s)`);
            return;
        }

        document.getElementById("SearchForm")?.requestSubmit();
    });
});



document.body.addEventListener("click", function (e) {

    const saveBtn = document.querySelector(".js-save");
    const removeBtn = e.target.closest(".js-remove");
    if (!removeBtn) return;

    const copyElement = removeBtn.closest(".js-copy-container");

    const copyInput = copyElement.querySelector(".js-copy");

    const bookId = copyInput.dataset.bookId;

    const bookIdx = selectedCopies.findIndex(c => c.bookId == bookId);

    if (bookIdx !== -1)
        selectedCopies.splice(bookIdx, 1);

    copyElement.remove();

    const form = document.getElementById("CopiesForm");
    syncSelectedCopies(form);

    if (!selectedCopies.length)
        saveBtn.classList.add("d-none");
});


function onAddCopySuccess(copyHtml) {

    const form = document.getElementById("CopiesForm");
    const saveBtn = document.querySelector(".js-save");


    const temp = document.createElement("div");
    temp.innerHTML = copyHtml;

    const newCopy = temp.querySelector(".js-copy");

    const bookId = newCopy.dataset.bookId;
    const serial = newCopy.value;

    if (selectedCopies.some(c => c.bookId == bookId)) {
        showError("You cannot add more than one copy from the same book");
        return;
    }

    const valueInput = document.getElementById("Value");
    if (valueInput) valueInput.value = "";

    form.prepend(...temp.children);
    syncSelectedCopies(form);

    saveBtn.classList.remove("d-none");
}

function syncSelectedCopies(form) {

    selectedCopies.length = 0;

    form.querySelectorAll(".js-copy").forEach((c,i) => {
        selectedCopies.push({
            serial: c.value,
            bookId: c.dataset.bookId
        });
        c.setAttribute("name", `SelectedCopies[${i}]`);
    });
}

