document.addEventListener("DOMContentLoaded", function () {

    var books = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: '/Search/Find?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    $('#Search').typeahead(
    {
        minLength: 4,
        highlight: true
    },
    {
        limit:100,
        name: 'book',
        display: 'title',
        source: books,
        templates: {
            empty: [
                '<div class="m-3 fw-bold">',
                'No Book Were Found!',
                '</div>'
            ].join('\n'),
            suggestion: Handlebars.compile('<div class="py-2"><span>{{title}}</span><br/><span class="fs-xs text-gray-500">by {{authors}}</span></div>'),
        }
        }).on("typeahead:select", function (e,book) {
            window.location.replace(`Search/BookDetails?bookKey=${book.key}`);
        });
})