function CreateJournalNoteView() {
    var myWindow = window.open("/JournalNote/Create", "", "width=1200,height=1200");
}

if (window.location.pathname === "/journalnote/create") {
    var header = $("header")[0];
    header.style.display = 'none';
}