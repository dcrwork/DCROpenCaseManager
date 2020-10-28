// formserver functions to get the data
(function (window) {

    var dcrFormServerUrl = '';

    // add any instance
    function getFormServerUrl(callback, errorCallback) {
        API.serviceGET('services/GetDCRFormServerURL')
            .done(function (response) {
                window.FormServer.dcrFormServerUrl = response;
                callback(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    function getFormServerJS(data, callback, errorCallback) {
        incrementLoaderCount("formserver");
        fetch(data + '/Scripts/dynamicform/CallBackHandler.js', { method: 'GET', mode: 'no-cors' })
            .then(async (r) => {
                API.getJSFile(data + '/Scripts/dynamicform/CallBackHandler.js')
                    .done(function (response) {
                    })
                    .fail(function (e) {
                        App.showExceptionErrorMessage(e);
                    });
                // Read the response as json.
                return r;
            }).then(async (r) => {
                setTimeout(function () {
                    decrementLoaderCount("formserver");
                }, 3000);
            }).catch(e => {
                console.log(e);
            });
    }

    function getFormServerIFrame(data, callback, errorCallback) {
        fetch(data + '/Scripts/dynamicform/CallBackHandler.js', { mode: 'no-cors' }).then(r => {
            if (r.status == 200) {
                API.getJSFile(response + '/Scripts/dynamicform/CallBackHandler.js')
                    .done(function (response) {
                    })
                    .fail(function (e) {
                        App.showExceptionErrorMessage(e);
                    });
            }
        }).catch(e => {
            console.log(e);
        });
    }

    // formserver library
    var formserver = function () {
        this.getFormServerUrl = getFormServerUrl;
        this.getFormServerJS = getFormServerJS;
        this.getFormServerIFrame = getFormServerIFrame;
        this.dcrFormServerUrl = dcrFormServerUrl;
    };

    return window.FormServer = new formserver;
}(window));

// document get ready
$(document).ready(function () {
    FormServer.getFormServerUrl(FormServer.getFormServerJS);
});
