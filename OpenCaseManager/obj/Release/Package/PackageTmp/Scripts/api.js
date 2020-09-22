(function (window) {

    var api = window.API || {};
    api.url = "/api/";

    // call to interval services
    api.service = function (url, data, timeout) {
        if (typeof (timeout) == 'undefined') timeout = 30000;
        return $.ajax({
            type: 'POST',
            url: api.url + url,
            timeout: timeout,
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            accept: "application/json; charset=utf-8",
            data: JSON.stringify(data)
        });
    }

    // call to interval services
    api.serviceGET = function (url) {
        return $.ajax({
            type: 'GET',
            url: api.url + url,
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            accept: "application/json; charset=utf-8",
        });
    }

    // get javascript file
    api.getJSFile = function (url) {
        return $.getScript(url);
    }

    window.API = api;
}(window))