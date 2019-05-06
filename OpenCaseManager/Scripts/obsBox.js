﻿function toggleEdit() {
    if ($("#obsTextArea").attr("disabled")) {
        $("#obsTextArea").attr("disabled", false);
        $("#obsTextArea").focus();

    }
    else {
        $("#obsTextArea").attr("disabled", true);
    }

    $(".obsSaveButton").toggleClass('hide');
}

function saveObs() {
    var obsTekst = $("#obsTextArea").val();

    //api call goes here
    var childId = App.getParameterByName("id", window.location.href);

    var data = {
        obsText: obsTekst,
        childId: childId
    };

    var API = window.API;

    API.service('records/updateChild', data).done(function (response) {
        console.log($("#obsTextArea").val(), response);
    });

    $("#obsTextArea").attr("disabled", true);
    $(".obsSaveButton").toggleClass('hide');
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