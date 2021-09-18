$(function () {
    $('.selectType').on('change', function () {
        var selected_id = this.value;
        $.post({
            url: "/Drives/PaymentAmountJson",
            data: { id: selected_id}
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
        });
    });
});