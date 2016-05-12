$(document).ready(function () {

    // Get #file path.
    function readURL(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $('#image').attr('src', e.target.result);
            }

            reader.readAsDataURL(input.files[0]);
        }
    }

    // Change #file path on upload.
    $("#file").change(function () {
        readURL(this);
    });

    // Change #image height according to width.
    var cw = $("#image").width();
    $("#image").css({
        "height": cw + "px"
    });

    // Change button on dropdown click.
    $("a.dropdown-item").click(function () {
        var option = $(this).html();
        $("#button-change").html(option);
    });

});