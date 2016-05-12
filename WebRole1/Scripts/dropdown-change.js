$("a.dropdown-item").click(function () {
    var option = $(this).html();
    $("#button-change").html(option);
});