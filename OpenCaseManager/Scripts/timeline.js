
async function getTimelineData(userId) {
    var childId = userId;
    var query = {
        "type": "SELECT",
        "entity": "Timeline",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "ChildId",
        "operator": "equal",
        "value": childId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);

    var result = await API.service('records', query);
    return JSON.parse( result )
}

$(document).ready(function () {

    $('body').on('click', 'a[name="downloadDoc"]', function () {
        elem = $(this);
        var link = elem.attr('documentlink');
        var win = window.open(window.location.origin + "/file/downloadfile?link=" + link, '_blank');
        win.focus();
    })
})