function formatDateTimeline(date) {
    var value = new Date(date);
    return addZero(value.getDate()) + "/" + addZero(value.getMonth() + 1) + "-" + value.getFullYear();
}

function addZero(i) {
    if (i < 10) i = "0" + i;
    return i;
}
function documentType(data) {
    var text = (data.DocumentType === 'ChildDocument') ? '' : 'Indsats: ' + data.InstanceTitle;
    return {
        type: "Dokument",
        time: data.EventDate,
        responsible: data.DocumentResponsible,
        body: [{
            tag: 'a',
            content: '<h3>' + data.DocumentTitle + '</h3>',
            attr: {
                href: '#',
                documentlink: data.Link,
                documentid: data.DocumentId,
                name: "downloadDoc"
            }
        },
        {
            tag: 'p',
            content: text
            }]
    }
}
// TODO -> Her skal der evt. v�re noget content tekst som er passer til den tekst de rer skrevet.
function journalNoteType(data) {
    return {
        type: "Journalnotat",
        time: data.EventDate,
        responsible: data.DocumentResponsible,
        body: [{
            tag: 'a',
            content: '<h3>' + data.DocumentTitle + '</h3>',
            attr: {
                href: '/JournalNote?instanceId=' + data.InstanceId + '&childId=' + data.ChildId + '&documentLink=' + data.Link + '&documentTitle=' + data.DocumentTitle + '&documentAuthor=' + data.DocumentResponsible,
                target: '_blank'
            }
        },
        {
            tag: 'p',
            content: "Tilføjet: " + formatDateTimeline(data.CreationDate),
        }]
    }
}
function activitiesType(data) {
    var eventtype = data.Type;
    if (data.Type === 'Event') eventtype = "Aktivitet";
    return {
        type: eventtype,
        time: data.EventDate,
        responsible: data.EventResponsible,
        body: [{
            tag: 'h3',
            content: data.Title,
            attr: {
                cssclass: 'group-title'
            }
        },
            {
                tag: 'p',
                content: "Indsats: " + data.InstanceTitle
            }]
    }
}

$(document).ready(function () {
    async function getData(activity, journalnote, document) {
        var childId = App.getParameterByName("id", window.location.href);
        var data = await getTimelineData(childId);
        var normData = [];
        $.each(data, function (index, value) {   
            if (journalnote && (value.DocumentType === 'JournalNote' || value.Type === 'JournalNoteBig' || value.Type === 'JournalNoteLittle')) normData.push(journalNoteType(value));
            if (document && (value.DocumentType === 'Instance' || value.DocumentType === 'ChildDocument' || value.DocumentType === 'InstanceDocument')) normData.push(documentType(value));
            if (activity && value.Type === 'Event') normData.push(activitiesType(value)); 
        });

        $('#myTimeline').albeTimeline(normData);
        $('#timeline-menu').trigger('change');
    }

    var activityChecked = true;
    var journalnoteChecked = true;
    var documentChecked = true;

    getData(activityChecked, journalnoteChecked, documentChecked);

    $(document).on('change', '.filterTypeWrapper', function () {
        activityChecked = $('#activityCheckbox').prop('checked');
        journalnoteChecked = $('#journalnoteCheckbox').prop('checked');
        documentChecked = $('#documentCheckbox').prop('checked');

        if (!activityChecked && !journalnoteChecked && !documentChecked) getData(true, true, true);
        else getData(activityChecked, journalnoteChecked, documentChecked);
    });
    
});

