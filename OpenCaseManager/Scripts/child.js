$(document).ready(function () {
    App.hideDocumentWebpart();

    var adjunktId = App.getParameterByName("id", window.location.href);
    
    var query = {
        "AdjunktId": adjunktId
    }
    API.service('records/GetAdjunktInfo', query)
        .done(function (response) {
            displayAdjunktName(response);
            setupChangeResponsible(response);
            var result = JSON.parse(response);
            showAdjunktInstancesX();
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
        });
});

function setupChangeResponsible(response) {
    var result = JSON.parse(response);
    var id = result[0].Id;
    var responsible = result[0].Responsible;
    var query = {
        "type": "SELECT",
        "entity": "[User]",
        "resultSet": ["Name"],
        "filters": new Array(),
        "order": []
    }
  
    var whereResponsibleIdMatchesFilter = {
        "column": "Id",
        "operator": "equal",
        "value": responsible,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereResponsibleIdMatchesFilter);


    API.service('records', query)
        .done(function (response2) {
            var result = JSON.parse(response2);
            var name = result[0].Name
            $("#itemResponsible").attr({
                "itemchildid": id,
                "oldResponsible": responsible,
                "itemResponsible": name,
                "changeResponsibleFor": "child"
            });
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
            console.log(e);
        });

    
}


function showAdjunktInstancesX() {
    var adjunktId = App.getParameterByName("id", window.location.href);
    var query = {
        "type": "SELECT",
        "entity": "ChildInstances('$(loggedInUserId)')",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": [{ "column": "Pending", "descending": true }, { "column": "PendingAndEnabled", "descending": true }]

    }

    var whereAdjunktIdMatchesFilter = {
        "column": "ChildId",
        "operator": "equal",
        "value": adjunktId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereAdjunktIdMatchesFilter);

    API.service('records', query)
        .done(function (response) {
            showAdjunktInstances(response);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
        });
}

function displayAdjunktName(response) {
    var result = JSON.parse(response);
    var adjunktName = (result[0] == undefined) ? 'Ingen adjunkt at finde' : ((result[0].Name == null) ? "Intet navn på adjunkt" : result[0].Name);
    $("#childName").html("").append(adjunktName);
    $('head title', window.parent.document).text(adjunktName);
}

function displayAdjunktNameX(adjunktName) {
    $("#childName").html("").append(adjunktName);
    $('head title', window.parent.document).text(adjunktName);
    $('#itemResponsible').attr('itemTitle', adjunktName);
}

function setAdjunktResponsible(adjunktInfo, adjunktId) {
    $('#childResponsibleName').text(adjunktInfo.CaseManagerName);
    $('#itemResponsible').attr('itemResponsible', adjunktInfo.CaseManagerName);
    $('#itemResponsible').attr('oldResponsible', adjunktInfo.CaseManagerInitials);
    $('#itemResponsible').attr('itemChildId', adjunktId);
    $('#itemResponsible').attr('changeResponsibleFor', 'child');
}

function showAdjunktInstances(OCMresponse) {
    var OCMresult = JSON.parse(OCMresponse);
    var list = "";
    if (OCMresult.length === 0) {
        list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
    } else {
        for (i = 0; i < OCMresult.length; i++) {
            list += getAdjunktInstanceHtml(OCMresult[i]);
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

function getAdjunktInstanceHtml(item) {

    var open;
    var instanceLink;
    var numberOfPending;
    var title;
    open = item.IsOpen ? "" : "instanceClosed";
    instanceLink = "../AdjunktInstance?id=" + item.Id + "&AdjunktId=" + item.ChildId;
    numberOfPending = (item.PendingAndEnabled == 0) ? "" : item.PendingAndEnabled;
    title = item.Title;
    
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
    returnHtml += (item.Pending == 'true') ? "<td><img src='../Content/Images/priorityicon.svg' height='16' width='16'/> " + numberOfPending + "</td>" : '<td></td>';
    returnHtml += (item.Pending == 'true') ? "<td style='display:none'> " + numberOfPending + "</td>" : '<td style="display:none;">0</td>';
    if (instanceLink == "")
        returnHtml += "<td>" + title + "</td>";
    else
        returnHtml += "<td><a class='linkStyling' href='" + instanceLink + "'>" + title + "</a></td>";
    returnHtml += "<td>" + item.Process + "</td>";
    if (item.CaseManagerName == null)
        returnHtml += "<td>" + "NULL" + "</td>";
        returnHtml += '<td><a href="#" class="linkStyling responsibleSelectOptions" itemResponsible="' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '" itemTitle="' + title + '" itemInstanceId="' + item.Id + '" changeResponsibleFor="instance">' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '</a></td>';

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
