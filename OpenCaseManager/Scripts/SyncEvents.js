(function (window) {

    function syncInstanceEvents(xml, instanceId) {
        var query = {
            "xml": xml,
            "instanceId": instanceId
        };

        API.service('services/SyncEvents', query)
            .done(function (response) {
                App.showSuccessMessage('Events xml is successfully synced');
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }


    var syncEvents = function () {
        this.syncInstanceEvents = syncInstanceEvents;
    };

    return window.SyncEvents = new syncEvents;
}(window));
