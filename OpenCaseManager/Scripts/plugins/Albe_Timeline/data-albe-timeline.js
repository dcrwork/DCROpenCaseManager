function formatDateTimeline(date) {
    var value = new Date(date);
    return value.getFullYear() + "-" + value.getMonth() + "-" + value.getDate() ;
}

function documentType(data) {
    var documenttype = data.DocumentType;
    if (data.DocumentType === 'Instance') documenttype = "Dokument";
    return {
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
                content: documenttype,
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
    var journaltype = data.DocumentType;
    if (data.DocumentType === 'JournalNote') journaltype = "Journalnotat";
    return {
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
                content: journaltype,
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
    return {
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
            content: data.Type,
            attr: {
                cssclass: 'group-sub-title'
            }
        },
        {
            tag: 'p',
            content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui.'
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

