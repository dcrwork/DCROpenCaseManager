
$(document).ready(function () {
    $('#createInstance').on('click', function (e) {

        var title = $('#instanceTitle').val();
        var graphId = $('#instanceProcesses').find(":selected").val();

        var userRoles = new Array()
        $('select[name="multi-select"]').each(function (index, select) {
            var userRole = { roleId: $(select).attr('userid'), userId: '' }
            $(select.selectedOptions).each(function (index, item) {
                if (item.value !== '0')
                    userRole.userId = item.value;
            })
            if (userRole.userId !== '')
                userRoles.push(userRole)
        })

        if (title !== '' && graphId > 0) {
            App.addInstance(title, graphId, userRoles);
            App.getMyInstances();
            $('#addNewInstance').modal('toggle');
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });

    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
    });

    promise.then(function (result) {
        App.getProcess(true, true);
        App.getMyInstances();
        App.getTasks();
    }, function (err) {
        console.log(err); // Error: "It broke"
    });

    $('#instanceProcesses').on('change', function () {
        var graphId = $('#instanceProcesses').find(":selected").val();
        App.getRoles(graphId);
    });
});

$(document).ready(function () {

    query = {
        "type": "SELECT",
        "entity": "GetMyChildren('$(loggedInUserId)')",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": []
    }

    console.log(query);

    API.services('records', query)
        .done(function (response) {
            displayChildName(response);
        })
        .fail(function (e) {
            reject(e);
        });

});

function displayChildren(response) {
    var result = JSON.parse(response);
    var list = "";
    if (result.length === 0) {
        list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
    } else {
        for (i = 0; i < result.length; i++) {
            list += getChildInstanceHtml(result[i]);
        }
    }
    $("#allMyChildren").html("").append(list);
}

function getChildInstanceHtml(item) {
    var childLink = "../Child?id=" + item.Id;

    var returnHtml = "<tr class='trStyleClass'>";
    returnHtml += "<td><a href='" + childLink + "'>" + item.Name + "</a></td>";
    returnHtml += "<td>123456-7890</td>";
    returnHtml += "<td>" + item.Responsible + "</td>";
    returnHtml += (item.NextDeadline != null) ? "<td class='statusColumn'>" + getStatus(item.NextDeadline) + "</td>" : "<td class='statusColumn'>Ingen kommende deadlines</td>";

    returnHtml += "</tr>";
    return returnHtml;
}