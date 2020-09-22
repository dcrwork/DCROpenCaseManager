$(document).ready(function () {
    App.hideDocumentWebpart();

    var childId = App.getParameterByName("id", window.location.href);

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

    query = {
        "JournalCaseId": childId
    }
    API.service('services/GetChildCases', query)
        .done(function (response) {
            var result = JSON.parse(response);
            showChildInstancesX(result);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
        });
});

function showChildInstancesX(AcadreResult) {
    var childId = App.getParameterByName("id", window.location.href);
    var query = {
        "type": "SELECT",
        "entity": "ChildInstances('$(loggedInUserId)')",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": [{ "column": "Pending", "descending": true }, { "column": "PendingAndEnabled", "descending": true }]

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
            showChildInstances(AcadreResult, response);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
        });
}

function displayChildName(response) {
    var result = JSON.parse(response);
    var childName = (result[0] == undefined) ? 'Intet barn at finde' : ((result[0].Name == null) ? "Intet navn på barn" : result[0].Name);
    $("#childName").html("").append(childName);
    $('head title', window.parent.document).text(childName);
}

function displayChildNameX(childName) {
    $("#childName").html("").append(childName);
    $('head title', window.parent.document).text(childName);
    $('#itemResponsible').attr('itemTitle', childName);
}

function setChildResponsible(childInfo, childId) {
    $('#childResponsibleName').text(childInfo.CaseManagerName);
    $('#itemResponsible').attr('itemResponsible', childInfo.CaseManagerName);
    $('#itemResponsible').attr('oldResponsible', childInfo.CaseManagerInitials);
    $('#itemResponsible').attr('itemChildId', childId);
    $('#itemResponsible').attr('changeResponsibleFor', 'child');
}

function showChildInstances(result, OCMresponse) {
    var OCMresult = JSON.parse(OCMresponse);
    var list = "";
    if (result.length === 0 && OCMresult.length === 0) {
        list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
    } else {
        for (i = 0; i < OCMresult.length; i++) {
            list += getChildInstanceHtml(OCMresult[i], 0, null);
        }
        for (i = 0; i < result.length; i++) {
            list += getChildInstanceHtml(result[i], 1, OCMresult);
        }
    }
    $("#childInstances").html("").append(list);
    setClosedInstancesToFadedAndMoveDown();

    $('#tableChildInstances').sortable({
        // DIV selector before table
        divBeforeTable: '',
        // DIV selector after table
        divAfterTable: '',
        // initial sortable column
        initialSort: '',
        // ascending or descending order
        initialSortDesc: false,
        // language
        locale: locale,
        // use table array
        tableArray: []
    });
}

function getChildInstanceHtml(item, iType, OCMresult) {
    // iType: 0:OCM, 1:Acadre
    // Ignore Acadre cases already in OCM list
    if (iType == 1) {
        for (var j = 0; j < OCMresult.length; j++) {
            if (item.CaseID == OCMresult[j].InternalCaseId)
                return "";
        }
    }

    var open;
    var instanceLink;
    var numberOfPending;
    var title;
    if (iType == 0) {
        open = item.IsOpen ? "" : "instanceClosed";
        instanceLink = "../ChildInstance?id=" + item.Id + "&ChildId=" + item.ChildId;
        numberOfPending = (item.PendingAndEnabled == 0) ? "" : item.PendingAndEnabled;
        title = item.Title;
    }
    else {
        open = (!item.IsClosed) ? "" : "instanceClosed";
        instanceLink = "";
        numberOfPending = (item.PendingAndEnabled == 0) ? "" : item.PendingAndEnabled;
        title = item.CaseContent;
        if (title.indexOf('Løbende journal') == 0)
            return "";
    }
    console.log(item);
    var returnHtml = "<tr class='trStyleClass " + open + "'>";
    returnHtml += (open == "") ? "<td class='statusColumn'>" + getStatus(item.NextDeadline) + "</td>" : "<td class='statusColumn'>Lukket</td>";
    var statusClass = $(getStatus(item.NextDeadline)).attr('class');
    switch (statusClass) {
        case 'dot dotRed':
            returnHtml += (open == "") ? "<td style='display:none'>0</td>" : "<td style='display:none'>4</td>";
            break;
        case 'dot dotYellow':
            returnHtml += (open == "") ? "<td style='display:none'>1</td>" : "<td style='display:none'>4</td>";
            break;
        case 'dot dotGreen':
            returnHtml += (open == "") ? "<td style='display:none'>2</td>" : "<td style='display:none'>4</td>";
            break;
        case 'dot dotGrey':
            returnHtml += (open == "") ? "<td style='display:none'>3</td>" : "<td style='display:none'>4</td>";
            break;
        default:
            returnHtml += (open == "") ? "<td style='display:none'>4</td>" : "<td style='display:none'>4</td>";
            break;
    }
    returnHtml += (item.Pending == 'true') ? "<td><img src='../Content/Images/Standard/priorityicon.svg' height='16' width='16'/> " + numberOfPending + "</td>" : '<td></td>';
    returnHtml += (item.Pending == 'true') ? "<td style='display:none'> " + numberOfPending + "</td>" : '<td style="display:none;">0</td>';
    if (instanceLink == "")
        returnHtml += "<td>" + title + "</td>";
    else
        returnHtml += "<td><a href='" + instanceLink + "'>" + title + "</a></td>";
    if (iType == 0)
        returnHtml += "<td>" + item.Process + "</td>";
    else
        returnHtml += "<td><a href='#' onclick='LinkCaseToInstance(" + item.CaseID + ",\"" + item.CaseNumberIdentifier + "\",\"" + title.replace(/\'/g, '') + "\")'>" + "Knyt til proces" + "</a></td>";
    if (item.CaseManagerName == null)
        returnHtml += "<td>" + "NULL" + "</td>";
    else if (iType == 0)
        returnHtml += '<td><a href="#" class="linkStyling responsibleSelectOptions" itemResponsible="' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '" itemTitle="' + title + '" itemInstanceId="' + item.Id + '" changeResponsibleFor="instance">' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '</a></td>';
    else
        returnHtml += '<td>' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '</td>';

    if (item.LastUpdated != null) {
        var sDate = item.LastUpdated.toString().substr(0, 10);
        sDate = sDate.substr(8, 2) + '/' + sDate.substr(5, 2) + '-' + sDate.substr(0, 4);
        returnHtml += "<td>" + sDate + "</td>";
        returnHtml += "<td style='display:none;'>" + item.LastUpdated + "</td>";
    } else {
        returnHtml += "<td> intet gjort</td>";
        returnHtml += "<td style='display:none;'>" + new Date('1002-01-01') + "</td>";
    }
    returnHtml += "</tr>";
    return returnHtml;
}

function LinkCaseToInstance(CaseId, CaseNoForeign, title) {
    $('#processTitle').text('Tilknyt sag til process');
    $('#create-process').text('Tilknyt');
    $('#Case-Number-Identifier').text(CaseNoForeign);
    $('#CaseId').text(CaseId);
    $('#process-title').val(title);
    $('#create-process-modal').modal('show');
}

$('#create-process-modal-button').on('click', function () {
    $('#processTitle').text('Tilføj indsats');
    $('#create-process').text('Tilføj');
});

function setClosedInstancesToFadedAndMoveDown() {
    $('.instanceClosed').each(function () {
        $(this).find('td, a').css('color', 'lightgray');
        $(this).remove();
        $('#childInstances').append(this);
    });
}
