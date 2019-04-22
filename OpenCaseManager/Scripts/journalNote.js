var alreadyDrafted;
var documentId;
var documentText;
var documentTitle;
var documentEventDate;
var documentEventTime;

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
            /*{
                "column": "IsLocked",
                "operator": "equal",
                "value": false,
                "valueType": "boolean",
                "logicalOperator": "and"
            },*/
            {
                "column": "Type",
                "operator": "equal",
                "value": "JournalNoteBig",
                "valueType": "string",
                "logicalOperator": "and"
            }
        ],
        "resultSet": ["Id", "Title", "EventDate"],
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
    //postwindow,dialog=yes,close=no,location=no,status=no,
}

function CreateJournalNoteViewWithLink() {
    var id = $.urlParam("instanceId");
    var documentLink = $('#documentLink').val();
    documentText = $(".content").html();
    console.log(documentLink);
    var query = getDocumentByLinkQuery(documentLink);
    API.service('records', query)
        .done(function (response) {
            var documentInfo = $.parseJSON(response)[0];
            console.log(documentInfo);
            documentId = documentInfo.Id;
            var newWindow = window.open("/JournalNote/Create" + (id ? "?id=" + id : "") + (documentId ? "&documentId=" + documentId : ""), "", "width=800,height=600");
            newWindow.documentId = documentId;
            newWindow.alreadyDrafted = true;
            newWindow.documentText = documentText;
            newWindow.documentTitle = documentInfo.Title;
            var splitTime = documentInfo.EventDate.split("T");
            var regex = /(\d\d:\d\d)/gm;
            var match = regex.exec(splitTime[1]);
            newWindow.documentEventTime = match[1];
            newWindow.documentEventDate = splitTime[0];
            
            //console.log(newWindow.documentId);
            //console.log(newWindow.documentText);

        });
    //postwindow,dialog=yes,close=no,location=no,status=no,
}


$(function () {
    $("#input-journal-title").val(documentTitle);
    $(".ui-datepicker").val(documentEventDate);
    $(".timepicker").val(documentEventTime);
    var inputJournalNote = $('#input-journal-note');
    if (inputJournalNote != null) {

        inputJournalNote.trumbowyg();
        inputJournalNote.trumbowyg({
            tagsToRemove: ['Redo']
        });
        inputJournalNote.trumbowyg('html', documentText);
    }
})


function formatDate(date) {
    var value = new Date(date);
    return value.getDate() + "/" + (value.getMonth()+1) + "/" + value.getFullYear();
}

$(function () {
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
});


$("#datepicker").attr('readonly', 'readonly');

/*$.datepicker.setDefaults($.datepicker.regional["da"]);*/

function changedate(inputId, lableId) {
    var value = $('#' + inputId).val();
    var applyTo = $('#' + lableId)[0];
    applyTo.value = value;
    applyTo.textContent = value;
}

$(document).on('click', '.add-journal-note-button', function () {
    var documentName = $('#input-journal-title').val();
    var journalText = "<div>"+$('#input-journal-note').val()+"</div>";

    // $('#dateLabel').textContent
    if (!alreadyDrafted) {
        submitFiles(documentName, journalText);  //It is only for testing right now that it will set it as true, in the final version it should always set it as false, and then the program will set it as true after 24 hours
    }
    else {
        updateFiles(documentName, journalText);
    }
    window.close();
});

function makeTextFile(text) {
    var data = new Blob([text], { type: 'text/html' });
    return data;
};

function submitFiles(fileName, textContents) {
    var instanceId = $.urlParam("id");
    var file = makeTextFile(textContents);
    uploadFile(file, instanceId, fileName);

}

function uploadFile(file, instanceId, fileName) {
    console.log(file, instanceId, fileName)
    var eventDateTime = $(".ui-datepicker").val() + " " + $(".timepicker").val();
    console.log(eventDateTime);
    if (fileName != '') {
        $.ajax({
            url: window.location.origin + "/api/records/AddDocument",
            type: 'POST',
            headers: {
                'filename': fileName + '.html',
                'type': 'JournalNoteBig',
                'instanceId': instanceId,
                'givenFileName': fileName,
                'eventTime': eventDateTime
            },
            data: file,
            async: false,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            success: function (data, textStatus, request) {
                documentId = request.responseText;
                var myRegexp = /(\d+)/gm;
                var match = myRegexp.exec(documentId);
                documentId = match[1];
                //console.log(match[1]);
                //console.log(documentId)
            },

        });
    }
}

function closeJournalNotatWindow() {
    window.close()
}

$(document).ready(function () {
    $('input.timepicker').timepicker({
        timeFormat: 'HH:mm',
        interval: 30,
        minTime: '0000',
        maxTime: '2359',
        startTime: '06',
        dynamic: false,
        dropdown: true,
        scrollbar: true
    });
});

$(document).on('click', '.change-journal-note-button', function (event) {
    event.preventDefault();
    var documentName = $('#input-journal-title').val();
    var journalText = "<div>" + $('#input-journal-note').val() + "</div>";

    // $('#dateLabel').textContent
    if (!alreadyDrafted)
    {
        submitFiles(documentName, journalText);
        alreadyDrafted = true;
    }
    else
    {
        updateFiles(documentName, journalText);
    }

});

function updateFiles(fileName, textContents) {
    var instanceId = $.urlParam("id");
    var file = makeTextFile(textContents);

    console.log(file, instanceId, fileName)
    var eventDateTime = $(".ui-datepicker").val() + " " + $(".timepicker").val();
    console.log(eventDateTime);
    if (fileName != '') {
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
                'eventTime': eventDateTime
            },
            data: file,
            async: false,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,

        });
    }
}


