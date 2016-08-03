//$(document).on("click", ".form button[type='submit']", function () {
//    $.ajax({
//        type: 'POST',
//        url: siteUrl + 'Search/HandleGoogleSearchRequest',
//        data: {
//            SearchTerms: $("#txtSearchTerms").val(),
//            UrlToFind: $("#txtUrlToFind").val()
//        }
//    });
//});
$(document).ready(function () {

    $("form").validate({
        rules: {
            SearchTerms: {
                required: true,

            },
            UrlToFind: {
                required: true
            },
        },
        messages: {
            SearchTerms: {
                required: "You must enter search terms!"

            },
            UrlToFind: {
                required: "You must enter a url to find!"
            },
        },
        errorClass: "error",
        successClass: "valid"
    });
})