async function getTimelineData(childId) {
    try {
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
        return JSON.parse(result)
    }
    catch (e) {
        App.showErrorMessage(e.responseJSON.ExceptionMessage);
    }
}