function formatDateTimeline(date) {
    var value = new Date(date);
    return value.getFullYear() + "-" + value.getMonth() + "-" + value.getDate() ;
}

function documentType(data) {
    return {
        type: "Dokument",
        time: formatDateTimeline(data.EventDate),
        responsible: data.Responsible,
        body: [{
            tag: 'h3',
            content: data.DocumentTitle,
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
// TODO -> Her skal der evt. v�re noget content tekst som er passer til den tekst de rer skrevet.
function journalNoteType(data) {
    return {
        type: "Journalnotat",
        time: formatDateTimeline(data.EventDate),
        responsible: data.Responsible,
        body: [{
            tag: 'h3',
            content: data.DocumentTitle,
            attr: {
                cssclass: 'group-title'
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
        time: formatDateTimeline(data.EventDate),
        responsible: data.Responsible,
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

function normalize(data) {
    if (data.DocumentType === 'Instance') return documentType(data);
    if (data.DocumentType === 'JournalNote') return journalNoteType(data);
    return activitiesType(data);
}

$(document).ready(function () {
    async function getData() {
        var childId = App.getParameterByName("id", window.location.href);
        var data = await getTimelineData(childId);

        var normData = data.map(function (value) {
            return normalize(value);
        });
        
        $('#myTimeline').albeTimeline(normData);
    }

    getData();
});

