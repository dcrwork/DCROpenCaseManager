
$(document).ready(function () {

    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
    });

    promise.then(function (result) {
        var query = App.getParameterByName("query", window.location.href);
        $('#searchCase').val(query);
        App.searchCases(query);
    }, function (err) {
        console.log(err);
    });

});