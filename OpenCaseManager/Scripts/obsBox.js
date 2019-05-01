function toggleEdit() {
    if ($("#obsTextArea").attr("disabled")) {
        $("#obsTextArea").attr("disabled", false);
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
        console.log("done");
    });

    $("#obsTextArea").attr("disabled", true);
    $(".obsSaveButton").toggleClass('hide');
}