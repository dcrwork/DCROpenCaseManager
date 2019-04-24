function getStatus(deadline) {
    if (deadline != null) {
        var now = new Date().getTime();
        deadline = new Date(deadline).getTime();
        if (now >= deadline) {
            return "<span class='dot dotRed'></span>";
        } else if (now + 604800000 >= deadline) {
            return "<span class='dot dotYellow'></span>";
        }
    }

    return "<span class='dot dotGreen'></span>";
}

function toggleSearch() {
    $("#search").toggleClass('show');
    $("#search").toggleClass('hide');

    if ($("#search").hasClass('show')) {
        $('main').css('margin-top', -40);
    } else {
        $('main').css('margin-top', 0);
    }
}