function formatDateTimeline(date) {
    var value = new Date(date);
    return value.getFullYear() + "-" + value.getMonth() + "-" + value.getDate() ;
}

function documentType(data) {
    return {
        type: data.DocumentType,
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
                tag: 'span',
                content: "Dokument",
                attr: {
                    cssclass: 'group-sub-title'
                }
            },
        {
            tag: 'p',
                content: "Indsats: " + data.Title
            }]
    }
}
// TODO -> Her skal der evt. v�re noget content tekst som er passer til den tekst de rer skrevet.
function journalNoteType(data) {
    return {
        type: data.DocumentType,
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
                tag: 'span',
                content: "Journalnotat",
                attr: {
                    cssclass: 'group-sub-title'
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
        type: data.Type,
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
            tag: 'span',
            content: eventtype,
            attr: {
                cssclass: 'group-sub-title'
            }
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
        var data = await getTimelineData(9);

        console.log(data);

        var normData = data.map(function (value) {
            return normalize(value);
        });

        console.log(normData)
        $('#myTimeline').albeTimeline(normData);
    }

    getData();
});

