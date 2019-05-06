/*!
 * Albe-Timeline v4.0.0, https://github.com/Albejr/jquery-albe-timeline
 * ======================================================================
 * Plugin para renderização de 'linha do tempo' a partir de listas de dados em JSON
 *
 * 2017, Albertino Júnior, http://albertino.eti.br
 */
var months;

    $(document).ready(function () {
        var monthMenu = $('<select>').attr('id', 'timeline-month-selector');
        months = ['Januar', 'Februar', 'Marts', 'April', 'Maj', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'December'];
        var year = getYearId();
        updateMonthMenu(year, monthMenu, months);

        updateTimeline(monthMenu);

        // updates the month dropdown menu according to the year selected
        $(document).on('change', '#timeline-menu', function () {
            var year = getYearId();
            updateMonthMenu(year, monthMenu, months);
        });

        // toggles the filter checkboxes when the filter button is clicked
        $(document).on('click', '#filterButton', function () {
            $('.filterTypeWrapper').toggle("slide");
        });
});

function updateMonthMenu(year, monthMenu, months) {
    var defaultOption = $('<option>').attr('value', 12).append('');
    monthMenu.empty();
    monthMenu.append(defaultOption);
    var monthsElements = $('div[id^="y' + year + '"]');

    // adds months to dropdown menu
    monthsElements.each(function (index) {       
        if (index === 0) { return true; }
        var monthSplit = (monthsElements[index].id).split("m");
        var monthIndex = parseInt(monthSplit[1]);
        
        var monthOption = $('<option>').attr('value', monthIndex).append(months[monthIndex]);
        monthMenu.append(monthOption);
    });
}

function getMonthId() {
    var id = $("#timeline-month-selector").children(":selected").attr("value");
    return id;
}

function getYearId() {
    var id = $("#timeline-menu").children(":selected").attr("value");
    return id;
}

// go to the year and month selected 
function goToTimeframe() {
    var monthId = getMonthId();
    var yearId = getYearId();
    var id;

    if (monthId == 12) {
        id = '#y' + yearId;
    } else {
        id = '#a' + yearId + '-' + monthId + '-' + '1';
    }

    $('html, body').animate(
        {
            scrollTop: $(id).offset().top - 50,
        },
        500,
        'linear'
    )
}

// creates the timeline
function updateTimeline(monthMenu) {

    $.fn.albeTimeline = function (json, options) {
        var _this = this;
        _this.html('');
        var settings = $.extend({}, $.fn.albeTimeline.defaults, options);

        var language = ($.fn.albeTimeline.languages.hasOwnProperty(settings.language)) ?
            $.fn.albeTimeline.languages[settings.language] : { // da-DK
                days: [translations.Monday, translations.Tuesday, translations.Wednesday, translations.Thursday, translations.Friday, translations.Saturday, translations.Sunday],
                months: [translations.January, translations.February, translations.March, translations.April, translations.May, translations.June, translations.July, translations.August, translations.September, translations.October, translations.November, translations.December],
                shortMonths: [translations.Jan, translations.Feb, translations.Mar, translations.Apr, translations.Maj, translations.Jun, translations.Jul, translations.Aug, translations.Sep, translations.Oct, translations.Nov, translations.Dec],
                separator: 'den',
                msgEmptyContent: 'Der var ikke noget.',
            };
        months = [translations.January, translations.February, translations.March, translations.April, translations.May, translations.June, translations.July, translations.August, translations.September, translations.October, translations.November, translations.December];

        if (typeof (json) == 'string') {
            json = $.parseJSON(json);
        }

        if ($.isEmptyObject(json)) {
            console.warn(language.msgEmptyContent);
            return;
        }

        json = json.sort(function (a, b) {
            return (settings.sortDesc) ? (Date.parse(b['time']) - Date.parse(a['time'])) : (Date.parse(a['time']) - Date.parse(b['time']));
        });

        var yearMenu = $('<select>').attr('id', 'timeline-menu');

        var findTimeFrameButton = $('<button>').attr('id', 'find-timeframe-button').text(translations.Find);
        findTimeFrameButton.attr('onclick', 'goToTimeframe()');

        var eTimeline = $('<section>').attr('id', 'timeline');

        // appends the data to the timeline
        $.each(json, function (index, element) {

            var timelineType = element.type;

            var year = new Date(element.time).getFullYear();
            var month = new Date(element.time).getMonth();
            var createGroupYear = $(eTimeline).find('div.group' + year);
            var createGroupMonth = $(eTimeline).find('div.group' + year + '-' + month);

            // Create group if it doesnt exist
            if (createGroupYear.length === 0) {
                createGroupYear = $('<div>').attr('id', ('y' + year)).addClass('group' + year).text(year);

                $(eTimeline).append(createGroupYear);

                var anchorYear = $('<a>').attr('href', ('#y' + year)).text(year);
                var yearOption = $('<option>').attr('value', year).append(anchorYear);
                yearMenu.append(yearOption);
            }

            if (createGroupMonth.length === 0) {
                createGroupMonth = $('<div>').attr('id', ('y' + year + '-m' + month)).addClass('group' + year + '-' + month).text(language.months[month]);
                $(eTimeline).append(createGroupMonth);
            }

            if (month !== $('#timeline-month-selector').val()) {
                /****************************************SLOT <article>****************************************/
                var leftWrapper = $('<div>').addClass('leftWrapper');
                var badge = $('<div>').addClass('badge');
                badge.text(formatDateTimeline(element.time));

                var responsible = $('<p>').addClass('timelineResponsible');
                responsible.text(element.responsible || '');

                badge.append(responsible);
                leftWrapper.append(badge);

                var ePanel = $('<div>').addClass('timelinePanel').append(leftWrapper);
                var symbol = $('<span>').addClass('icon ' + timelineType);

                symbol.attr('title', timelineType);
                ePanel.append(symbol);

                if (element.header) {
                    var ePanelHead = $('<div>').addClass('panel-heading');
                    var ePaneltitle = $('<h4>').addClass('panel-title').text(element.header);

                    ePanelHead.append(ePaneltitle);
                    ePanel.append(ePanelHead);
                }

                var ePanelBody = $('<div>').addClass('panel-body');
                $.each(element.body, function (index2, value2) {
                    var e = $('<' + value2.tag + '>');

                    
                    $(value2.attr).each(function () {
                        $.each(this, function (index3, value3) {
                            (index3.toLowerCase() === 'cssclass') ? e.addClass(value3) : e.attr(index3, value3);
                        });
                    });

                    if (value2.content)
                        e.html(value2.content);

                    ePanelBody.append(e);
                });

                ePanel.append(ePanelBody);

                if (element.footer) {
                    var ePanelFooter = $('<div>').addClass('panel-footer').html(element.footer);
                    ePanel.append(ePanelFooter);
                }

                var monthSiblings = createGroupMonth.siblings('article[id^="a' + year + '-' + month + '"]');

                var slot = $('<article id="a' + year + '-' + month + '-' + (monthSiblings.length + 1) + '">').append(ePanel);

                if (monthSiblings.length > 0) {
                    slot.insertAfter(monthSiblings.last());
                }
                else
                    slot.insertAfter(createGroupMonth);
                /****************************************FIM - SLOT <article> ****************************************/
            }          
        });

        var badge = $('<div>').addClass('badge').html('&nbsp;');
        var ePanel = $('<div>').addClass('timelinePanel').append(badge);
        eTimeline.append($('<article>').append(ePanel));
        eTimeline.append($('<div>').addClass('clearfix').css({
            'float': 'none'
        }));

        $.each(eTimeline.find('article'), function (index, value) {
            if (settings.effect && settings.effect != 'none')
                $(this).addClass('animated ' + settings.effect);
        }); 

        var filterTimeWrapper = $('<div>').addClass('filterTimeWrapper').append(yearMenu);
        filterTimeWrapper.append(monthMenu);
        filterTimeWrapper.append(findTimeFrameButton);        

        filterTimeWrapper.appendTo(_this);

        eTimeline.appendTo(_this);
    };

    $.fn.albeTimeline.languages = {};
    $.fn.albeTimeline.defaults = {
        effect: 'fadeInUp',
        formatDate: 'dd/MM-yyyy',
        language: 'da-DK',
        showGroup: true,
        showMenu: true,
        sortDesc: true,
    };

    var fnDateFormat = function (value, format, language) {

        var parts = value.split(/[ :\-\/]/g);
        var newDate = new Date(parts[0], (parts[1] - 1), parts[2], (parts[3] || 0), (parts[4] || 0), (parts[5] || 0));

        if (language.separator) {
            format = format.replace(new RegExp(language.separator, 'g'), '___');
        }

        format = format.replace('ss', padLeft(newDate.getSeconds(), 2));
        format = format.replace('s', newDate.getSeconds());
        format = format.replace('dd', padLeft(newDate.getDate(), 2));
        format = format.replace('d', newDate.getDate());
        format = format.replace('mm', padLeft(newDate.getMinutes(), 2));
        format = format.replace('m', newDate.getMinutes());
        format = format.replace('MMMM', language.months[newDate.getMonth()]);
        format = format.replace('MMM', language.months[newDate.getMonth()].substring(0, 3));
        format = format.replace('MM', padLeft((newDate.getMonth() + 1), 2));
        format = format.replace('DD', language.days[newDate.getDay()]);
        format = format.replace('yyyy', newDate.getFullYear());
        format = format.replace('YYYY', newDate.getFullYear());
        format = format.replace('yy', (newDate.getFullYear() + '').substring(2));
        format = format.replace('YY', (newDate.getFullYear() + '').substring(2));
        format = format.replace('HH', padLeft(newDate.getHours(), 2));
        format = format.replace('H', newDate.getHours());

        if (language.separator) {
            format = format.replace(new RegExp('___', 'g'), language.separator);
        }

        return format;
    };

    var padLeft = function (n, width, z) {
        z = z || '0';
        n = n + '';
        return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
    };
}

