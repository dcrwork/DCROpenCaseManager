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
}