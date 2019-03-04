// on document ready initialization
$(document).ready(function () {
    // hide/show text of instruction web part
    $('.expander').click(function () {
        var panel = $(this).prev('.information');
        var icon = $(this).find('.glyphicon');
        var text = $(this).find('.headline-stamdata');
        if (panel.is(':visible')) {
            panel.slideUp(1000);
            icon.removeClass('glyphicon-minus');
            icon.addClass('glyphicon-plus');
            setTimeout(function () {
                text.text('Se udvidet stamdata');
            }, 1000);
        } else {
            panel.slideDown(1000);
            icon.removeClass('glyphicon-plus');
            icon.addClass('glyphicon-minus');
            setTimeout(function () {
                text.text('Se mindre');
            }, 1000);
        }
    });
});