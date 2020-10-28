﻿var oldText;

function toggleEdit() {
    if ($("#obsTextArea").attr("disabled")) {
        $("#obsTextArea").attr("disabled", false);
        $("#obsTextArea").focus();
    }
    else {
        $("#obsTextArea").attr("disabled", true);
    }
    $(".obsBoxEdit").toggleClass('hide');
    $(".obsSaveButton").toggleClass('hide');
    $(".obsCancelButton").toggleClass('hide');
    $(".textCount").toggleClass('hide');
    oldText = $("#obsTextArea").val();
}

async function saveObs() {
    var obsTekst = $("#obsTextArea").val();

    //api call goes here
    var Id = App.getParameterByName("id", window.location.href);
    var childId = App.getParameterByName("ChildId", window.location.href);

    if (childId == null && window.location.pathname == '/Child') // 3.9.2019
        childId = Id;

    if (childId == null) {
        childId = Id;
        var query = {
            "type": "SELECT",
            "entity": "[InstanceExtension]",
            "resultSet": ["InstanceId", "ChildId"],
            "filters": new Array(),
            "order": []
        }

        var whereInstanceIdMatchesFilter = {
            "column": "Instanceid",
            "operator": "equal",
            "value": Id,
            "valueType": "int",
            "logicalOperator": "and"
        };
        query.filters.push(whereInstanceIdMatchesFilter);
        var result2 = await window.API.service('records', query);
        var instance2 = JSON.parse(result2);
        childId = instance2[0].ChildId;
    }

    var data = {
        obsText: obsTekst,
        childId: childId
    };

    var API = window.API;


    API.service('services/updateChild', data)
        .done(function (response) {
            $("#obsTextArea").attr("disabled", true);
            $(".obsSaveButton").toggleClass('hide');
            $(".obsBoxEdit").toggleClass('hide');
            $(".obsCancelButton").toggleClass('hide');
            $(".textCount").toggleClass('hide');
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
            console.log(e);
        });
}

function cancelObs() {
    $("#obsTextArea").val(oldText);
    updateTextCount($("#obsTextArea")[0]);
    $(".obsSaveButton").toggleClass('hide');
    $(".obsCancelButton").toggleClass('hide');
    $(".obsBoxEdit").toggleClass('hide');
    $(".textCount").toggleClass('hide');
    $("#obsTextArea").attr("disabled", true);
}

function updateTextCount(element) {
    var value = element.value.length
    $("#textCount").text(value + " / 100");
    if (value > 100) {
        $("#textCount").attr("style", "color:#E04141");
        $("#obsSaveButton").attr("disabled", true);
    }
    else {
        $("#textCount").removeAttr("style");
        $("#obsSaveButton").removeAttr("disabled");
    }
}
