//Json ObjectS


var nogeandet =    [
	{
		time: '2015-03-29',
		body: [{
			tag: 'h3',
			content: 'Lorem ipsum',
			attr: {
				cssclass: 'group-title'
			}
		},
		{
			tag: 'span',
			content: 'Lorem ipsum 2',
			attr: {
				cssclass: 'group-sub-title'
			}
		},
		{
			tag: 'p',
			content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus class massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui.'
		}]
	},
	{
		time: '2015-04-15',
		body: [{
			tag: 'h3',
			content: 'Lorem ipsum',
			attr: {
				cssclass: 'group-title'
			}
		},
		{
			tag: 'span',
			content: 'Lorem ipsum 2',
			attr: {
				cssclass: 'group-sub-title'
			}
		},
		{
			tag: 'p',
			content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus class massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui.'
		}]
	},
	{
		time: '2016-01-20',
		body: [{
			tag: 'h3',
			content: 'Lorem ipsum',
			attr: {
				cssclass: 'group-title'
			}
		},
		{
			tag: 'span',
			content: 'Lorem ipsum 2',
			attr: {
				cssclass: 'group-sub-title'
			}
		},
		{
			tag: 'p',
			content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus class massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui. See <a href=\"https://github.com/Albejr/jquery-albe-timeline\" target=\"_blank\">more details</a>'
		}]
	},
	{
		time: '2013-01-20',
		body: [{
			tag: 'h3',
			content: 'Lorem ipsum',
			attr: {
				cssclass: 'group-title'
			}
		},
		{
			tag: 'span',
			content: 'Lorem ipsum 2',
			attr: {
				cssclass: 'group-sub-title'
			}
		},
		{
			tag: 'p',
			content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus class massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui.'
		}]
    }];

function formatDateTimeline(date) {
    var value = new Date(date);
    return value.getFullYear() + "-" + value.getMonth() + "-" + value.getDate() ;
}

function normalize(data) {
    return {
        time: formatDateTimeline(data.EventDate),
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
            content: 'Lorem ipsum dolor sit amet, nisl lorem, wisi egestas orci tempus class massa, suscipit eu elit urna in urna, gravida wisi aenean eros massa, cursus quisque leo quisque dui.'
        }]
    }
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

