﻿var debugMode = true;
var Responsible = null;

(function (window) {

    function getAllProcesses() {
        var query = {
            "type": "SELECT",
            "entity": "Processes",
            "resultSet": ["Id", "ProcessId", "GraphId", "Title", "OnFrontPage", "MajorVersionTitle", "MajorVerisonDate", "ProcessOwner", "InstanceId", "ProcessApprovalState"],
            "order": [{ "column": "OnFrontPage", "descending": true }, { "column": "title", "descending": false }]
        }

        var result = API.service('records', query)
            .done(function (response) {
                renderProcesses(response);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getProcessAsync(filters) {
        if (filters)
            filters.push({
                "column": "Status",
                "operator": "equal",
                "value": true,
                "valueType": "boolean",
                "logicalOperator": "and"
            });
        var query = {
            "type": "SELECT",
            "entity": "Process",
            "filters": filters || [
                {
                    "column": "Status",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "and"
                },
                {
                    "column": "OnFrontPage",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "and"
                }
            ],
            "resultSet": ["Id", "GraphId", "Title", "OnFrontPage"],
            "order": [{ "column": "OnFrontPage", "descending": true }, { "column": "title", "descending": false }]
        }

        var result = API.service('records', query)
            .done(function (response) {
                return response;
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            })
        return result;
    }

    // api functions
    function getProcess(getRole, showOnFrontPage) {
        var filters;

        if (showOnFrontPage) {
            filters =
                [{
                    "column": "OnFrontPage",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "and"
                }]
        }
        if ((window.location.pathname.toLowerCase() === "/process/processes")) {
            filters =
                [{
                    "column": "Status",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "and"
                }]
        }
        getProcessAsync(filters).done(function (response) {
            if (getRole) {
                renderData("instanceProcesses", response, getProcessHtml)
                var graphId = $('#instanceProcesses').find(":selected").val();
                getRoles(graphId);
            }
            else {
                renderProcesses(response);
            }
        }).fail(function (e) {
            showExceptionErrorMessage(e);
        });
    }

    function renderProcesses(response) {
        renderData("processes", response, getProcessesHtml);
        registerEditProcessEvent();
        registerCancelProcessEvent();
        registerUpdateProcessEvent();
        registerDeleteProcessEvent();
        registerRefreshProcessEvent();
        registerGotoDCRGraphsEvent();
        registerCheckOnFrontPage();
        exportDCRXMLLog();
    }

    async function hideUpdateMajorRevision(graph) {
        var revisionUpdate = await getMajorRevisions(graph);
        for (var i = 0; i < revisionUpdate.length; i++) {
            if (revisionUpdate[i].Error !== '') {
                $('i[id="error_' + revisionUpdate[i].GraphId + '"]').removeClass('hide');
                $('i[id="error_' + revisionUpdate[i].GraphId + '"]').attr('title', revisionUpdate[i].Error);
            }
            else {
                $('button[name="updateMajorRevision"][graphId="' + revisionUpdate[i].GraphId + '"]').show();
                $('button[name="updateMajorRevision"][graphId="' + revisionUpdate[i].GraphId + '"]').attr('revisionTitle', revisionUpdate[i].MajorRevisionTitle);
            }
        }
    }

    async function getMajorRevisions(graph) {
        try {
            var result = await API.service('services/GetProcessMajorRevisions', graph);
            return result;
        }
        catch (e) {
            showErrorMessage(e.responseJSON.ExceptionMessage);
        }
    }

    function getMyInstances() {
        var query = {
            "type": "SELECT",
            "entity": "MyInstances",
            "resultSet": ["Title", "Id"],
            "filters": [
                {
                    "column": "Responsible",
                    "operator": "equal",
                    "value": "$(loggedInUserId)",
                    "valueType": "string",
                    "logicalOperator": "and"
                },
                {
                    "column": "IsOpen",
                    "operator": "equal",
                    "value": "1",
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "title", "descending": false }]
        }
        API.service('records', query)
            .done(function (response) {
                renderData("myInstances", response, getMyInstanceHtml)
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function addInstance(title, graphId, userRoles, IsChild, childId, caseNumberIdentifier, caseId) {

        var data = {
            title: title,
            graphId: graphId,
            userRoles: userRoles,
            childId: childId,
            caseNumberIdentifier: caseNumberIdentifier,
            caseId: caseId
        }

        API.service('records/addInstance', data)
            .done(function (response) {
                var result = JSON.parse(response);
                var instanceId = result;

                API.service('services/InitializeGraph', { instanceId: instanceId, graphId: graphId })
                    .done(function (response) {

                        getMyInstances();
                        getTasks();
                        if (!IsChild) {
                            window.location.replace('/Instance?id=' + instanceId);
                        }
                        else {
                            window.location.replace('/ChildInstance?id=' + instanceId + '&ChildId=' + childId);
                        }
                    })
                    .fail(function (e) {
                        showExceptionErrorMessage(e);
                    });

            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function addChild(childName, caseNumber, responsible) {
        var data = {
            childName: childName,
            caseNumber: caseNumber,
            responsible: responsible
        }

        API.service('records/addChild', data)
            .done(function (response) {
                var result = JSON.parse(response);
                var childId = result;
                window.location.replace('/Child?id=' + childId);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    async function executeEvent(data, isFrontPage, uiEvent, isMUS, type) {
        if (uiEvent != null) {
            var globalEvents = [];
            if (data.eventId.toLocaleLowerCase().startsWith("global")) {
                globalEvents = await canExecuteGlobalEvents(data.eventId, data.instanceId);
            }
            else {
                globalEvents = ["Not Global Event"];
            }

            if (globalEvents.length > 0) {
                var promise = new Promise(function (resolve, reject) {
                    API.service('records/ReplaceEventTypeParamsKeys', { instanceId: data.instanceId, eventTypeValue: uiEvent })
                        .done(function (response) {
                            getCustomCode(data, response, resolve);
                        })
                        .fail(function (e) {
                            showExceptionErrorMessage(e);
                        });
                });
                promise.then(function () {
                    executeEvent(data, isFrontPage, null, isMUS);
                }, function (e) {
                    showExceptionErrorMessage(e);
                });
            }
        }
        else {
            if (data.eventId.toLocaleLowerCase().startsWith("global")) {
                globalEvents = await canExecuteGlobalEvents(data.eventId, data.instanceId);
                if (globalEvents.length > 0) {
                    await executeGlobalEvents(data.eventId, data.instanceId, globalEvents);
                }
            } else {
                API.service('services/ExecuteEvent', { taskId: data.taskId, instanceId: data.instanceId, graphId: data.graphId, simulationId: data.simulationId, eventId: data.eventId, title: data.title, trueEventId: data.trueEventId })
                    .done(function (response) {
                        if (type === "tasksWNoteFull") {
                            closeWindowWithoutAsking = null;
                            window.close();
                        }
                        if (isMUS == null) {
                            if (isFrontPage) {
                                getMyInstances();
                                getTasks();
                            }
                            else {
                                getTasks(data.instanceId);
                                getInstanceDetails(data.instanceId);
                                getJournalHistoryForInstance(data.instanceId);
                                getPhases(data.instanceId);
                            }
                        }
                        else {
                            MUS.musDetails(MUS.showMUS);
                        }
                    })
                    .fail(function (e) {
                        showExceptionErrorMessage(e);
                        if (isMUS == null) {
                            if (isFrontPage) {
                                getMyInstances();
                                getTasks();
                            }
                            else {
                                getTasks(data.instanceId);
                                getInstanceDetails(data.instanceId);
                                getJournalHistoryForInstance(data.instanceId);
                                getPhases(data.instanceId);
                            }
                        } else {
                            MUS.musDetails(MUS.showMUS);
                        }
                    });
            }
        }
    }

    async function canExecuteGlobalEvents(eventId, instanceId) {
        var childId = 0;
        if ((window.location.pathname.toLowerCase() === "/child")) {
            childId = getParameterByName("id", window.location.href);
        }
        else {
            childId = App.getParameterByName("ChildId", window.location.href);
            if (childId === null) {
                var Id = getParameterByName("id", window.location.href);
                var query = {
                    "type": "SELECT",
                    "entity": "[InstanceExtension]",
                    "resultSet": ["InstanceId", "ChildId"],
                    "filters": new Array(),
                    "order": []
                };

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
        }

        var globalEventData = {
            "eventId": eventId,
            "childId": childId
        };

        var globalEventsJSON = await API.service('services/CanExecuteGlobalEvent', globalEventData);
        var globalEvents = JSON.parse(globalEventsJSON);
        if (globalEvents.length > 0) {
            if (globalEvents[0].Message === '') {
                return globalEvents;
            }
            else {
                App.showErrorMessage(globalEvents[0].Message);
                return [];
            }
        }
        return [];
    }

    async function executeGlobalEvents(eventId, instanceId, globalEvents) {
        var childId = 0;
        if ((window.location.pathname.toLowerCase() === "/child")) {
            childId = getParameterByName("id", window.location.href);
        }
        else {
            childId = App.getParameterByName("ChildId", window.location.href);
            if (childId === null) {
                var Id = getParameterByName("id", window.location.href);
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
        }

        var globalEventData = {
            "eventId": eventId,
            "childId": childId
        };

        globalEventsJSON = await API.service('services/ExecuteGlobalEvents', globalEvents);
        getTasks(instanceId);
        getInstanceDetails(instanceId);
        getJournalHistoryForInstance(instanceId);
        getPhases(instanceId);
    }

    function getInstanceDetails(id) {
        var query = {
            "type": "SELECT",
            "entity": "AllInstances",
            "resultSet": ["Id", "Title", "CaseNoForeign", "CaseLink", "CurrentPhaseNo", "Description", "GraphId", "NextDeadline", "IsOpen", "Responsible"],
            "filters": [
                {
                    "column": "Id",
                    "operator": "equal",
                    "value": id,
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "title", "descending": false }]
        }
        API.service('records', query)
            .done(function (response) {
                renderData("instanceDetails", response, getInstanceHtml);

                var result = JSON.parse(response);
                if (typeof (Instruction) == 'undefined') Instruction = null;
                
                //set workflow title
                if (result.length > 0) {
                    if (Instruction != null) {
                        Instruction.setInstanceIsAccepting(result[0].IsOpen);
                    }
                    if (result[0].GraphId != null) {
                        query = {
                            "type": "SELECT",
                            "entity": "Process",
                            "resultSet": ["Title"],
                            "filters": [
                                {
                                    "column": "GraphId",
                                    "operator": "equal",
                                    "value": result[0].GraphId,
                                    "valueType": "int"
                                }
                            ],
                            "order": [{ "column": "Title", "descending": true }]
                        };

                        API.service('records', query)
                            .done(function (response) {
                                var result = JSON.parse(response)
                                if (Instruction != null)
                                    Instruction.setTitleText(result[0].Title);
                            })
                            .fail(function (e) {
                                App.showExceptionErrorMessage(e);
                            });
                    }
                }
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getJournalHistoryForInstance(instanceId) {
        var query = {
            "type": "SELECT",
            "entity": "JournalHistoryForASingleInstance(" + instanceId + ")",
            "resultSet": ["*"],
            "filters": [],
            "order": [{ "column": "EventDate", "descending": true }]
        }

        API.service('records', query)
            .done(function (response) {
                Task.getJournalHistory(response);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getTasks(instanceId, showOnlyMyTasks) {
        // get all tasks of all my instance
        var entityName = "dbo.InstanceTasks('$(loggedInUserId)')";
        var onlyMyTasks = false;
        var onlyTasksFromOpenInstances = false;
        var showCaseInfo = false;
        if (showOnlyMyTasks)
            onlyMyTasks = showOnlyMyTasks;

        if (instanceId == null) {
            onlyMyTasks = true;
            onlyTasksFromOpenInstances = true;
            instanceId = 0;
            showCaseInfo = true;
        }
        else {
            if (window.localStorage.getItem('responsibleDD') == "1") {
                onlyMyTasks = true;
            }
            else if (window.localStorage.getItem('responsibleDD') == "0") {
                onlyMyTasks = false;
            }
        }

        var promise = new Promise(function (resolve, reject) {
            Task.getTasks(entityName, onlyMyTasks, onlyTasksFromOpenInstances, instanceId, resolve, reject);
        });
        promise.then(function (response) {
            Task.tasksHtml('tasks', response, showCaseInfo, onlyMyTasks);
            if (instanceId == 0) {
                Task.hideTableColumns(['responsible']);
            }
            else {
                if (window.location.href.toLocaleLowerCase().indexOf("MyActivities".toLocaleLowerCase()) > 0)
                    Task.showSingleInstanceFilter();
            }
        }, function (e) {
            showExceptionErrorMessage(e);
        });
    }

    function getPhases(InstanceId) {
        Core.getInstancePhases(InstanceId, getPhasesCallback, showExceptionErrorMessage);
    }

    function getPhasesCallback(results) {
        var response = results.response;
        var instanceId = results.instanceId;

        var result = JSON.parse(response);
        if (result.length > 0)
            renderData("processPhase", response, getProcessPhaseHtml);
        else
            $('#processPhase').html('');
    }

    function getResponsible(resolve) {
        if (Responsible !== null) {
            return resolve();
        }
        else {
            API.service('services/getResponsible', {})
                .done(function (response) {
                    if (response.length > 0)
                        Responsible = {};
                    else {
                        showErrorMessage(translations.NoUserDetails);
                        return;
                    }
                    resolve();
                })
                .fail(function (e) {
                    showExceptionErrorMessage(e);
                });
        }
    }

    function getRoles(graphId, resolve, containerId) {
        //todo:mytasks-Check for Process Engine
        var data = { graphId: graphId };
        API.service('services/getProcessRoles', data)
            .done(function (response) {
                var roles = JSON.parse(response);
                roles = skipAutoRoles(roles);
                if (roles.length > 0) {
                    if (resolve != null) {
                        resolve(roles);
                        return;
                    }
                    var query = {
                        "type": "SELECT",
                        "entity": "UserDetail",
                        "resultSet": ["Name", "Id"],
                        "order": [{ "column": "name", "descending": false }]
                    }
                    API.service('records', query)
                        .done(function (response) {
                            renderUserRolesData(containerId || 'userRoles', response, roles, getUserRoles);
                        })
                        .fail(function (e) {
                        });
                }
                else {
                    $('#userRoles').html('');
                }
            })
            .fail(function (e) {
            });
    }

    function searchProcess(searchText) {
        var data = { searchText: searchText };
        API.service('services/searchProcess', data)
            .done(function (response) {
                var processes = JSON.parse(response);
                renderProcessesHtml('processes', response, getSearchProcessesHTML);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
    }

    async function addProcesses(searchText) {
        var selectedProcess = new Array();
        $('.process:checked').each(function (index, checkbox) {
            var id = $(this).attr('id');
            var title = $('input[name="addProcess"][Id="' + id + '"]').val();
            var data = { graphId: id, title: title };
            selectedProcess.push(data);
        });

        if (selectedProcess.length > 0) {
            for (var i = 0; i < selectedProcess.length; i++) {
                var instanceId = await API.service('services/AddProcessInstance', { graphId: selectedProcess[i].graphId });
                var data = { graphId: selectedProcess[i].graphId, title: selectedProcess[i].title, instanceId: instanceId };
                var process = new Array();
                process.push(data);

                API.service('records/addProcess', process)
                    .done(function (response) {
                        var count = JSON.parse(response);
                        if (count === 1)
                            showSuccessMessage(translations.ProcessAdded);
                        else if (count > 1)
                            showSuccessMessage(count + ' ' + translations.ProcessesAdded);
                    })
                    .fail(function (e) {
                        if (e.status === 409) {
                            showErrorMessage(translations.ProcessAlreadyAdded);
                        }
                        else
                            showExceptionErrorMessage(e)
                    });
            }
        }
    }

    function updateProcess(graphId, processTitle, showOnFrontPage) {
        var data = { graphId: graphId, processTitle: processTitle, processStatus: true, showOnFronPage: showOnFrontPage };

        API.service('records/updateProcess', data)
            .done(function (response) {
                getAllProcesses();
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function deleteProcess(graphId, processTitle, showOnFrontPage) {
        var data = { graphId: graphId, processTitle: processTitle, processStatus: false, showOnFronPage: showOnFrontPage };

        API.service('records/updateProcess', data)
            .done(function (response) {
                getAllProcesses();
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function searchCases(searchText) {
        var data = {
            "type": "SELECT",
            "entity": "AllInstances",
            "resultSet": ["Id", "Title", "CaseNoForeign", "IsOpen", "Responsible"],
            "filters": [
                {
                    "column": "Title",
                    "operator": "like",
                    "value": '%' + searchText + '%',
                    "valueType": "string",
                    "logicalOperator": "or"
                },
                {
                    "column": "CaseNoForeign",
                    "operator": "like",
                    "value": '%' + searchText + '%',
                    "valueType": "string"
                }
            ],
            "order": [{ "column": "Title", "descending": true }, { "column": "CaseNoForeign", "descending": true }]
        }

        API.service('records', data)
            .done(function (response) {
                var cases = JSON.parse(response);
                renderData('search-cases', response, getSearchCasesHTML);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
    }

    function getResponsibleName() {
        var data = {
            "type": "SELECT",
            "entity": "[User]",
            "resultSet": ["Name", "Id"],
            "filters": [
                {
                    "column": "Id",
                    "operator": "equal",
                    "value": "$(loggedInUserId)",
                    "valueType": "string",
                }
            ]
        }

        API.service('records', data)
            .done(function (response) {
                var user = JSON.parse(response);
                if (user.length > 0)
                    $('#userName').text(user[0].Name);
                window.App.user = user[0];
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
    }

    function refreshProcess(graphId, processId) {
        var data = { graphId: graphId, processId: processId };

        API.service('records/updateProcessFromDCR', data)
            .done(function (response) {
                getProcess(false);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    async function openDCRGraphsURL(graphId) {
        var key = "DCRPortalURL";
        var response = await getKeyValue(key);
        window.open(response + '/Tool?id=' + graphId);
    }

    function logJsError(message) {

        API.service('records/logJsError', message)
            .done(function (response) {

            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
    }

    function hideDocumentWebpart(showWebPart) {
        if (showWebPart != null) {
            if (showWebPart) {
                $('#documents').show();
            }
            else {
                $('#documents').hide();
            }
        }
        else {
            API.serviceGET('services/hidedocumentwebPart')
                .done(function (response) {
                    if (response) {
                        $('#documents').hide();
                    }
                    else {
                        $('#documents').show();
                    }
                })
                .fail(function (e) {
                    showExceptionErrorMessage(e)
                });
        }
    }

    async function getKeyValue(key) {
        try {
            var result = await API.serviceGET('services/GetKeyValue?key=' + key);
            return result;
        }
        catch (e) {
            // App.showErrorMessage(e.responseJSON.ExceptionMessage);
            alert(e.Message);
        }
    }

    // public functions
    function getParameterByName(name, url) {
        if (!url) url = window.location.href;
        url = url.toLowerCase();
        name = name.toLowerCase();
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    // private functions

    function registerCheckOnFrontPage() {
        $('.bootstrap-toggle').bootstrapToggle({
            width: '50%',
            height: '20%'
        });

        $('input[class="bootstrap-toggle"]').on('change', function (e) {
            var graphId = e.currentTarget.attributes.graphId.value;
            var processTitle = $('input[graphId="' + graphId + '"]').val();
            var showOnFrontPage = $(e.currentTarget).prop('checked');
            updateProcess(graphId, processTitle, showOnFrontPage);
        });
    }

    function registerEditProcessEvent() {
        $('button[name="editProcess"]').on('click', function (e) {
            var graphId = e.currentTarget.attributes.graphId.value;
            $('a[graphId="' + graphId + '"]').hide();
            $('span[graphId="' + graphId + '"]').hide();
            $('input[graphId="' + graphId + '"]').show();
            $('button[name="editProcess"][graphId="' + graphId + '"]').hide();
            $('button[name="updateProcess"][graphId="' + graphId + '"]').show();
            $('button[name="cancelUpdateProcess"][graphId="' + graphId + '"]').show();
            $('button[name="deleteProcess"][graphId="' + graphId + '"]').hide();
            $('button[name="refreshProcess"][graphId="' + graphId + '"]').hide();
            $('button[name="inApproval"][graphId="' + graphId + '"]').hide();
            $('button[name="updateMajorRevision"][graphId="' + graphId + '"]').hide();
            $('a[name="processInstance"][graphId="' + graphId + '"]').hide();
            $('a[name="processHistory"][graphId="' + graphId + '"]').hide();
            $('button[name="gotoProcess"][graphId="' + graphId + '"]').hide();
            $('button[name="exportModal"][graphId="' + graphId + '"]').hide();
            $('i[id="error_' + graphId + '"]').hide();
            e.preventDefault();
        });
    }

    function registerCancelProcessEvent() {
        $('button[name="cancelUpdateProcess"]').on('click', function (e) {
            getAllProcesses();
        });
    }

    function registerUpdateProcessEvent() {
        $('button[name="updateProcess"]').on('click', function (e) {
            var graphId = e.currentTarget.attributes.graphId.value;
            var processTitle = $('input[type="text"][graphId="' + graphId + '"]').val();
            var showOnFrontPage = $('input[type="checkbox"][graphId="' + graphId + '"]').prop('checked');
            updateProcess(graphId, processTitle, showOnFrontPage);
            e.preventDefault();
        });
    }

    function registerDeleteProcessEvent() {
        $('button[name="deleteProcess"]').on('click', function (e) {
            App.showConfirmMessageBox(translations.ProcessDeleteMessage, translations.Yes, translations.No, function () {

                var graphId = e.currentTarget.attributes.graphId.value;
                var processTitle = $('a[graphId="' + graphId + '"]').text();
                var showOnFrontPage = $('input[type="checkbox"][graphId="' + graphId + '"]').prop('checked');

                deleteProcess(graphId, processTitle, showOnFrontPage);

            }, null, translations.Delete);
            e.preventDefault();
        });
    }

    function registerRefreshProcessEvent() {
        $('button[name="refreshProcess"]').on('click', function (e) {
            var graphId = e.currentTarget.attributes.graphId.value;
            var processId = e.currentTarget.attributes.processId.value;

            refreshProcess(graphId, processId);
            e.preventDefault();
        });
    }

    function registerGotoDCRGraphsEvent() {
        $('button[name="gotoProcess"]').on('click', function (e) {
            var graphId = e.currentTarget.attributes.graphId.value;

            openDCRGraphsURL(graphId);
            e.preventDefault();
        });
    }

    function exportDCRXMLLog() {
        $('button[name="exportModal"]').on('click', function (e) {
            var processId = e.currentTarget.attributes.graphId.value;
            $('#exportDCRXMLLOGFileModal').modal('toggle');

            $('#graphId').val(processId);
            e.preventDefault();
        });
    }

    async function showTaskWithNoteFullPopup(data, elem, isFrontPage, uievent, isMUS) {
        var globalEvents = [];
        if (data.eventId.toLocaleLowerCase().startsWith("global")) {
            globalEvents = await canExecuteGlobalEvents(data.eventId, data.instanceId);
        }
        else {
            globalEvents = ["Not Global Event"];
        }

        if (globalEvents.length > 0) {
            CreateJournalNoteView(data.eventId, data.Modified);
        }
    }

    async function showTaskWithNotePopup(data, elem, isFrontPage, uievent, isMUS) {
        var globalEvents = [];
        if (data.eventId.toLocaleLowerCase().startsWith("global")) {
            globalEvents = await canExecuteGlobalEvents(data.eventId, data.instanceId);
        }
        else {
            globalEvents = ["Not Global Event"];
        }

        if (globalEvents.length > 0) {

            var eventTitle = elem.next('.title').html();
            var eventDescription = elem.next().next('.description').html();
            var taskWNoteModal = $('#TasksWNote');
            taskWNoteModal.modal('show');
            taskWNoteModal.find('.TasksWNoteheading span').text(eventTitle);
            taskWNoteModal.find('.modal-body p').html(eventDescription);
            taskWNoteModal.find('.commentbox').val('');

            taskWNoteModal.find('.TasksWNoteBtn').on('click', function (e) {
                taskWNoteModal.find('.TasksWNoteBtn').unbind("click");
                var promise = new Promise(function (resolve, reject) {
                    var comment = taskWNoteModal.find('.commentbox').val();
                    Task.saveTasksNote(data.eventId, data.instanceId, comment, false, resolve, reject);
                });
                promise.then(function (response) {
                    App.executeEvent(data, isFrontPage, uievent, isMUS);
                    taskWNoteModal.modal('hide');
                }, function (e) {
                    showExceptionErrorMessage(e);
                });
            });
        }
    }

    function getMyInstanceHtml(item) {
        return '<tr class="trStyleClass"><td><a href="/Instance?id=' + item.Id + '"> ' + item.Title + '</a></td ></tr>';
    }

    function getInstanceHtml(item) {

        $('#instanceTitle').text(item.Title);
        if (item.CaseNoForeign !== null) {
            $('.caseNum').show();
            $('#instanceCaseNo').text(item.CaseNoForeign);
        }
        if (item.CaseLink !== null) {
            $('.caseLink').show();
            $('#entityLink').attr('href', item.CaseLink);
        }

        if (item.IsOpen) $('#instanceTitle').append(getStatus(item.NextDeadline));
        else $('#instanceTitle').append("<span class='dot dotGrey'></span>");
        if (typeof (Instruction) == 'undefined') Instruction = null;
        if (Instruction != null) {
            if (item.Description != null && item.Description != '')
                Instruction.setText(item.Description);
            else
                Instruction.hideWebPart();
        }

        $('#instanceResponsible').text(item.Responsible);
        $('#itemResponsible').attr('itemResponsible', item.Responsible);
        $('#itemResponsible').attr('itemTitle', item.Title);
        $('#itemResponsible').attr('itemInstanceId', item.Id);
        $('#itemResponsible').attr('changeResponsibleFor', 'instance');
    }

    function getProcessHtml(item) {
        return "<option value= " + item.GraphId + ">" + item.Title + "</option>";
    }

    function getProcessesHtml(item) {
        var returnHtml = '';
        returnHtml = '<tr class="trStyleClass">' +
            '<td style="display:none;" > ' + item.ProcessId + '</td >' +
            '<td>' + item.GraphId + '</td>' +
            '<td> ' +
            '<a href="../../process?id=' + item.ProcessId + '" processId="' + item.ProcessId + '" graphId="' + item.GraphId + '">' + item.Title + '</a><i style="margin-left:5px;" id="error_' + item.GraphId + '" class="fas fa-exclamation-circle text-danger hide"></i>' +
            '<input graphId="' + item.GraphId + '" processId="' + item.ProcessId + '" style="display:none" type="text" name="processTitle" class="form-control" value="' + item.Title + '"/>' +
            '</td>' +
            '<td>&nbsp;' + (item.MajorVersionTitle == null ? '' : item.MajorVersionTitle) + ' </td>' +
            '<td> ' + (item.MajorVerisonDate === null ? '&nbsp;' : moment(new Date(item.MajorVerisonDate)).format('L LT')) + ' </td>' +
            '<td> ' + '' + item.ProcessOwner + ' </td>' +

            '<td> <input processId="' + item.ProcessId + '" graphId="' + item.GraphId + '" type="checkbox" class="bootstrap-toggle" data-size="mini" data-onstyle="info" data-style="color" data-on="' + translations.On + '" data-off="' + translations.Off + '" ';
        if (item.OnFrontPage) {
            returnHtml += " checked ";
        }
        returnHtml += '/> ' +
            '</td>' +
            '<td>';
        returnHtml += '<button style="margin-right:3px;" processId="' + item.ProcessId + '" title="' + item.Title + '" graphId="' + item.GraphId + '" type="button" value="" class="btn btn-info taskExecutionBtn"><img class="btn btn-info taskExecutionBtn" src="../Content/Images/update.png" name="checkMajorVersions" graphId="' + item.GraphId + '" style="cursor:pointer" /></button>';
        returnHtml += '<button style="margin-right:3px;display:none;" approval="' + item.ProcessApprovalState + '" processId="' + item.ProcessId + '" title="' + item.Title + '" graphId="' + item.GraphId + '" type="button" name="updateMajorRevision" value="" class="btn btn-info taskExecutionBtn"><img title="' + translations.NewVersionAvailable + '" src="../../content/images/standard/refresh.png"></button>';
        if (item.ProcessApprovalState === 0) {
            returnHtml += '<button style="display:inline" approval="' + item.ProcessApprovalState + '" processId="' + item.ProcessId + '" title="' + translations.InApproval + '" graphId="' + item.GraphId + '" type="button" name="inApproval" value="" class="btn btn-info taskExecutionBtn"><i class="fas fa-user-edit"></i></button>&nbsp;';
        }
        if (item.InstanceId !== null)
            returnHtml += '<a name="processInstance" target="_blank" href="../instance?id=' + item.InstanceId + '" graphId="' + item.GraphId + '" class="btn btn-info taskExecutionBtn"><i class="fas fa-infinity btn-info" title="' + translations.ProcessInstance + '"></i></a> ';
        returnHtml += '<a name="processHistory" graphId="' + item.GraphId + '" target="_blank" href="../process/processhistory?graphid=' + item.GraphId + '" class="btn btn-info taskExecutionBtn"><i class="fas fa-history btn-info" title="' + translations.ProcessHistory + '"></i></a> ';
        returnHtml += '<button graphId="' + item.GraphId + '" processId="' + item.ProcessId + '" type="button" name="editProcess" value="editProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Edit + '" src="../../content/images/standard/edit.png"></button> ';
        returnHtml += '<button  graphId="' + item.GraphId + '" style="display:none" processId="' + item.ProcessId + '" type="button" name="updateProcess" value="updateProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Edit + '" src="../../content/images/standard/edit.png"></button> ';
        returnHtml += '<button  graphId="' + item.GraphId + '"  style="display:none" processId="' + item.ProcessId + '" type="button" name="cancelUpdateProcess" value="cancelUpdateProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Cancel + '" src="../../content/images/standard/cancel.png"></button> ';
        //returnHtml += '<button style="display:none;" processId="' + item.ProcessId + '" graphId="' + item.GraphId + '" type="button" name="refreshProcess" value="refreshProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Update + '" src="../../content/images/standard/update.png"></button> ';
        returnHtml += '<button graphId="' + item.GraphId + '" processId="' + item.ProcessId + '" type="button" name="deleteProcess" value="deleteProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Delete + '" src="../../content/images/standard/delete.png"></button> ';
        returnHtml += '<button processId="' + item.ProcessId + '" graphId="' + item.GraphId + '" type="button" name="gotoProcess" value="gotoProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.GotoGraph + '" src="../../content/images/standard/goto.png"></button> ';
        returnHtml += '<button processId="' + item.ProcessId + '" graphId="' + item.GraphId + '" type="button" name="exportModal" value="exportModal" class="btn btn-info taskExecutionBtn"><img title="' + translations.ExportDCRXML + '" src="../../content/images/standard/exportFile.png"></button> ';
        returnHtml += '</td>' + '</tr>';
        return returnHtml;
    }

    function getProcessPhaseHtml(item, index, count) {
        var html = '';
        if (item.PhaseId == item.CurrentPhase)
            html = "<li class=\"phaseItem selectedPhase\"> " + item.Title + "</li><li>";
        else
            html = "<li class=\"phaseItem\"> " + item.Title + "</li><li>";

        if (index < count - 1)
            html += "<img class='phaseDash' src='' /></li> ";
        return html;
    }

    function renderData(id, response, template) {
        var result = JSON.parse(response)
        var list = "";
        if (result.length === 0)
            list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        else {
            for (i = 0; i < result.length; i++) {
                list += template(result[i], i, result.length);
            }
        }
        $("#" + id).html("").append(list);
    }

    function renderUserRolesData(id, response, roles, template) {
        var result = JSON.parse(response)
        var list = "";
        if (roles.length === 0)
            list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        else {
            for (i = 0; i < roles.length; i++) {
                list += template(roles[i], result);
            }
        }
        $("#" + id).html("").append(list);

        /*  for (i = 0; i < result.length; i++) {
              $('#multi-select-' + result[i].Id + '').multiselect();
          }*/
    }

    function getUserRoles(role, users) {
        var returnHtml = '';
        returnHtml = '<div class="form-group clearfix" style="width:100%"><label class="labelStyling">' + role.title + '</label>' +
            '<select name="multi-select" class="form-control formFieldStyling" userId="' + role.title + '" id="multi-select-' + role.title + '">';
        if (role.title == "Socialrådgiver") {
            if (users.length > 0) {
                $.each(users, function (index, user) {
                    if (window.App.user.Id == user.Id) returnHtml += '<option selected="selected" value="' + user.Id + '">' + user.Name + '</option>';
                    else returnHtml += '<option value="' + user.Id + '">' + user.Name + '</option>';
                });
            }
        } else {
            returnHtml += '<option value="0">' + translations.SelectResponsible + '</option>';
            if (users.length > 0) {
                $.each(users, function (index, user) {
                    returnHtml += '<option value="' + user.Id + '">' + user.Name + '</option>';
                });
            }
        }
        returnHtml += '</select></div>';
        return returnHtml;
    }

    function renderProcessesHtml(id, response, template) {
        console.log("data", response);
        var result = JSON.parse(response)
        var list = "";
        if (result.graphs.graph === null)
            list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        else {
            $('#addProcess').show();
            if (result.graphs !== '' && result.graphs.graph.length > 1) {
                for (i = 0; i < result.graphs.graph.length; i++) {
                    list += template((result.graphs.graph[i]));
                }
            } else if (result.graphs !== '') {
                list += template((result.graphs.graph));
            }
            else {
                list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
                $('#addProcess').hide();
            }
        }
        $("#" + id).html("").append(list);
    }

    function getSearchProcessesHTML(item) {
        var returnHtml = '';
        returnHtml = '<tr class="trStyleClass">' +
            '<td class="selectBox"><input class="process" id=' + item["@id"] + ' type="checkbox" name="' + item["@title"] + '"></td>' +
            '<td><span>' + item["@id"] + '</span></td>' +
            '<td><input name="addProcess" Id="' + item["@id"] + '" type="text" value="' + item["@title"] + '" class="form-control"/></td>' +
            '</tr>';
        return returnHtml;
    }

    function getSearchCasesHTML(item) {
        if (item.CaseNoForeign == null)
            item.CaseNoForeign = '-';
        if (item.IsOpen)
            item.IsOpen = translations.Open;
        else
            item.IsOpen = translations.Close;
        var returnHtml = '';
        returnHtml = '<tr class="trStyleClass"><td> <a href="../ChildInstance?id=' + item.Id + '">' + item.Title + '</a></td><td>' + item.CaseNoForeign + '</td><td>' + item.Responsible + '</td><td>' + item.IsOpen + '</td></tr>';
        return returnHtml;
    }

    function setTexts() {
        if (typeof (translations) === "undefined") {
            getTranslations();
        } else {
            $('[data-locale]').each(function (index, element) {
                if ($(this).attr('data-apply') === 'attribute') {
                    $(this).attr($(this).attr('data-attribute'), translations[$(this).attr('data-key')]);
                }
                else if ($(this).attr('data-apply') === 'text') {
                    $(this).text(translations[$(this).attr('data-key')]);
                }
            });
        }

    }

    function getTranslations() {
        var localeDefs = {
            'da': 'da-DK',
            'da-DK': 'da-DK',
            'en': 'en-US',
            'en-US': 'en-US',
            'pt': 'pt-BR',
            'pt-BR': 'pt-BR',
            'es': 'es',
            'es-AR': 'es',
            'ca': 'ca',
            'it': 'it-IT',
            'it-IT': 'it-IT',
            'nb': 'nb-NO',
            'nb-NO': 'nb-NO',
            'sv': 'se-SV',
            'se-SV': 'se-SV'
        };

        var browserLocale = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
        var defaultLocale = 'en-US';

        var locale = (localeDefs[browserLocale] === undefined) ? defaultLocale : localeDefs[browserLocale];
        var getUrl = window.location;
        var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
        var fileUrl = baseUrl + 'scripts/translations/' + locale + '.js';

        API.getJSFile(fileUrl)
            .done(function (response) {
                setTexts();
            })
            .fail(function (e) {
                alert('Error in getting locale');
            });
    }

    function skipAutoRoles(roles) {
        var returnRoles = [];

        if (roles.roles != null && roles.roles !== '') {

            if (roles.roles.role.length > 1) {

                $.each(roles.roles.role, function (index, role) {
                    if (!(role["#text"].toLocaleLowerCase() === 'robot' || role["#text"].toLocaleLowerCase() === 'automatic')) {
                        var roleObject = { title: role["#text"] };
                        returnRoles.push(roleObject);
                    }
                });
            }
            else if (roles.roles.role !== null) {
                if (!(roles.roles.role["#text"].toLowerCase() === 'robot' || roles.roles.role["#text"].toLowerCase() === 'automatic')) {
                    var role = { title: roles.roles.role["#text"] };
                    returnRoles.push(role);
                }
            }

        }

        return returnRoles;
    }

    function showExceptionErrorMessage(exception) {
        var message = '';
        if (exception.ExceptionMessage !== undefined) {
            message = exception.ExceptionMessage;
        }
        else if (exception.Message !== undefined) {
            message = exception.Message;
        }
        else if (exception.responseJSON !== undefined) {
            try {
                message = exception.responseJSON.ExceptionMessage;
            }
            catch (e) {
                if (exception.responseJSON.Message !== undefined) {
                    message = exception.responseJSON.Message;
                }
                else if (exception.responseText !== undefined) {
                    message = exception.responseText;
                }
                else {
                    message = 'An error occured. Check Log to see the details';
                }
            }
        }
        else
        {
            message = exception.responseText;
        }

        new Noty({
            type: 'error',
            theme: 'mint',
            layout: 'topRight',
            text: message,
            timeout: 5000,
            container: '.custom-container'
        }).show();

    }

    function showErrorMessage(message) {
        var errorMessage = '';
        try {
            errorMessage = JSON.parse(message);
        }
        catch (ex) {
            errorMessage = message;
        }

        if (typeof (errorMessage) === "string")
            new Noty({
                type: 'error',
                theme: 'mint',
                layout: 'topRight',
                text: errorMessage,
                timeout: 5000,
                container: '.custom-container'
            }).show();
        else {
            showExceptionErrorMessage(errorMessage);
        }
    }

    function showSuccessMessage(message) {
        new Noty({
            type: 'success',
            theme: 'mint',
            layout: 'topRight',
            text: message,
            timeout: 5000,
            container: '.custom-container'
        }).show();
    }

    function showWarningMessage(message) {
        new Noty({
            type: 'warning',
            theme: 'mint',
            layout: 'topRight',
            text: message,
            container: '.custom-container'
        }).show();
    }

    function showInformationMessage(message) {
        new Noty({
            type: 'info',
            theme: 'mint',
            layout: 'topRight',
            text: message,
            container: '.custom-container'
        }).show();
    }

    // confirm function for with the modal
    function showConfirmMessageBox(msg, yesTxt, noTxt, yesFun, noFun, title) {
        if (title != undefined) {
            $('#confirmModal .modal-title').html(title)
        } else {
            $('#confirmModal .modal-title').html(translations.confirm)
        }

        if (msg != undefined) {
            $('#confirmMsg').html(msg)
        } else {
            $('#confirmMsg').html(translations.confirm_action_plead)
        }

        if (yesTxt == null) {
            $('#confirmYes').hide()
        } else if (yesTxt != undefined) {
            $('#confirmYes').html(yesTxt).show()
            $('#confirmYes').attr('title', yesTxt)
        } else {
            $('#confirmYes').html('Ok').show()
            $('#confirmYes').attr('title', translations.ok)
        }

        if (noTxt == null) {
            $('#confirmNo').hide()
        } else if (noTxt != undefined) {
            $('#confirmNo').html(noTxt).show()
            $('#confirmNo').attr('title', noTxt)
        } else {
            $('#confirmNo').html(translations.cancel).show();
            $('#confirmNo').attr('title', translations.cancel)
        }

        $('#confirmModal').modal('show')

        $('#confirmModal .close').on('click', function () {
            $('#confirmNo').trigger('click')
        })

        $('#confirmNo').unbind('click')
        $('#confirmNo').on('click', function () {
            if (noFun != undefined) {
                noFun()
            }
            $('#confirmModal').modal('hide')
        })

        $('#confirmYes').unbind('click')
        $('#confirmYes').on('click', function () {
            if (yesFun != undefined) {
                yesFun()
            }
            $('#confirmModal').modal('hide')
        })
    }

    function getCustomCode(data, functionName, resolve) {
        if (Window.Custom == null) {
            var getUrl = window.location;
            var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
            var fileUrl = baseUrl + 'scripts/customfunctions.js';
            API.getJSFile(fileUrl)
                .done(function (response) {
                    runCustomCode(data, functionName, resolve);
                })
                .fail(function (e) {
                    showExceptionErrorMessage(e);
                });
        }
        else {
            runCustomCode(data, functionName, resolve);
        }
    }

    function runCustomCode(data, functionName, resolve) {
        try {
            Custom.eventData = data;
            Custom.resolve = resolve;

            var fn = functionName.split("(")
            var params = fn[1].split(/[()]+/);
            var paramsArray = params[0].split("',");
            for (var i = 0; i < paramsArray.length; i++) {
                var itemId = paramsArray[i].substring(1, paramsArray[i].length);
                paramsArray[i] = itemId;
                if (i === paramsArray.length - 1) {
                    itemId = paramsArray[i].substring(0, paramsArray[i].length - 1);
                    paramsArray[i] = itemId;
                }
            }
            Custom[fn[0]].apply(undefined, paramsArray);
        }
        catch (e) {
            showErrorMessage('Method not found or method parsing is failed due to parameters');
            throw e;
        }
    }

    function getDocumentUrls(resolve) {
        var data = {
            "Type": "Personal",
            "InstanceId": ""
        }

        API.service('records/getdocumentsurl', data)
            .done(function (response) {
                var results = JSON.parse(response);
                resolve(results);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function cleanupDocs(results) {
        var data = {
            "docsUrl": results
        }

        API.service('records/cleanUpTempDocuments', data)
            .done(function (response) {
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getInstanceTitle(id) {
        var query = {
            "type": "SELECT",
            "entity": "Instance",
            "resultSet": ["Title"],
            "filters": [
                {
                    "column": "Id",
                    "operator": "equal",
                    "value": id,
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "title", "descending": false }]
        }
        API.service('records', query)
            .done(function (response) {
                var result = JSON.parse(response);
                document.title = result[0].Title;
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    var app = function () {
        this.getProcessAsync = getProcessAsync;
        this.getProcessHtml = getProcessHtml;
        this.api = window.API || {};
        this.responsible = getResponsible;
        this.getProcess = getProcess;
        this.getMyInstances = getMyInstances;
        this.getJournalHistoryForInstance = getJournalHistoryForInstance;
        this.getTasks = getTasks;
        this.addInstance = addInstance;
        this.executeEvent = executeEvent;
        this.getInstanceDetails = getInstanceDetails;
        this.getPhases = getPhases;
        this.getParameterByName = getParameterByName;
        this.getRoles = getRoles;
        this.searchProcess = searchProcess;
        this.addProcesses = addProcesses;
        this.searchCases = searchCases;
        this.showExceptionErrorMessage = showExceptionErrorMessage;
        this.getResponsibleName = getResponsibleName;
        this.showConfirmMessageBox = showConfirmMessageBox;
        this.showWarningMessage = showWarningMessage;
        this.showSuccessMessage = showSuccessMessage;
        this.showErrorMessage = showErrorMessage;
        this.getDocumentUrls = getDocumentUrls;
        this.cleanupDocs = cleanupDocs;
        this.logJsError = logJsError;
        this.showInformationMessage = showInformationMessage;
        this.showTaskWithNotePopup = showTaskWithNotePopup;
        this.showTaskWithNoteFullPopup = showTaskWithNoteFullPopup;
        this.hideDocumentWebpart = hideDocumentWebpart;
        this.getUserRoles = getUserRoles;
        this.addChild = addChild;
        this.getInstanceTitle = getInstanceTitle;
        this.setTexts = setTexts;
        this.getKeyValue = getKeyValue;
        this.getAllProcesses = getAllProcesses;
        this.hideUpdateMajorRevision = hideUpdateMajorRevision;
    };

    setTexts();
    getResponsibleName();
    return window.App = new app;
}(window));

$(document).ready(function () {
    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
    });

    promise.then(function () {
        App.getResponsibleName();
    }, function (err) {
        console.log(err); // Error: "It broke"
    });

    $('#searchCase').keypress(function (e) {
        if (e.which == 13) {
            var searchText = $('#searchCase').val();
            window.location.href = window.location.origin + '/instance/search?query=' + searchText;
        }
    });
});
