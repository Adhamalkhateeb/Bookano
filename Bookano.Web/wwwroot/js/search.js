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
        { minLength: 4, highlight: true, classNames: { highlight: 'tt-highlight' } },
        {
            limit: 100,
            name: 'book',
            display: 'title',
            source: books,
            templates: {
                empty: '<div class="p-3 fw-bold text-muted">No books were found!</div>',
                suggestion: Handlebars.compile(`
    <div class="px-4 py-2 border-bottom border-gray-200">
      <div class="text-gray-900">{{title}}</div>
      <div class="text-muted fs-7">by {{authors}}</div>
    </div>
  `)
            }
        }
    ).on("typeahead:select", function (e, book) {
        window.location.replace(`/Search/BookDetails?bookKey=${book.key}`);
    });

});