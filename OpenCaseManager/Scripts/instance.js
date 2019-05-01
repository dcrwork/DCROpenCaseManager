
$(document).ready(function () {

    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
        App.hideDocumentWebpart();
    });

    promise.then(function (result) {
        var instanceId = App.getParameterByName("id", window.location.href);
        //App.getPhases(instanceId);
        App.getTasks(instanceId);
        App.getJournalHistoryForInstance(instanceId);
        App.getInstanceDetails(instanceId);
        $('#addNewDocumentText').text(translations.Documents);
    }, function (err) {
        console.log(err); // Error: "It broke"
    });

    $('#responsibleDropdown').change(function () {
        window.localStorage.setItem('responsibleDD', $('#responsibleDropdown').val());
        var instanceId = App.getParameterByName("id", window.location.href);
        App.getTasks(instanceId);
    });

    if (window.localStorage.getItem('responsibleDD') != null) {
        $('#responsibleDropdown').val(window.localStorage.getItem('responsibleDD'));
    }

    $('#taskStatusDropDown').change(function () {
        window.localStorage.setItem('taskStatusDD', $('#taskStatusDropDown').val());
        var instanceId = App.getParameterByName("id", window.location.href);
        App.getTasks(instanceId);
    });

    if (window.localStorage.getItem('taskStatusDD') != null) {
        $('#taskStatusDropDown').val(window.localStorage.getItem('taskStatusDD'));
    }

});
