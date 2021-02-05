// *** sortable.js - https://github.com/rern/sortable.js/ ***
/*
usage:
...
<link rel="stylesheet" href="/path/sortable.css">
</head>
<body>
	<div id="divbeforeid"> <!-- optional -->
		(divBeforeTable html)
	</div>
	<table id="tableid">
		<thead><tr><td></td></tr></thead>
		<tbody><tr><td></td></tr></tbody>
	</table>
	<div id="divafterid"> <!-- optional -->
		(divAfterTable html)
	</div>
<script src="/path/jquery.min.js"></script>
<script src="/path/sortable.js"></script>
<script>
...
$('tableid').sortable();             // without options > full page table
// or
$('tableid').sortable( {
	  divBeforeTable:  'divbeforeid' // default: (none) - div before table, enclosed in single div
	, divAfterTable:   'divafterid'  // default: (none) - div after table, enclosed in single div
	, initialSort:     'column#'     // default: (none) - start with 0
	, initialSortDesc: true          // default: false
	, locale:          'code'        // default: 'en'   - locale code
	, negativeSort:    [column#]     // default: (none) - column with negative value
	, rotateTimeout:   ms            // try higher if 'thead2' misaligned
	, shortViewportH:  px            // max height to unfix divBeforeTable, divAfterTable
	, tableArray:      []            // default: (none) - use table data array directly
} );
...

custom css for table:
edit in sortable.css  
*/

(function ($) {

    $.fn.sortable = function (options) {
        //******************************************************************
        var settings = $.extend({   // #### defaults:
            divBeforeTable: ''       // 
            , divAfterTable: ''      // 
            , initialSort: ''        // 
            , initialSortDesc: false //
            , locale: 'en'           // 
            , negativeSort: []       // column with negative value
            , timeout: 400           // try higher if 'thead2' misaligned
            , shortViewportH: 414    // max height to unfix divBeforeTable, divAfterTable
            , tableArray: []        // raw data array to skip extraction
        }, options);

        var $window = $(window);
        var $table = this;
        var $thead = $table.find('thead');
        var $thtr = $thead.find('tr');
        var $thtd = $thtr.children(); // either 'th' or 'td'
        var $tbody = $table.find('tbody');
        var $tbtr = $tbody.find('tr');
        var $tbtd = $tbtr.find('td');

        // use table array directly if provided
        if (settings.tableArray.length) {
            var tableArray = settings.tableArray;
        } else {
            // convert 'tbody' to value-only array [ [i, 'a', 'b', 'c', ...], [i, 'd', 'e', 'f', ...] ]
            tableArray = [];
            $tbtr.each(function (i) {
                var row = [i];
                $(this).find('td').each(function (j) {
                    if (settings.negativeSort.indexOf(j + 1) === -1) { // '+1' - make 1st column = 1, not 0
                        var cell = $(this).text();
                    } else { // get minus value in alphanumeric column
                        cell = $(this).text().replace(/[^0-9\.\-]/g, ''); // get only '0-9', '.' and '-'
                    }
                    row.push($thtd.eq(j).text() == '' ? '' : cell); // blank header not sortable
                });
                tableArray.push(row);
            });
        }

        var tableID = this[0].id;
        var tableParent = '#sortable' + tableID;
        var trH = $tbtr.height();
        $table.wrap('<div id="sortable' + tableID + '" class="tableParent"></div>');
        $table.addClass('sortable');


        // #### add l/r padding 'td' to keep table center
        var $tabletmp = $table.detach(); // avoid many dom traversings
        // change 'th' to 'td' for consistent selection
        //$thtd.prop('tagName') == 'TH' &&
        //    $thtr.html($thtr.html().replace(/th/g, 'td'));

        // add 'tdpad'
        $thtr.add($tbtr)
            .prepend('<th style="display:none;" class="tdpad"></th>')
            .append('<th style="display:none;" class="tdpad"></th>')

        $(tableParent).append($tabletmp);
        // refresh cache after add 'tdpad'
        $thtd = $thtr.find('th');
        $tbtd = $tbtr.find('td');

        // #### add fixed 'thead2'
        var thead2html = '<a></a>';
        $thtd.each(function (i) {
            if (i > 0 && i < ($thtd.length - 1)) {
                thead2html += '<a style="text-align: ' + $(this).css('text-align') + ';">'
                    + $(this).text()
                    + '</a>'
                    ;
            }
        });

        var $thead2 = $('#' + tableID + 'th2');
        var $thead2a = $thead2.find('a');
        // delegate click to 'thead'
        $thead2a.click(function () {
            $thtd.eq($(this).index()).click();
        });

        // #### add empty 'tr' to bottom
        $tbody.append(
            $tbody.find('tr:last')
                .clone()
                .empty()
                .prop('id', 'trlast')
        );

        // #### initial align 'thead2a' and sort column, set height
        setTimeout(function () {
            settings.initialSort &&
                $thtd.eq(settings.initialSort).trigger('click', settings.initialSortDesc);

        }, settings.timeout);

        // #### click 'thead' to sort
        $thtd.click(function (event, initdesc) {
            // Collapses all descriptions
            $table.find('.showMe').remove();
            var i = $(this).index();
            var order = ($(this).hasClass('asc') || initdesc) ? 'desc' : 'asc';
            var type = $(this).attr('type');
            var sorted;
            // sort value-only array, not table tr
            if (type !== 'date' && type != "customSort") {
                sorted = tableArray.sort(function (a, b) {
                    var ab = (order == 'desc') ? [a, b] : [b, a];
                    if (settings.negativeSort.indexOf(i) === -1) {
                        return ab[0][i].localeCompare(ab[1][i], settings.locale, { numeric: true });
                    } else {
                        return ab[0][i] - ab[1][i];
                    }
                });

            }
            else {
                //Sort the table
                i = i + 1;
                sorted = tableArray.sort(function (a, b) {
                    switch (type) {
                        case 'date':
                            var ab = (order == 'desc') ? [a, b] : [b, a];
                            a = new Date(a[i]);
                            b = new Date(b[i]);
                            return order === 'asc' ? a.getTime() - b.getTime() : b.getTime() - a.getTime();
                        case 'customSort':
                            ab = (order == 'desc') ? [a, b] : [b, a];
                            if (settings.negativeSort.indexOf(i) === -1) {
                                return ab[0][i].localeCompare(ab[1][i], settings.locale, { numeric: true });
                            } else {
                                return ab[0][i] - ab[1][i];
                            }
                    }
                });
                i = i - 1;
            }
            // sort 'tbody' in-place by each 'array[ 0 ]'
            $tbodytmp = $tbody.detach();
            $thead2a.add($thtd).add($tbtd)
                .removeClass('asc desc sorted');
            $.each(sorted, function () {
                $tbodytmp.prepend($tbtr.eq($(this)[0]));
            });
            // switch sort icon and highlight sorted column
            $thead2a.eq(i).add(this)
                .addClass(order)
                .add($tbody.find('td:nth-child(' + (i + 1) + ')'))
                .addClass('sorted')
                ;
            $table.append($tbodytmp);
        });

        // #### maintain scroll position on rotate
        // get scroll position
        var positionTop = 0;
        var scrollTimeout;
        (function getScrollTop() {
            $window.scroll(function () {
                // cancel previous 'scroll' within 'settings.timeout'
                clearTimeout(scrollTimeout);
                scrollTimeout = setTimeout(function () {
                    positionTop = $window.scrollTop();
                }, settings.timeout);
            });
        })();

        // reference for scrolling calculation
        var fromShortViewport = ($window.height() <= settings.shortViewportH) ? 1 : 0;
        var positionCurrent = 0;
        // 'orientationchange' always followed by 'resize'
        window.addEventListener('orientationchange', function () {
            $window.off('scroll'); // suppress new 'scroll'
            $thead2.hide(); // suppress 'thead2' unaligned flash
            // maintain scroll (get 'scrollTop()' here works only on ios)
            if ($thead.css('visibility') == 'visible') {
                positionCurrent = positionTop + divBeforeH;
                fromShortViewport = 1;
            } else {
                // omit 'divBeforeTable' if H to V from short viewport
                positionCurrent = positionTop - (fromShortViewport ? divBeforeH : 0);
                fromShortViewport = 0;
            }
            positionTop = positionCurrent; // update to new value
            setTimeout(function () {
                $window.scrollTop(positionCurrent);
            }, settings.timeout);
        });

        // #### realign 'thead2' on rotate / resize
        var resizeTimeout;
        window.addEventListener('resize', function () {
            $window.off('scroll'); // suppress new 'scroll'
            // cancel previous 'resize' within 'timeout'
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function () {
            }, settings.timeout);
        });
    };
}(jQuery));
