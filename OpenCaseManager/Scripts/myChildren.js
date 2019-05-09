$(document).ready(function () {

    query = {
        "type": "SELECT",
        "entity": "GetDataForAllMyChildren('$(loggedInUserId)')",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": [{ "column": "ChildName", "descending": false }]
    }

    API.service('records', query)
        .done(function (response) {
            displayChildren(response);
            $('#child-name').attr('placeholder', translations.NameOfChild);
            $('#case-number').attr('placeholder', translations.CaseNumber);
            $('.dotGrey').attr('title', translations.NoInstances);
        })
        .fail(function (e) {
            reject(e);
        });

    $('#add-child').on('click', function (e) {
        var childName = $('#child-name').val();
        var caseNumber = $('#case-number').val();
        var responsible = $('#responsible').val();

        if (childName !== '') {
            App.addChild(childName, caseNumber, responsible);
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
    var date = objDate.getFullYear() + "-" + addZero((objDate.getMonth() + 1)) + "-" + addZero(objDate.getDate());
    var time = addZero(objDate.getHours()) + ":" + addZero(objDate.getMinutes());
    var returnHtml = "<tr class='trStyleClass'>";
    returnHtml += (item.NextDeadline != null || item.SumOfEvents > 0) ? "<td>" + getStatus(item) + "</td>" : "<td><span class='dot dotGrey'></span></td>";
    returnHtml += "<td>" + item.SumOfEvents + "</td>";
    returnHtml += "<td><a href='" + childLink + "'>" + item.ChildName + "</a></td>";
    returnHtml += "<td>123456-7890</td>";
    returnHtml += "<td>" + item.Responsible + "</td>";
    returnHtml += (item.NextDeadline != null) ? "<td >" + date + " " + time + "</td>" : "<td>" + translations.NoDeadline + "</td>";

    returnHtml += "</tr>";
    return returnHtml;
}

function getStatus(item) {
    var deadline = item.NextDeadline;
    var sumOfEvents = item.SumOfEvents;
    if (deadline != null) {
        var now = new Date().getTime();
        deadline = new Date(deadline).getTime();
        if (now >= deadline) {
            return "<span class='dot dotRed'></span>";
        } else if (now + 604800000 >= deadline) {
            return "<span class='dot dotYellow'></span>";
        }

        if (deadline == null && sumOfEvents > 0) {
            return "<span class='dot dotGreen'></span>";
        }
    } 
    
    return "<span class='dot dotGreen'></span>";
}


