
document.addEventListener("DOMContentLoaded", function () {

    $(".js-date-range").daterangepicker({
        autoUpdateInput: false,
        autoApply: true,
        showDropdowns: true,
        minYear: 2025,
        maxDate: new Date()
    });

    $(".js-date-range").on("apply.daterangepicker", function (ev, picker) {
        $(this).val(
            picker.startDate.format("MM/DD/YYYY") +
            " - " +
            picker.endDate.format("MM/DD/YYYY")
        );
    });

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