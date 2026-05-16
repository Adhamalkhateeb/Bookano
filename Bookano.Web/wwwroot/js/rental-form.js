const selectedCopies = [];
let currentCopies = [];
let isEditMode = false;

document.addEventListener('DOMContentLoaded', function () {

    if (document.querySelectorAll(".js-copy").length > 0) {
        syncSelectedCopies();
        currentCopies = [...selectedCopies];
        isEditMode = true;
    }

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

    if (e.target.closest(".js-remove")) {
        const removeBtn = e.target.closest(".js-remove");
        const copyContainerEl = removeBtn.closest(".js-copy-container");

        if (isEditMode) {
            removeBtn.classList.replace("btn-light-danger", "btn-light-success");
            removeBtn.classList.replace("js-remove", "js-readd");
            removeBtn.textContent = "Add again";

            copyContainerEl.querySelector("img").style.opacity = 0.5;
            copyContainerEl.querySelector("h4").style.textDecoration = "line-through";

            const copyInput = copyContainerEl.querySelector("input.js-copy");
            copyInput.classList.replace("js-copy", "js-remove");
            copyInput.removeAttribute("name");
        } else {
            copyContainerEl.remove();
        }
    }

    else if (e.target.closest(".js-readd")) {
        const readdBtn = e.target.closest(".js-readd");
        const copyContainerEl = readdBtn.closest(".js-copy-container");

        readdBtn.classList.replace("btn-light-success", "btn-light-danger");
        readdBtn.classList.replace("js-readd", "js-remove");
        readdBtn.textContent = "Remove";

        copyContainerEl.querySelector("img").style.opacity = 1;
        copyContainerEl.querySelector("h4").style.textDecoration = "none";

        const copyInput = copyContainerEl.querySelector("input.js-remove");
        copyInput.classList.replace("js-remove", "js-copy");
    }

    else return; 

    syncSelectedCopies();
    toggleSaveBtn();
});

function toggleSaveBtn() {
    const saveBtn = document.querySelector(".js-save");
    const hasAnyCopies = document.querySelectorAll(".js-copy").length > 0;

    if (!hasAnyCopies) {
        saveBtn.classList.add("d-none");
        return;
    }

    if (!isEditMode) {
        saveBtn.classList.toggle("d-none", !selectedCopies.length);
        return;
    }

    if (JSON.stringify(currentCopies) === JSON.stringify(selectedCopies))
        saveBtn.classList.add("d-none");
    else
        saveBtn.classList.remove("d-none");
}


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
    syncSelectedCopies();
    toggleSaveBtn();
}

function syncSelectedCopies() {

    const form = document.getElementById("CopiesForm");
    selectedCopies.length = 0;

    form.querySelectorAll(".js-copy").forEach((c, i) => {
        selectedCopies.push({
            serial: c.value,
            bookId: c.dataset.bookId
        });
        c.setAttribute("name", `SelectedCopies[${i}]`);
    });
}

