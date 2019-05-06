var alreadyDrafted;
var documentId;
var documentText;
var documentTitle;
var childId;
var instanceId;
var documentEventDate;
var documentEventTime;
var timer = $.now()-1000;
var numberOfChanges = 0;
var intervalPause = false;
var isAlreadyDraftWhenOpened;

$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
        .exec(window.location.search);

    return (results !== null) ? results[1] || 0 : false;
}

function getDocumentByLinkQuery(documentLink) {
    var query = {
        "type": "SELECT",
        "entity": "DocumentTimes",
        "filters": [
            {
                "column": "Type",
                "operator": "equal",
                "value": "JournalNoteBig",
                "valueType": "string",
                "logicalOperator": "and"
            }
        ],
        "resultSet": ["Id", "Title", "EventDate", "IsDraft"],
        "order": [{ "column": "Id", "descending": false }]
    }
    
    query.filters.push({
        "column": "Link",
        "operator": "equal",
        "value": documentLink,
        "valueType": "string"

    });
    return query;
}

function CreateJournalNoteViewInstance() {
    var id = $.urlParam("id");
    var newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + id : ""), "", "width=800,height=600");
    newWindow.alreadyDrafted = false;
    newWindow.isAlreadyDraftWhenOpened = true;
    newWindow.childId = $('#childIdHidden').val();
    newWindow.instanceId = $('#instanceIdHidden').val();
}

function CreateJournalNoteViewChild() {
    var id = $.urlParam("id");
    var newWindow = window.open("/JournalNote/Create" + (id ? "?childId=" + id : ""), "", "width=800,height=600");
    newWindow.alreadyDrafted = false;
    newWindow.isAlreadyDraftWhenOpened = true;
    newWindow.childId = $('#childIdHidden').val();
    newWindow.instanceId = "";
}

function CreateJournalNoteViewWithLink() {
    var id = $.urlParam("instanceId");
    var documentLink = $('#documentLink').val();
    documentText = $(".content").html();
    var query = getDocumentByLinkQuery(documentLink);
    API.service('records', query)
        .done(function (response) {
            var documentInfo = $.parseJSON(response)[0];
            documentId = documentInfo.Id;
            var newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + id : "") + (documentId ? "&documentId=" + documentId : ""), "", "width=800,height=600");
            newWindow.documentId = documentId;
            newWindow.alreadyDrafted = true;
            newWindow.documentText = documentText;
            newWindow.documentTitle = documentInfo.Title;
            newWindow.isAlreadyDraftWhenOpened = documentInfo.IsDraft;
            newWindow.childId = $('#childIdHidden').val();
            newWindow.instanceId = id;

            var splitTime = documentInfo.EventDate.split("T");
            var regex = /(\d\d:\d\d)/gm;
            var match = regex.exec(splitTime[1]);
            newWindow.documentEventTime = match[1];
            newWindow.documentEventDate = splitTime[0];
        });
}


$(function () {
    try {
        if (!isAlreadyDraftWhenOpened) $('.change-journal-note-button').html('Opdater');
        $("#input-journal-title").val(documentTitle);
        $(".ui-datepicker").val(documentEventDate);
        $(".timepicker").val(documentEventTime);

        $('#input-journal-title').on('input', function () {
            numberOfChanges++;
        });

        var inputJournalNote = $('#input-journal-note');
        if (inputJournalNote != null) {

            inputJournalNote.trumbowyg();
            inputJournalNote.trumbowyg({
                tagsToRemove: ['Redo']
            });
            inputJournalNote.trumbowyg('html', documentText);
            inputJournalNote.trumbowyg().on('tbwchange', function () {
                numberOfChanges++;
            });
        }
    }
    catch (err) { }

});

function automaticSaveDraft() {
    if ($.now() - timer > 2000 && numberOfChanges >= 10) {
        saveFile(isAlreadyDraftWhenOpened, false, null);
        new Noty({
            type: 'alert',
            theme: 'mint',
            layout: 'topRight',
            text: translations.DraftSaved,
            timeout: 2000,
            progressBar: false,
            container: '.custom-container2'
        }).show()
        timer = $.now();
        numberOfChanges = 0;
    }
    else if ($.now() - timer > 10000 && numberOfChanges > 0) {
        saveFile();
        new Noty({
            type: 'alert',
            theme: 'mint',
            layout: 'topRight',
            text: translations.DraftSaved,
            timeout: 2000,
            progressBar: false,
            container: '.custom-container2'
        }).show()
        timer = $.now();
        numberOfChanges = 0;
    }
}

function formatDate(_date) {
    var value = new Date(_date);

    var day = value.getDate() < 10 ? '0' + value.getDate() : value.getDate();
    var month = value.getMonth + 1;
        month = (value.getMonth()+1) < 10 ? "0" + (value.getMonth()+1) : (value.getMonth()+1);
    var year = value.getFullYear();

    return day + "/" + month + "/" + year;
}

$(function () {
    try {
        $("#datepicker").datepicker();
        $("#datepicker").datepicker("option", "dateFormat", "dd/mm/yy");
        $("#datepicker").datepicker({ maxDate: "+0d" });
        var maxDate = $("#datepicker").datepicker("option", "maxDate");
        $("#datepicker").datepicker("option", "maxDate", "+0d");

        $("#datepicker").datepicker({ dayNames: ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"] });
        var dayNames = $("#datepicker").datepicker("option", "dayNames");
        $("#datepicker").datepicker("option", "dayNames", ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"]);

        $("#datepicker").datepicker({ dayNamesMin: ["Sø", "Ma", "Ti", "On", "To", "Fr", "Lø"] });
        var dayNamesMin = $("#datepicker").datepicker("option", "dayNamesMin");
        $("#datepicker").datepicker("option", "dayNamesMin", ["Sø", "Ma", "Ti", "On", "To", "Fr", "Lø"]);

        $("#datepicker").datepicker({ monthNamesShort: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"] });
        var monthNamesShort = $("#datepicker").datepicker("option", "dayNamesMin");
        $("#datepicker").datepicker("option", "monthNamesShort", ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"]);

        $("#datepicker").datepicker({ monthNames: ["Januar", "Februar", "Marts", "April", "Maj", "Juni", "Juli", "August", "September", "Oktober", "November", "December"] });
        var dayNamesMin = $("#datepicker").datepicker("option", "monthNames");
        $("#datepicker").datepicker("option", "monthNames", ["Januar", "Februar", "Marts", "April", "Maj", "Juni", "Juli", "August", "September", "Oktober", "November", "December"]);

        $("#datepicker").datepicker({ gotoCurrent: true });
        var gotoCurrent = $("#datepicker").datepicker("option", "gotoCurrent");
        $("#datepicker").datepicker("option", "gotoCurrent", true);

        $("#datepicker").datepicker({ firstDay: 1 });
        var firstDay = $("#datepicker").datepicker("option", "firstDay");
        $("#datepicker").datepicker("option", "firstDay", 1);

        $("#datepicker").datepicker({ hideIfNoPrevNext: true });
        var hideIfNoPrevNext = $("#datepicker").datepicker("option", "hideIfNoPrevNext");
        $("#datepicker").datepicker("option", "hideIfNoPrevNext", true);

        $("#datepicker").datepicker({ nextText: "Næste" });
        var nextText = $("#datepicker").datepicker("option", "nextText");
        $("#datepicker").datepicker("option", "nextText", "Næste");

        $("#datepicker").datepicker({ prevText: "Forrige" });
        var prevText = $("#datepicker").datepicker("option", "nextText");
        $("#datepicker").datepicker("option", "prevText", "Forrige");

        $("#datepicker").val(formatDate(new Date().toString()));
        $("#datepicker").attr('readonly', 'readonly');
        $('#input-journal-title').attr('placeholder', 'Titel');
    }
    catch (err) { }

    
});




/*$.datepicker.setDefaults($.datepicker.regional["da"]);*/

function changedate(inputId, lableId) {
    var value = $('#' + inputId).val();
    var applyTo = $('#' + lableId)[0];
    applyTo.value = value;
    applyTo.textContent = value;
}

$(document).on('click', '.add-journal-note-button', function (event) {
    saveFile(false, true, event);
});

function makeTextFile(text) {
    var data = new Blob([text], { type: 'text/html' });
    return data;
};

function submitFiles(fileName, textContents, isDraft, closeWindow) {
    var file = makeTextFile(textContents);
    uploadFile(file, fileName, isDraft, closeWindow);

}

function uploadFile(file, fileName, isDraft, closeWindow) {
    if (fileName == "") { fileName = "NA" };
    var eventDateTime = $(".ui-datepicker").val() + " " + $(".timepicker").val();
  
    if (fileName != '') {
        intervalPause = true;
        $.ajax({
            url: window.location.origin + "/api/records/AddDocument",
            type: 'POST',
            headers: {
                'filename': fileName + '.html',
                'type': 'JournalNoteBig',
                'instanceId': instanceId,
                'givenFileName': fileName,
                'childId': childId,
                'eventTime': eventDateTime,
                'isDraft': isDraft
            },
            data: file,
            async: true,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            success: function (data, textStatus, request) {
                documentId = request.responseText;
                var myRegexp = /(\d+)/gm;
                var match = myRegexp.exec(documentId);
                documentId = match[1];
            },
            complete: function () {
                intervalPause = false;
                if (closeWindow) {
                    window.close();
                }
            },

        });
    }
}

function closeJournalNotatWindow() {
    window.close()
}

function addMinutsToTime(currentTime, minutsToAdd) {
    var timeValues = currentTime.split(':');
    var h = parseInt(timeValues[0]);
    var m = parseInt(timeValues[1]) + minutsToAdd;

    while (m < 0) {
        h = h - 1
        m = m + 60;
    }
    h = h < 0 ? 23 : h; 

    var h = (h + Math.floor(m / 60)) % 24; 
    var m = m % 60; 

    var m = m < 10 ? '0' + m : m;

    return h + ':' + m;
}

function incrementTime() {
    var currentTime = $('input.timepicker').val()
    var newTime = addMinutsToTime(currentTime, 15);
    $('input.timepicker').val(newTime)
}

function decrementTime() {
    var currentTime = $('input.timepicker').val()
    var newTime = addMinutsToTime(currentTime, -15);
    $('input.timepicker').val(newTime);
}

$(document).ready(function () {
    var now = new Date();
    var latestQuarter = now.getMinutes() - (now.getMinutes() % 15);
    var defaultTime = now.getHours() + ':' + latestQuarter;
    try {
        $('input.timepicker').timepicker({
            timeFormat: 'HH:mm',
            defaultTime: defaultTime,
            interval: 30,
            minTime: '0000',
            maxTime: '2359',
            startTime: '06',
            dynamic: false,
            dropdown: true,
            scrollbar: true,
        });
    }
    catch (err) { }
});

$(document).on('click', '.change-journal-note-button', function (event) {
    saveFile(isAlreadyDraftWhenOpened, false, event);
    var notyText = translations.OpdatedJournalNote;
    if (isAlreadyDraftWhenOpened) notyText = translations.DraftSaved;
    new Noty({
        type: 'success',
        theme: 'mint',
        layout: 'topRight',
        text: notyText,
        timeout: 2000,
        progressBar: false,
        container: '.custom-container'
    }).show()
});

function saveFile(isDraft, closeWindow, event) {
    if (event != null) event.preventDefault();
    var documentName = $('#input-journal-title').val();
    var journalText = "<div>" + $('#input-journal-note').html() + "</div>";

    if (!alreadyDrafted)
    {
        submitFiles(documentName, journalText, isDraft, closeWindow);
        alreadyDrafted = true;
    }
    else {
        updateFiles(documentName, journalText, isDraft, closeWindow);
    }
}

function updateFiles(fileName, textContents, isDraft, closeWindow) {
    if (fileName == "") { fileName = "NA" };
    var file = makeTextFile(textContents);

    var eventDateTime = $(".ui-datepicker").val() + " " + $(".timepicker").val();

    if (fileName != '') {
        intervalPause = true;
        $.ajax({
            url: window.location.origin + "/api/records/UpdateDocument",
            type: 'POST',
            headers: {
                'id': documentId,
                'filename': fileName + '.html',
                'type': 'JournalNoteBig',
                'instanceId': instanceId,
                'givenFileName': fileName,
                'isNewFileAdded': 'True',
                'eventTime': eventDateTime,
                'isDraft': isDraft
            },
            data: file,
            async: true,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            complete: function () {
                intervalPause = false;
                if (closeWindow) {
                    window.close();
                }
            },

        });
    }
}

window.setInterval(function () {
    if (!intervalPause && isAlreadyDraftWhenOpened) {
        automaticSaveDraft();
    }
}, 1000);

