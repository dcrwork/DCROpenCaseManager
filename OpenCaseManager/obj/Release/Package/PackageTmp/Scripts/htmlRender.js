function getStatus(deadline) {
    if (deadline != null) {
        var now = new Date().getTime();
        deadline = new Date(deadline).getTime();
        if (now >= deadline) {
            return "<span title='Frist overskredet' class='dot dotRed'></span>";
        } else if (now + 604800000 >= deadline) {
            return "<span title='Frist snart overskredet' class='dot dotYellow'></span>";
        }
    }

    return "<span title='Ingen kommende frister' class='dot dotGreen'></span>";
}


function toggleSearch() {
    $("#search").toggleClass('show');
    $("#search").toggleClass('hide');

    if ($("#search").hasClass('show')) {
        $('#searchCase').focus();
    }
}