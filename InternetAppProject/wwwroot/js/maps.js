var markers = {};
var size;

$.ajax({
    method: "get",
    url: "/Workplaces/FetchJson",
    data: {}
}).done(function (result) {
    size = 0;
    $.each(result, function (key, value) {
        markers[size] = { "name": value.name, "p_lat": value.p_lat, "p_long": value.p_long }
        size++;
    });
}).done(function () {

    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: 15,
        center: new google.maps.LatLng(31.97012071073282, 34.77270090743654),
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    var infowindow = new google.maps.InfoWindow();

    var marker, i;
    console.log("size: " + size);
    for (i = 0; i < size; i++) {
        console.log(markers[i]["p_lat"]);
        console.log(markers[i]["p_long"]);
        marker = new google.maps.Marker({
            position: new google.maps.LatLng(markers[i]["p_lat"], markers[i]["p_long"]),
            map: map
        });

        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                infowindow.setContent(markers[i]["name"]);
                infowindow.open(map, marker);
            }
        })(marker, i));
    }
});