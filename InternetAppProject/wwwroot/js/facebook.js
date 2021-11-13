// load facebook API
window.fbAsyncInit = function () {
    FB.init({
        appId: '163121062684887', // App ID
        channelUrl: '//mynetwork.net/', // Channel File
        status: true, // check login status
        cookie: true, // enable cookies to allow the server to access the session
        xfbml: true,  // parse XFBML
        version: 'v2.5'
    });
};

var k5 = "YWbGHUADpTc1ei2yYqG8vNcfcc";
var k2 = "ZAO0wQ1o4rdLyYOtFWSWZ";
var k6 = "oa6Y0IcM7fIHouVErc8ynm";
var k7 = "qLIefLc3OQCfrZCWpkTyWnwsjdD";
var k1 = "EAACUW5TZCKNcBAANU";
var k8 = "ILBbrp9IRag99g2SwZA8K6cApl";
var k9 = "ibQAdA9h8ZD";
var k3 = "CX5xanZB7BTmh1ppk5vavZC";
var k4 = "7w4VwPFZCDQO0b3fRSJOJnDD";

// send message to facebook to post on the profile
$('.btnPost').click(function () {
    console.log("hello??");
    var MyMessage = $('#postText').val();
    FB.api(
        '/112283481226370/feed',
        'POST',
        {
            "message": MyMessage,
            //"access_token": "EAACUW5TZCKNcBALpcUufZAvYXoh42vDDwQLZBQcN4u7OkN5ZAG8tWXayMHLZCr8ELKo8rZANtvcKCh8XpkaW4to9Aue41vjSopThiB7NSQpS0zrzngroR5OMcpZBlU93hXQhtQKLYul0Q0NXSihMRXuGDdOoVYaPKczaPf4xfAqIHBwLFKHvKq11S0Lszgw6iMZD"
            "access_token": k1 + k2 + k3 + k4 + k5 + k6 + k7 + k8 + k9,
        },
        function (response) {
            $('.error').html("");
            $('.response').html("");
            console.log(response);
            try {
                $('.error').html(response.error.message);
            }
            catch {
                $('.response').html("Your message was posted! Congratulations!");
            }
        }
    );
    // access token:
    //EAACUW5TZCKNcBAAeC7PuRdWSZC6Kmc28VmUwdn2jBxjtUIksslJGjjlOnFwwlVj7W5JIf3GIIJh2Bsm1JDDtkyfEQw9nEtvWpHeps61J0Es4O96OUOW6sUjZAkqZBLWhH2Ennt3nr33B9qcTcm4XTdf7AZBSZBcD8yib3P6FMbsys3e3kXwij6
    //our feed id: 112283481226370
    console.log("after");
});