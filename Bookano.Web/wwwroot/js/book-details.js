
function onRowAdded() {

    const copiesCountEl = document.querySelector('#CopiesCount');
    const alertEl = document.querySelector('#Copies .js-alert');
    const tableEl = document.querySelector('#Copies .table-responsive');

    if (!copiesCountEl || !alertEl || !tableEl || !datatable) return;

    const count = datatable.rows().count();
    copiesCountEl.textContent = count;

    if (count > 0) {
        alertEl.classList.add('d-none');
        tableEl.classList.remove('d-none');

        requestAnimationFrame(() => {
            datatable.columns.adjust().draw(false);
        });
    }

    animate(copiesCountEl, 'animate__jackInTheBox');
}

function copyISBN(el) {
    const isbn = el.innerText;

    navigator.clipboard.writeText(isbn).then(() => {
        const original = el.innerHTML;
        el.innerHTML = 'Copied ✔';

        setTimeout(() => {
            el.innerHTML = original;
        }, 1000);
    }).catch(() => {
        alert('Copy failed');
    });
}