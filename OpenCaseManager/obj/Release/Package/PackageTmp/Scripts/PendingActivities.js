var childId;

$(document).ready(function () {
    
        childId = (window.location.pathname.toLowerCase() == "/adjunkt") ? App.getParameterByName("id", window.location.href) : null;

        query = {
            "type": "SELECT",
            "entity": "PendingActivities('$(loggedInUserId)')",
            "resultSet": ["*"],
            "filters": new Array(),
            "order": []
        }

        if (childId != null) {
            var whereChildIdMatchesFilter = {
                "column": "ChildId",
                "operator": "equal",
                "value": childId,
                "valueType": "int",
                "logicalOperator": "and"
            };
            query.filters.push(whereChildIdMatchesFilter);
        }

        API.service('records', query)
            .done(function (response) {
                var result = JSON.parse(response);
                fillPendingActivitiesTable(result);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    function fillPendingActivitiesTable(result) {
        var list = "";
        if (result.length === 0) {
            list = "<tr class='trStyleClass'><td></td><td>" + translations.NoRecordFound + " </td></tr>";
        } else {
            for (i = 0; i < result.length; i++) {
                list += getPendingActivityHtml(result[i]);
            }
        }
        $(".pendingActivitiesTableBody").append(list);
    }

    function getPendingActivityHtml(item) {
        var instanceLink = "../AdjunktInstance?id=" + item.InstanceId;

        var returnHtml = "<tr class='trStyleClass'><td width='16px'><img src='../Content/Images/Standard/priorityicon.svg' height='16' width='16'/></td>";
        
        returnHtml += "<td><a title='" + item.InstanceTitle + "' href='" + instanceLink + "'>" + item.EventTitle + "</a></td>";
        return returnHtml;
    }