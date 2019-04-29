$(document).ready(function () {
    App.hideDocumentWebpart();

    var childId = App.getParameterByName("id", window.location.href);

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

    API.service('records', query)
        .done(function (response) {
            displayChildName(response);
        })
        .fail(function (e) {
            reject(e);
        });
    
    query = {
        "type": "SELECT",
        "entity": "ChildInstances('$(loggedInUserId)')",
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
            var result = JSON.parse(response);
            showChildInstances(result);
        })
        .fail(function (e) {
            reject(e);
        });
});

function displayChildName(response) {
    var result = JSON.parse(response);
    var childName = (result[0] == undefined) ? 'Intet barn at finde' : result[0].Name;
    $("#childName").html("").append(childName);
    $('head title', window.parent.document).text(childName);
}

function showChildInstances(result) {
    var list = "";
    if (result.length === 0) {
        list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
    } else {
        for (i = 0; i < result.length; i++) {
            list += getChildInstanceHtml(result[i]);
        }
    }
    $("#childInstances").html("").append(list);
    setClosedInstancesToFadedAndMoveDown();
}

function getChildInstanceHtml(item) {
    var open = (item.IsOpen) ? "" : "instanceClosed";
    var instanceLink = "../ChildInstance?id=" + item.Id;
    var numberOfPending = (item.PendingAndEnabled == 0) ? "" : item.PendingAndEnabled;

    var returnHtml = "<tr class='trStyleClass " + open + "'>";
    returnHtml += (item.IsOpen) ? "<td class='statusColumn'>" + getStatus(item.NextDeadline) + "</td>" : "<td class='statusColumn'>Lukket</td>";
    returnHtml += (item.Pending == 'true') ? "<td><img src='../Content/Images/priorityicon.svg' height='16' width='16'/> " + numberOfPending + "</td>" : '<td></td>';
    returnHtml += "<td><a href='" + instanceLink + "'>" + item.Title + "</a></td>";
    returnHtml += "<td>" + item.Process + "</td>";
    returnHtml += "<td>" + item.Name.substr(0, 1).toUpperCase() + item.Name.substr(1) + "</td>";
    if (item.LastUpdated != null) {
        returnHtml += "<td>" + item.LastUpdated.toString().substr(0, 10) + "</td>";
    } else {
        returnHtml += "<td> intet gjort</td>";
    }
    returnHtml += "</tr>";
    return returnHtml;
}

function setClosedInstancesToFadedAndMoveDown() {
    $('.instanceClosed').each(function () {
        $(this).find('td, a').css('color', 'lightgray');
        $(this).remove();
        $('#childInstances').append(this);
    });
}
