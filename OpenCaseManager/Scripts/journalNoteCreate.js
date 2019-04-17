function CreateJournalNoteView() {
    var id = $.urlParam("id");
    window.open("/JournalNote/Create" + (id ? "?id=" + id : ""), "", "width=800,height=600");
    //postwindow,dialog=yes,close=no,location=no,status=no,
}


$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
        .exec(window.location.search);

    return (results !== null) ? results[1] || 0 : false;
}