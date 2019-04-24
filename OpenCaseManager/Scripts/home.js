
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
        "order": [{ "column": "ChildName", "descending": false }]
    }

    API.service('records', query)
        .done(function (response) {
            displayChildren(response);
        })
        .fail(function (e) {
            reject(e);
        });

    $('#add-child').on('click', function (e) {
        var childName = $('#child-name').val();
        var responsible = $('#responsible').val();

        if (childName !== '') {
            App.addChild(childName, responsible);
            $('#add-child-modal').modal('toggle');
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
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

function addZero(i) {
    if (i < 10) {
        i = "0" + i;
    }
    return i;
}

function getChildInstanceHtml(item) {
    var childLink = "../Child?id=" + item.ChildId;
    var objDate = new Date(item.NextDeadline);
    var date = objDate.getFullYear() + "-" + addZero((objDate.getMonth() + 1)) + "-" + addZero(objDate.getDay());
    var time = addZero(objDate.getHours()) + ":" + addZero(objDate.getMinutes());
    var returnHtml = "<tr class='trStyleClass'>";
    returnHtml += (item.NextDeadline != null) ? "<td>" + getStatus(item.NextDeadline) + "</td>" : "<td><span class='dot dotGrey' title='Ingen status'></span></td>";
    returnHtml += "<td><a href='" + childLink + "'>" + item.ChildName + "</a></td>";
    returnHtml += "<td>123456-7890</td>";
    returnHtml += "<td>" + item.Responsible + "</td>";
    returnHtml += (item.NextDeadline != null) ? "<td >" + date + " " + time + "</td>" : "<td>Ingen kommende deadlines</td>";

    returnHtml += "</tr>";
    return returnHtml;
}

function getStatus(deadline) {
    if (deadline != null) {
        var now = new Date().getTime();
        deadline = new Date(deadline).getTime();
        if (now >= deadline) {
            return "<span class='dot dotRed'></span>";
        } else if (now + 604800000 >= deadline) {
            return "<span class='dot dotYellow'></span>";
        }
    } 
    
    return "<span class='dot dotGreen'></span>";
}


