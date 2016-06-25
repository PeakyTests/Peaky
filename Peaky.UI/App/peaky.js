var blah = 'hello'

alert(blah);

$(document).ready(function () {
    $.ajax({
        url: "/tests",
        context: document.body,
        success: function () {
            alert("got tests");
        }
    });
});