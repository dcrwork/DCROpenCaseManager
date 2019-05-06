var alreadyDrafted;
var documentId;
var documentText;
var documentTitle;
var documentEventDate;
var documentEventTime;
var timer = $.now()-1000;
var numberOfChanges = 0;
var intervalPause = false;
var isAlreadyDraftWhenOpened = true;

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
        "value": documentLink + ".html",
        "valueType": "string"

    });
    return query;
}

function CreateJournalNoteView() {
    var id = $.urlParam("id");
    var newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + id : ""), "", "width=800,height=600");
    newWindow.alreadyDrafted = false;
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
            var splitTime = documentInfo.EventDate.split("T");
            var regex = /(\d\d:\d\d)/gm;
            var match = regex.exec(splitTime[1]);
            newWindow.documentEventTime = match[1];
            newWindow.documentEventDate = splitTime[0];
        });
}


$(function () {
    if (!isAlreadyDraftWhenOpened) $('.change-journal-note-button').html(translations.Opdate);
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
})

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
        month = value.getMonth() < 10 ? "0" + value.getMonth() : value.getMonth();
    var year = value.getFullYear();

    return day + "/" + month + "/" + year;
}

$(function () {
    $("#datepicker").datepicker();
    $("#datepicker").datepicker("option", "dateFormat", "dd/mm/yy");
    $("#datepicker").datepicker({ maxDate: "+0d" });
    var maxDate = $("#datepicker").datepicker("option", "maxDate");
    $("#datepicker").datepicker("option", "maxDate", "+0d");

    $("#datepicker").datepicker({ dayNames: [translations.Sunday, translations.Monday, translations.Tuesday, translations.Wednesday, translations.Thursday, translations.Friday, translations.Saturday] });
    var dayNames = $("#datepicker").datepicker("option", "dayNames");
    $("#datepicker").datepicker("option", "dayNames", [translations.Sunday, translations.Monday, translations.Tuesday, translations.Wednesday, translations.Thursday, translations.Friday, translations.Saturday]);

    $("#datepicker").datepicker({ dayNamesMin: [translations.Su, translations.Mo, translations.Tu, translations.We, translations.Th, translations.Fr, translations.Sa] });
    var dayNamesMin = $("#datepicker").datepicker("option", "dayNamesMin");
    $("#datepicker").datepicker("option", "dayNamesMin", [translations.Su, translations.Mo, translations.Tu, translations.We, translations.Th, translations.Fr, translations.Sa]);

    $("#datepicker").datepicker({ monthNamesShort: [translations.Jan, translations.Feb, translations.Mar, translations.Apr, translations.Maj, translations.Jun, translations.Jul, translations.Aug, translations.Sep, translations.Oct, translations.Nov, translations.Dec] });
    var monthNamesShort = $("#datepicker").datepicker("option", "dayNamesMin");
    $("#datepicker").datepicker("option", "monthNamesShort", [translations.Jan, translations.Feb, translations.Mar, translations.Apr, translations.Maj, translations.Jun, translations.Jul, translations.Aug, translations.Sep, translations.Oct, translations.Nov, translations.Dec]);

    $("#datepicker").datepicker({ monthNames: [translations.January, translations.February, translations.March, translations.April, translations.May, translations.June, translations.July, translations.August, translations.September, translations.October, translations.November, translations.December] });
    var dayNamesMin = $("#datepicker").datepicker("option", "monthNames");
    $("#datepicker").datepicker("option", "monthNames", [translations.January, translations.February, translations.March, translations.April, translations.May, translations.June, translations.July, translations.August, translations.September, translations.October, translations.November, translations.December]);

    $("#datepicker").datepicker({ gotoCurrent: true });
    var gotoCurrent = $("#datepicker").datepicker("option", "gotoCurrent");
    $("#datepicker").datepicker("option", "gotoCurrent", true);

    $("#datepicker").datepicker({ firstDay: 1 });
    var firstDay = $("#datepicker").datepicker("option", "firstDay");
    $("#datepicker").datepicker("option", "firstDay", 1);

    $("#datepicker").datepicker({ hideIfNoPrevNext: true });
    var hideIfNoPrevNext = $("#datepicker").datepicker("option", "hideIfNoPrevNext");
    $("#datepicker").datepicker("option", "hideIfNoPrevNext", true);

    $("#datepicker").datepicker({ nextText: translations.Next });
    var nextText = $("#datepicker").datepicker("option", "nextText");
    $("#datepicker").datepicker("option", "nextText", translations.Next);

    $("#datepicker").datepicker({ prevText: translations.Previous });
    var prevText = $("#datepicker").datepicker("option", "nextText");
    $("#datepicker").datepicker("option", "prevText", translations.Previous);

    $("#datepicker").val(formatDate(new Date().toString()));
    
    $('#input-journal-title').attr('placeholder', translations.Title);
});


$("#datepicker").attr('readonly', 'readonly');

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
    var instanceId = $.urlParam("id");
    var file = makeTextFile(textContents);
    uploadFile(file, instanceId, fileName, isDraft, closeWindow);

}

function uploadFile(file, instanceId, fileName, isDraft, closeWindow) {
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
    var instanceId = $.urlParam("id");
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

