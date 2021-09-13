$(function () {
    $('.btnSearch').click(function () {
        var field1 = $('#desc');
        var field2 = $('#start');
        var field3 = $('#end');
        var loader = $(this).next();
        loader.removeClass('d-none');
        $.post({
            url: "/Images/SearchJson",
            data: { desc: field1.val(), start: field2.val(), end: field3.val()}
        }).done(function (result) {
            var tbody = $('tbody'); // the area to which the content will be loaded
            var template = $('#template').html();
            tbody.html(''); // empty the previous results
            $.each(result, function (key, value) {
                var temp = template;
                $.each(value, function (key, value) {
                        temp = temp.replaceAll('${' + key + '}', value);
                });
                tbody.append(temp);
            });
            loader.addClass('d-none');
        });
    });
});