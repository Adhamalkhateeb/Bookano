document.addEventListener("DOMContentLoaded", function () {

    const input = document.querySelector("#PageNumber");
    const form = document.querySelector("#Filters");

    document.querySelectorAll(".page-link").forEach(btn => {

        btn.addEventListener("click", function (e) {
            
            e.preventDefault();

            if (this.parentNode.classList.contains("active")) return;
            input.value = this.dataset.pageNumber;

            form.submit();
        });

    });

});