
$(document).ready(function () {
    var pageUrl = window.location.href.toLowerCase();
    //    if (pageUrl.includes("instance")) setInstancePageBreadcrumb();
    //    else setChildPageBreadcrumb();
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
        "entity": "ChildView",
        "resultSet": ["Name", "Responsible"],
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
    if (childIds[0] != undefined) {
        var childId = childIds[0].ChildId;
        //        var path = "/Child?id=" + childId;
        //        var childnames = await getChildName(childId);
        //        var childName = (childnames[0] == undefined) ? 'Intet barn at finde' : ((childnames[0].Name == null) ? "Intet navn på barn" : childnames[0].Name);
        //        $('a#childLink').attr("href", path).text(childName);

        var instanceNames = await getInstanceName(instanceId);
        var instanceName = instanceNames[0].Title;
        $("li.instance").text(instanceName);
    } else {
        $('a#childLink').attr("href", '/Child?id=').text('Intet barn at finde');
        $("li.instance").text('Ingen gyldig indsats med dette id');
    }
}

async function setChildPageBreadcrumb() {
    var childId = App.getParameterByName("id", window.location.href);
    var childnames = await getChildName(childId);
    var childName = (childnames[0] == undefined) ? 'Intet barn at findeZZ' : ((childnames[0].Name == null) ? "Intet navn på barn" : childnames[0].Name);
    $('li.child').text(childName);
}

async function setChildPageBreadcrumbX(childName, childId) {
    //	var o = document.getElementById('childLink');
    $('li.child').text(childName);
}

async function setInstancePageBreadcrumbX(childName, childId, instanceName) {
    var path = "/Child?id=" + childId;
    $('a#childLink').attr("href", path).text(childName);
    $("li.instance").text(instanceName);
}

