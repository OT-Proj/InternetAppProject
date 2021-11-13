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
            "access_token": "EAACUW5TZCKNcBAPoItekZBwJswIPnjow6CbPFLgM6ioap1GMd09y8ZCmJWaPVAyjd29InGK1wipIdGveomiFSOsZAv4w971Hen7ogLiZCsrN4TBI7yIZC4GoufigZC5nd3VqZAafIXlcsSLHIhS3tLcwiqb94sI5H1bDfZCilX8qrgmodOJichpflwezpqrC94YQZD"
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