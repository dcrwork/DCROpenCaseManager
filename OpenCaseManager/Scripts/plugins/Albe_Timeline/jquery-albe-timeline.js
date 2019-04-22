/*!
 * Albe-Timeline v4.0.0, https://github.com/Albejr/jquery-albe-timeline
 * ======================================================================
 * Plugin para renderização de 'linha do tempo' a partir de listas de dados em JSON
 *
 * 2017, Albertino Júnior, http://albertino.eti.br
 */

    $(document).ready(function () {
        var monthMenu = $('<select>').attr('id', 'timeline-month-selector');

        var months = ['Januar', 'Februar', 'Marts', 'April', 'Maj', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'December'];
        var defaultOption = $('<option>').attr('value', 12).append('');
        monthMenu.append(defaultOption);
        updateTimeline(monthMenu);

        var year = getYearId();
        updateMonthMenu(year, monthMenu, months);

        $(document).on('change', '#timeline-menu', function () {
            monthMenu.empty();
            monthMenu.append(defaultOption);

            var year = getYearId();
            updateMonthMenu(year, monthMenu, months);
        });
    });

function updateMonthMenu(year, monthMenu, months) {
    var monthsElements = $('div[id^="y' + year + '"]');

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
    console.log("month selected: " + id);
    return id;
}

function getYearId() {
    var id = $("#timeline-menu").children(":selected").attr("value");
    console.log(id);
    return id;
}
function goToTimeframe() {
    var monthId = getMonthId();
    var yearId = getYearId();
    var id;

    if (monthId == 12) {
        id = '#y' + yearId;
    } else {
        id = '#a' + yearId + '-' + monthId + '-' + '1';
    }

    console.log('id:' + id);
    $('html, body').animate(
        {
            scrollTop: $(id).offset().top - 50,
        },
        500,
        'linear'
    )
}


function updateTimeline(monthMenu) {
    $.fn.albeTimeline = function (json, options) {
        var _this = this;
        _this.html('');

        // Mescla opções do usuário com o padrão
        var settings = $.extend({}, $.fn.albeTimeline.defaults, options);

        var language = ($.fn.albeTimeline.languages.hasOwnProperty(settings.language)) ?
            $.fn.albeTimeline.languages[settings.language] : { // da-DK
                days: ['Mandag', 'Tirsdag', 'Onsdag', 'Torsdag', 'Fredag', 'Lørdag', 'Søndag'],
                months: ['Januar', 'Februar', 'Marts', 'April', 'Maj', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'December'],
                shortMonths: ['Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                separator: 'den',
                msgEmptyContent: 'Der var ikke noget.',
            };

        // Se for passado 'string', convert para 'object'.
        if (typeof (json) == 'string') {
            json = $.parseJSON(json);
        }

        // Exibe mensagem padão
        if ($.isEmptyObject(json)) {
            console.warn(language.msgEmptyContent);
            return;
        }

        // Ordena pela data
        json = json.sort(function (a, b) {
            return (settings.sortDesc) ? (Date.parse(b['time']) - Date.parse(a['time'])) : (Date.parse(a['time']) - Date.parse(b['time']));
        });

        var yearMenu = $('<select>').attr('id', 'timeline-menu');

        var findTimeFrameButton = $('<button>').attr('id', 'find-timeframe-button').text('Find');
        findTimeFrameButton.attr('onclick', 'goToTimeframe()');

        
       
       /* $.each(idioma.months, function (index, element) {
            var option = $('<option>').attr('value', index).append(element);
            monthMenu.append(option);
        });*/

        var eTimeline = $('<section>').attr('id', 'timeline');

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
                badge.text(fnDateFormat(element.time, settings.formatDate, language));

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

                    // Elemento HTML
                    var e = $('<' + value2.tag + '>');

                    // Atributos do elemento
                    $(value2.attr).each(function () {
                        $.each(this, function (index3, value3) {
                            // Atributo especial, defido o 'class' ser palavra reservada no javascript.
                            (index3.toLowerCase() === 'cssclass') ? e.addClass(value3) : e.attr(index3, value3);
                        });
                    });

                    // Conteúdo do elemento
                    if (value2.content)
                        e.html(value2.content);

                    ePanelBody.append(e);
                });

                ePanel.append(ePanelBody);

                if (element.footer) {
                    var ePanelFooter = $('<div>').addClass('panel-footer').html(element.footer);
                    ePanel.append(ePanelFooter);
                }

                // Adiciona o item ao respectivo agrupador.
                var yearSiblings = createGroupYear.siblings('article[id^="a' + year + '"]');
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

        // Marcador inicial da Timeline 
        var badge = $('<div>').addClass('badge').html('&nbsp;');
        var ePanel = $('<div>').addClass('timelinePanel').append(badge);
        eTimeline.append($('<article>').append(ePanel));
        eTimeline.append($('<div>').addClass('clearfix').css({
            'float': 'none'
        }));

        $.each(eTimeline.find('article'), function (index, value) {
            // Adiciona classe de animação.
            if (settings.effect && settings.effect != 'none')
                $(this).addClass('animated ' + settings.effect);
        });

        var groupWrapper = $('<div>').addClass('groupWrapper').append(yearMenu);
        groupWrapper.append(monthMenu);
        groupWrapper.append(findTimeFrameButton);
        groupWrapper.appendTo(_this);
        eTimeline.appendTo(_this);
        // return this;
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

    // value = "YYYY-MM-DD" (ISO 8601)
    // format =
    // .:"dd MMMM"
    // .:"dd/MM/yyyy"
    // .:"dd de MMMM de yyyy"
    // .:"DD, dd de MMMM de yyyy"
    // .:"MM/dd/yyyy"
    // .:"DD dd MMMM yyyy HH:mm:ss"


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

