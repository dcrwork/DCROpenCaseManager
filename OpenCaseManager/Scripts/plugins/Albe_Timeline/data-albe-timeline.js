function formatDateTimeline(date) {
    var value = new Date(date);
    return addZero(value.getDate()) + "/" + addZero(value.getMonth() + 1) + "-" + value.getFullYear();
}

function addZero(i) {
    if (i < 10) i = "0" + i;
    return i;
}
function documentType(data) {
    return {
        type: "Dokument",
        time: data.LastChangedDate,
        responsible: data.CaseNumberIdentifier,
        body: [{
            tag: 'a',
            content: '<h3>' + data.Title + '</h3>',
            attr: {
                href: data.ExternalLink,
                target: '_blank',
                documentlink: data.ExternalLink,
                documentid: data.DocumentId,
                name: "downloadDoc"
            }
        },
        {
            tag: 'p',
            //content: "Indsats: " + data.InstanceTitle
        }]
    }
}
// TODO -> Her skal der evt. v�re noget content tekst som er passer til den tekst de rer skrevet.
function journalNoteType(data) {
    return {
        type: "Journalnotat",
        time: data.LastChangedDate,
        responsible: data.CaseNumberIdentifier,
        body: [{
            tag: 'a',
            content: '<h3>' + data.Title + '</h3>',
            attr: {
                href: data.ExternalLink,
                //                href: '/JournalNote?instanceId=' + data.InstanceId + '&documentLink=' + data.ExternalLink + '&documentTitle=' + data.DocumentTitle + '&documentAuthor=' + data.DocumentResponsible,
                target: '_blank'
            }
        },
        {
            tag: 'p',
            //content: "Tilføjet: " + formatDateTimeline(data.CreationDate),
        }]
    }
}
function activitiesType(data) {
    var eventtype = data.Type;
    if (data.Type === 'Activity') eventtype = "Aktivitet";
    return {
        type: eventtype,
        time: data.LastChangedDate,
        responsible: data.CaseNumberIdentifier,
        body: [{
            tag: 'h3',
            content: data.Title,
            attr: {
                cssclass: 'group-title'
            }
        },
        {
            tag: 'p',
            //content: "Indsats: " + data.InstanceTitle
        }]
    }
}


var hasTimelineData = false;
var timeLineData = [];
$(document).ready(function () {

    $('#pills-timeline-tab').on('click', function () {

        if (!hasTimelineData) {
            var activityChecked = true;
            var journalnoteChecked = true;
            var documentChecked = true;

            getChildTimelineData(activityChecked, journalnoteChecked, documentChecked);
            hasTimelineData = true;
        }
    });

    $(document).on('change', '.filterTypeWrapper', function () {
        activityChecked = $('#activityCheckbox').prop('checked');
        journalnoteChecked = $('#journalnoteCheckbox').prop('checked');
        documentChecked = $('#documentCheckbox').prop('checked');

        //if (!activityChecked && !journalnoteChecked && !documentChecked) getChildTimelineData(true, true, true);
        //else
        getChildTimelineData(activityChecked, journalnoteChecked, documentChecked);
    });

});


async function getChildTimelineData(activity, journalnote, document) {
    var data = [];
    var childId = App.getParameterByName("id", window.location.href);
    if (timeLineData.length === 0) {
        data = await getTimelineData(childId);
        if (data != undefined)
            timeLineData = data;
    }
    else {
        data = timeLineData;
    }

    var prefix = await App.getKeyValue('AcadreFrontEndBaseURL');
    var normData = [];
    if (data != null && data.length > 0) {
        $.each(data, function (index, value) {
            if (journalnote && (value.DocumentType === 'JournalNote' || value.Type === 'JournalNoteBig' || value.Type === 'JournalNoteLittle' || value.Type === 'Memo')) {
                value.ExternalLink = prefix + '/Memo/Details?memoId=' + value.DocumentID;
                normData.push(journalNoteType(value));
            }
            if (document && (value.DocumentType === 'Instance' || value.Type === 'Document')) {
                value.ExternalLink = prefix + '/MainDocument/Details?documentId=' + value.DocumentID;
                normData.push(documentType(value));
            }
            if (activity && value.Type === 'Activity') {
                normData.push(activitiesType(value));
            }
        });

        $('#myTimeline').albeTimeline(normData);
        $('#timeline-menu').trigger('change');
    }
}

