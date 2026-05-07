$(document).ready(function () {

    const governorateSelect = $('#GovernorateId');
    const areaSelect = $('#AreaId');


    governorateSelect.on('change', function () {
        const governorateId = $(this).val();

        if (!governorateId) return;

        $.ajax({
            type: 'GET',
            url: `/Subscribers/GetAreas?governorateId=${governorateId}`
        }).then(function (data) {

            areaSelect.empty().append(new Option("Select an area...", "", true, false));


            data.forEach(area => {
                const option = new Option(area.text, area.value, false, false);
                areaSelect.append(option);
            });

            areaSelect.trigger('change.select2');
        }).catch(showErrorMessage)

    });
});