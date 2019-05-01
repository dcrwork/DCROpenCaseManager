
$(document).ready(function () {
    var pageUrl = window.location.href;
    console.log(pageUrl);
    if (pageUrl.includes("Instance")) setInstancePageBreadcrumb();
    else setChildPageBreadcrumb();
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

async function getChildName(childId) {
    var query = {
        "type": "SELECT",
        "entity": "Child",
        "resultSet": ["Name"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "Id",
        "operator": "equal",
        "value": childId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);
    var result = await API.service('records', query);
    return JSON.parse(result)
}

async function getInstanceName(instanceId) {
    var query = {
        "type": "SELECT",
        "entity": "Instance",
        "resultSet": ["Title"],
        "filters": new Array(),
        "order": []
    }

    var whereInstanceIdMatchesFilter = {
        "column": "Id",
        "operator": "equal",
        "value": instanceId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereInstanceIdMatchesFilter);
    var result = await API.service('records', query);
    return JSON.parse(result)
}

async function setInstancePageBreadcrumb() {
    var instanceId = App.getParameterByName("id", window.location.href);

    var childIds = await getChildId(instanceId)
    var childId = childIds[0].ChildId;
    var path = "/Child?id=" + childId;
    var childnames = await getChildName(childId);
    var childName = childnames[0].Name;
    $('a#childLink').attr("href", path).text(childName);

    var instanceNames = await getInstanceName(instanceId);
    var instanceName = instanceNames[0].Title;
    $("li.instance").text(instanceName);
}

async function setChildPageBreadcrumb() {
    var childId = App.getParameterByName("id", window.location.href);
    var childnames = await getChildName(childId);
    var childName = childnames[0].Name;
    $('li.child').text(childName);
}
