function CreateJournalNoteView(eventId, modified) {
    if (typeof (eventId) == 'undefined') eventId = '';
    var type = "child";
    var id = $.urlParam("id");
    var ChildId = $.urlParam("AdjunktId");
    if (window.location.href.indexOf("AdjunktInstance") > -1) {
        type = "instance";
    }
    var newWindow;
    if (!ChildId) // We're on child page
        newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + id : "") + (eventId == '' ? '' : '&eventId=' + eventId) + (modified == '' ? '' : '&modified=' + modified) + "&type=" + type, "", "width=800,height=600");
    else // We're on instance page
        newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + ChildId : "") + "&instanceId=" + id + (eventId == '' ? '' : '&eventId=' + eventId) + (modified == '' ? '' : '&modified=' + modified) + "&type=" + type, "", "width=800,height=600");
    newWindow.alreadyDrafted = false;
    newWindow.isAlreadyDraftWhenOpened = true;
}


$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
        .exec(window.location.search);

    return (results !== null) ? results[1] || 0 : false;
}