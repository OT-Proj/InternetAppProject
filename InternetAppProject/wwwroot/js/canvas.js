window.onload = function () {
    var canvas = document.getElementById("myCanvas");
    var context = canvas.getContext("2d");
    context.font = "bold 50px Arial";
    context.textAlign = "center";
    context.textBaseline = "middle";
    context.strokeStyle = "#5488DC";
    context.strokeText("MoodleDrive", canvas.width / 2, canvas.height / 2);
};