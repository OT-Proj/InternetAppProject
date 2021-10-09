$(function () {
    $('.btnSearch').click(function () {
        var field1 = $('#desc');
        var loader = $(this).next();
        loader.removeClass('d-none');
        $.post({
            url: "/PurchaseEvents/SearchJson",
            data: { id: field1.val()}
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