
// Processes Libray
(function (window) {
    // add Process Instance
    function addProcessInstance(graphId) {
        API.service('services/AddProcessInstance', { graphId: graphId })
            .done(function (response) {
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    function getProcesses() {
        var query = {
            "type": "SELECT",
            "entity": "Processes",
            "resultSet": ["Id", "GraphId", "Title", "OnFrontPage", "MajorVersionTitle", "MajorVerisonDate"],
            "order": [{ "column": "OnFrontPage", "descending": true }, { "column": "title", "descending": false }]
        }

        API.service('records', query)
            .done(function (response) {
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    // get process History
    function getProcessHistory(graphId, callback) {
        var query = {
            "type": "SELECT",
            "entity": "ProcessHistory",
            "filters": [
                {
                    "column": "GraphId",
                    "operator": "equal",
                    "value": graphId,
                    "valueType": "int"
                }
            ],
            "resultSet": ["Id", "GraphId", "Title", "OnFrontPage", "MajorVersionTitle", "MajorVerisonDate", "ReleaseDate"],
            "order": [{ "column": "MajorVersionId", "descending": true }]
        };

        API.service('records', query)
            .done(function (response) {
                callback(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    // private functions
    // render process history html
    function renderProcessHistory(response) {
        var data = JSON.parse(response);
        var html = '';
        for (var i = 0; i < data.length; i++) {
            html += '<tr><td style= "display:none">' + data[i].Id + '</td>' +
                '<td>' + data[i].Title + '</td>' +
                '<td>' + (data[i].MajorVersionTitle === null ? '&nbsp;' : data[i].MajorVersionTitle) + '</td > ' +
                '<td>' + (data[i].MajorVerisonDate === null ? '&nbsp;' : moment(new Date(data[i].MajorVerisonDate)).format('L LT')) + '</td > ' +
                '<td>' + (data[i].ReleaseDate === null ? '&nbsp; ' : moment(new Date(data[i].ReleaseDate)).format('L LT')) + '</td > ' +
                '</tr>';
        }
        $('#processes-history').html(html);
    }

    // processes library
    var processes = function () {
        this.addProcessInstance = addProcessInstance;
        this.getProcessHistory = getProcessHistory;
        this.renderProcessHistory = renderProcessHistory;
    };
    return window.Processes = new processes;
}(window));

// on document ready initialization
$(document).ready(function () {

    // add instance if new major version of process is available
    $(document).on('click', 'button[name="updateMajorRevision"]', function (e) {
        updateMajorVersion(e);
    });

    function updateMajorVersion(event) {
        var processId = event.currentTarget.attributes.processId.value;
        var graphId = event.currentTarget.attributes.graphId.value;
        var title = event.currentTarget.attributes.title.value;

        App.showConfirmMessageBox(translations.DoUpdateProcess.replace('$MajorRevisionTitle',
            event.currentTarget.attributes.revisionTitle.value), translations.Yes, translations.No, async function () {

                var instanceId = await API.service('services/AddProcessInstance', { graphId: graphId });
                var data = { graphId: graphId, title: title, instanceId: instanceId };
                var process = new Array();
                process.push(data);
                await API.service('records/AddProcessRevision', process);
                App.getAllProcesses();

            }, null, translations.UpdateProcess);
        event.preventDefault();
    }

    // check for major revisions for all processes
    $(document).on('click', '#checkMajorVersions', function (e) {
        App.hideUpdateMajorRevision();
    });

    // check for major revisions for specific process
    $(document).on('click', 'img[name="checkMajorVersions"]', function (e) {
        var input = { "graphId": e.currentTarget.attributes.graphId.value };
        App.hideUpdateMajorRevision(input);
    });
});