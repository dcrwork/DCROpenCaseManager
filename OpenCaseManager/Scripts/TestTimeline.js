async function getTimelineData(instanceId) {
    try {
        var query = {
            "type": "SELECT",
            "entity": "Timeline",
            "resultSet": ["*"],
            "filters": new Array(),
            "order": []
        }

        var whereInstanceIdMatchesFilter = {
            "column": "InstanceId",
            "operator": "equal",
            "value": instanceId,
            "valueType": "int",
            "logicalOperator": "and"
        };
        query.filters.push(whereInstanceIdMatchesFilter);

        var result = await API.service('records', query);
        return JSON.parse(result)
    }
    catch (e) {
        App.showErrorMessage(e.responseJSON.ExceptionMessage);
    }
}

$('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    var target = $(e.target).attr("href") // activated tab
    if (target == "#pills-timeline") {
        Timeline();
    }
});