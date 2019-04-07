$(document).ready(function () {
    var childId = App.getParameterByName("id", window.location.href);
    var query = {
        "type": "SELECT",
        "entity": "ChildInstances",
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

    API.service('records', query)
        .done(function (response) {
            showChildInstances(response);
        })
        .fail(function (e) {
            reject(e);
        });
});

function showChildInstances(response) {
    var result = JSON.parse(response);
    console.log(result);
    var list = "";
    if (result.length === 0) {
        list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
    } else {
        for (i = 0; i < result.length; i++) {
            list += getChildInstanceHtml(result[i]);
        }
    }
    $("#childInstances").html("").append(list);
}

function getChildInstanceHtml(item) {
    var returnHtml = "<tr class='trStyleClass'>";
    returnHtml += "<td>Grøn</td>";
    returnHtml += "<td>" + item.Title + "</td>";
    returnHtml += "<td>" + item.Process + "</td>";
    returnHtml += "<td>" + item.Name + "</td>";
    returnHtml += "<td> 02/04-2019</td>";

    return returnHtml;
    
}