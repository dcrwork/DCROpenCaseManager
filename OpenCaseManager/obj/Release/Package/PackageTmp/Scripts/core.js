
// core functions to get the data

(function (window) {

    // add any instance
    function addInstance(data, callback, errorCallback) {

        var query = {
            title: data.title,
            graphId: data.graphId,
            userRoles: data.userRoles
        }

        API.service('records/addInstance', query)
            .done(function (response) {
                var result = JSON.parse(response);
                var instanceId = result;

                var responseData = {
                    instanceId: instanceId,
                    graphId: query.graphId,
                    employee: data.employee
                }
                callback(responseData);
            })
            .fail(function (e) {
                errorCallback(e);
            });
    }

    // initialize a graph 
    function initializeGraph(instanceId, graphId, callback, errorCallback) {

        API.service('services/InitializeGraph', { instanceId: instanceId, graphId: graphId })
            .done(function (response) {
                callback();
            })
            .fail(function (e) {
                errorCallback(e);
            });
    }

    // get instance phases
    function getInstancePhases(instanceId, callback, errorCallback) {
        var query = {
            "type": "SELECT",
            "entity": "InstancePhases",
            "resultSet": ["Title", "PhaseId", "CurrentPhase"],
            "filters": [
                {
                    "column": "InstanceId",
                    "operator": "equal",
                    "value": instanceId,
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "sequenceNumber", "descending": false }]
        }

        API.service('records', query)
            .done(function (response) {
                var results = {
                    response: response,
                    instanceId: instanceId
                };
                callback(results);
            })
            .fail(function (e) {
                errorCallback(e);
            });
    }

    // get all ui events
    function getUIEvents(resolve, reject, instanceId) {
        var query = {
            "type": "SELECT",
            "entity": "MUSTasks",
            "resultSet": ["EventId", "Responsible", "InstanceId", "EventTitle", "Due", "SimulationId", "GraphId", "IsPending", "IsExecuted", "Description", "IsUIEvent", "UIEventValue", "UIEventCssClass", "EventType"],
            "filters": [
                {
                    "column": "Responsible",
                    "operator": "equal",
                    "value": '$(loggedInUserId)',
                    "valueType": "string",
                    "logicalOperator": "and"
                },
                {
                    "column": "InstanceId",
                    "operator": "equal",
                    "value": instanceId,
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "IsPending", "descending": false }]
        }

        API.service('records', query)
            .done(function (response) {
                var results = JSON.parse(response);
                resolve(results);
            })
            .fail(function (e) {
                reject(e);
            });
    }

    // core library
    var core = function () {
        this.getInstancePhases = getInstancePhases;
        this.addInstance = addInstance;
        this.initializeGraph = initializeGraph;
        this.getUIEvents = getUIEvents;
    };

    return window.Core = new core;
}(window));