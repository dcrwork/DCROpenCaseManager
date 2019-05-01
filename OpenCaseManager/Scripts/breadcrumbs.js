
$(document).ready(function () {
    setBreadcrumb();
});

async function getChildId(instanceId) {
    var query = {
        "type": "SELECT",
        "entity": "InstanceExtension",
        "resultSet": ["ChildId"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "InstanceId",
        "operator": "equal",
        "value": instanceId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);

    var result = await API.service('records', query);
    return JSON.parse(result)
}

async function setBreadcrumb() {
    var instanceId = App.getParameterByName("id", window.location.href);
    console.log(instanceId);
    var childId = await getChildId(instanceId)
    var path = "/Child?id=" + childId[0].ChildId;
    $('a#childLink').attr("href", path);
}
