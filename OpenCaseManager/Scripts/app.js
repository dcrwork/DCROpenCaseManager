var Responsible = null;

(function (window) {

    // api functions
    function getProcess(getRole, showOnFrontPage) {
        var query = {
            "type": "SELECT",
            "entity": "Process",
            "filters": [
                {
                    "column": "Status",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "and"
                }
            ],
            "resultSet": ["Id", "GraphId", "Title", "OnFrontPage"],
            "order": [{ "column": "title", "descending": false }]
        }

        if (showOnFrontPage) {
            query.filters.push(
                {
                    "column": "OnFrontPage",
                    "operator": "equal",
                    "value": true,
                    "valueType": "boolean",
                    "logicalOperator": "none"
                }
            );
        }

        API.service('records', query)
            .done(function (response) {
                if (getRole) {
                    renderData("instanceProcesses", response, getProcessHtml)
                    var graphId = $('#instanceProcesses').find(":selected").val();
                    getRoles(graphId);
                }
                else {
                    renderData("processes", response, getProcessesHtml)
                    registerEditProcessEvent();
                    registerCancelProcessEvent();
                    registerUpdateProcessEvent();
                    registerDeleteProcessEvent();
                    registerRefreshProcessEvent();
                    registerGotoDCRGraphsEvent();
                    registerCheckOnFrontPage();
                }
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
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

    function addInstance(title, graphId, userRoles) {

        var data = {
            title: title,
            graphId: graphId,
            userRoles: userRoles
        }

        API.service('records/addInstance', data)
            .done(function (response) {

                var result = JSON.parse(response);
                var instanceId = result;

                API.service('services/InitializeGraph', { instanceId: instanceId, graphId: graphId })
                    .done(function (response) {
                        getMyInstances();
                        getTasks();
                    })
                    .fail(function (e) {
                        showExceptionErrorMessage(e);
                    });

            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function executeEvent(data, isFrontPage, uiEvent, isMUS) {
        if (uiEvent != null) {
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
        else {
            API.service('services/ExecuteEvent', { taskId: data.taskId, instanceId: data.instanceId, graphId: data.graphId, simulationId: data.simulationId, eventId: data.eventId })
                .done(function (response) {
                    if (isMUS == null) {
                        if (isFrontPage) {
                            getMyInstances();
                            getTasks();
                        }
                        else {
                            getTasks(data.instanceId);
                            getInstanceDetails(data.instanceId);
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
                            getPhases(data.instanceId);
                        }
                    } else {
                        MUS.musDetails(MUS.showMUS);
                    }
                });
        }
    }

    function getInstanceDetails(id) {
        var query = {
            "type": "SELECT",
            "entity": "Instance",
            "resultSet": ["Title", "CaseNoForeign", "CaseLink", "CurrentPhaseNo", "Description"],
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
                renderData("instanceDetails", response, getInstanceHtml)
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
            "order": []
        }

        API.service('records', query)
            .done(function (response) {
                Task.getJournalHistory(response);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getTasks(instanceId) {
        // get all tasks of all my instance
        var entityName = "InstanceTasks('$(loggedInUserId)')";
        var onlyMyTasks = false;
        var onlyTasksFromOpenInstances = false;
        var showCaseInfo = false;

        if (instanceId == null) {
            onlyMyTasks = true;
            onlyTasksFromOpenInstances = true;
            instanceId = 0;
            showCaseInfo = true;
        }
        else {
            if (window.localStorage.getItem('taskStatusDD') == "1") {
                entityName = "InstanceTasksAllEnabled('$(loggedInUserId)')";
            }
            if (window.localStorage.getItem('responsibleDD') == "1") {
                onlyMyTasks = true;
            }
        }

        var promise = new Promise(function (resolve, reject) {
            Task.getTasks(entityName, onlyMyTasks, onlyTasksFromOpenInstances, instanceId, resolve, reject);
        });
        promise.then(function (response) {
            Task.tasksHtml('tasks', response, showCaseInfo);
            if (instanceId == 0) {
                Task.hideTableColumns(['responsible']);
            }
            else {
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

    function setResponsible() {
        API.service('services/getResponsible', {})
            .done(function (response) {
                window.App.user = response[0];
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getTranslations(locale) {
        var getUrl = window.location;
        var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
        var fileUrl = baseUrl + 'scripts/translations/' + locale + '.js';

        API.getJSFile(fileUrl)
            .done(function (response) {
                setTexts();
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function getRoles(graphId, resolve) {
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
                            renderUserRolesData('userRoles', response, roles, getUserRoles);
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

    function addProcesses(searchText) {
        var selectedProcess = new Array();
        $('.process:checked').each(function (index, checkbox) {
            var id = $(this).attr('id');
            var title = $('input[name="addProcess"][Id="' + id + '"]').val();
            var data = { graphId: id, title: title };
            selectedProcess.push(data);
        });

        if (selectedProcess.length > 0) {
            API.service('records/addProcess', selectedProcess)
                .done(function (response) {
                    var count = JSON.parse(response);
                    if (count == 1)
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

    function updateProcess(processId, processTitle, showOnFrontPage) {
        var data = { processId: processId, processTitle: processTitle, processStatus: true, showOnFronPage: showOnFrontPage };

        API.service('records/updateProcess', data)
            .done(function (response) {
                getProcess(false);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    function deleteProcess(processId, processTitle, showOnFrontPage) {
        var data = { processId: processId, processTitle: processTitle, processStatus: false, showOnFronPage: showOnFrontPage };

        API.service('records/updateProcess', data)
            .done(function (response) {
                getProcess(false);
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
            "resultSet": ["Name"],
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

    function openDCRGraphsURL(graphId) {
        API.serviceGET('services/getDCRGraphsURL')
            .done(function (response) {
                window.open(response + '/Tool?id=' + graphId);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
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

    // public functions
    function getParameterByName(name, url) {
        if (!url) url = window.location.href;
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
            var processId = e.currentTarget.attributes.processId.value;
            var processTitle = $('input[processId="' + processId + '"]').val();
            var showOnFrontPage = $(e.currentTarget).prop('checked');
            updateProcess(processId, processTitle, showOnFrontPage);
        });
    }

    function registerEditProcessEvent() {
        $('button[name="editProcess"]').on('click', function (e) {
            var processId = e.currentTarget.attributes.processId.value;
            $('span[processId="' + processId + '"]').toggle();
            $('input[processId="' + processId + '"]').toggle();
            $('button[name="editProcess"][processId="' + processId + '"]').toggle();
            $('button[name="updateProcess"][processId="' + processId + '"]').toggle();
            $('button[name="cancelUpdateProcess"][processId="' + processId + '"]').toggle();
            $('button[name="deleteProcess"][processId="' + processId + '"]').toggle();
            $('button[name="refreshProcess"][processId="' + processId + '"]').toggle();
            e.preventDefault();
        });
    }

    function registerCancelProcessEvent() {
        $('button[name="cancelUpdateProcess"]').on('click', function (e) {
            var processId = e.currentTarget.attributes.processId.value;
            $('span[processId="' + processId + '"]').toggle();
            $('input[processId="' + processId + '"]').toggle();
            $('button[name="editProcess"][processId="' + processId + '"]').toggle();
            $('button[name="updateProcess"][processId="' + processId + '"]').toggle();
            $('button[name="cancelUpdateProcess"][processId="' + processId + '"]').toggle();
            $('button[name="deleteProcess"][processId="' + processId + '"]').toggle();
            $('button[name="refreshProcess"][processId="' + processId + '"]').toggle();
            e.preventDefault();
        });
    }

    function registerUpdateProcessEvent() {
        $('button[name="updateProcess"]').on('click', function (e) {
            var processId = e.currentTarget.attributes.processId.value;
            var processTitle = $('input[type="text"][processId="' + processId + '"]').val();
            var showOnFrontPage = $('input[type="checkbox"][processId="' + processId + '"]').prop('checked');
            updateProcess(processId, processTitle, showOnFrontPage);
            e.preventDefault();
        });
    }

    function registerDeleteProcessEvent() {
        $('button[name="deleteProcess"]').on('click', function (e) {
            App.showConfirmMessageBox(translations.ProcessDeleteMessage, translations.Yes, translations.No, function () {

                var processId = e.currentTarget.attributes.processId.value;
                var processTitle = $('a[processId="' + processId + '"]').text();
                var showOnFrontPage = $('input[type="checkbox"][processId="' + processId + '"]').prop('checked');

                deleteProcess(processId, processTitle, showOnFrontPage);

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

    function showTaskWithNotePopup(data, elem, isFrontPage, uievent, isMUS) {

        var eventTitle = elem.next('.title').html();
        var eventDescription = elem.next().next('.description').html();
        var taskWNoteModal = $('#TasksWNote');
        taskWNoteModal.modal('show');
        taskWNoteModal.find('.TasksWNoteheading span').text(eventTitle);
        taskWNoteModal.find('.modal-body p').html(eventDescription);
        taskWNoteModal.find('.commentbox').val('');

        taskWNoteModal.find('.TasksWNoteBtn').on('click', function (e) {
            App.executeEvent(data, isFrontPage, uievent, isMUS);
            taskWNoteModal.modal('hide');
        });
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
        if (item.Description != null && item.Description != '')
            Instruction.setText(item.Description);
        else
            Instruction.hideWebPart();
    }

    function getProcessHtml(item) {
        return "<option value= " + item.GraphId + ">" + item.Title + "</option>";
    }

    function getProcessesHtml(item) {
        var returnHtml = '';
        returnHtml = '<tr class="trStyleClass">' +
            '<td style="display:none;" > ' + item.Id + '</td >' +
            '<td> ' + item.GraphId + ' </td>' +
            '<td> ' +
            '<a href="../../process?id=' + item.Id + '" processId="' + item.Id + '">' + item.Title + '</a>' +
            '<input processId="' + item.Id + '" style="display:none" type="text" name="processTitle" class="form-control" value="' + item.Title + '"/>' +
            '</td>' +
            '<td> <input processId="' + item.Id + '" type="checkbox" class="bootstrap-toggle" data-size="mini" data-onstyle="info" data-style="color" data-on="' + translations.On + '" data-off="' + translations.Off + '" ';
        if (item.OnFrontPage) {
            returnHtml += " checked ";
        }
        returnHtml += '/> ' +
            '</td>' +
            '<td>';
        returnHtml += '<button processId="' + item.Id + '" type="button" name="editProcess" value="editProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Edit + '" src="../../content/images/edit.png"></button> ';
        returnHtml += '<button style="display:none" processId="' + item.Id + '" type="button" name="updateProcess" value="updateProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Edit + '" src="../../content/images/edit.png"></button> ';
        returnHtml += '<button style="display:none" processId="' + item.Id + '" type="button" name="cancelUpdateProcess" value="cancelUpdateProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Cancel + '" src="../../content/images/cancel.png"></button> ';
        returnHtml += '<button  processId="' + item.Id + '" graphId="' + item.GraphId + '" type="button" name="refreshProcess" value="refreshProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Update + '" src="../../content/images/update.png"></button> ';
        returnHtml += '<button processId="' + item.Id + '" type="button" name="deleteProcess" value="deleteProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.Delete + '" src="../../content/images/delete.png"></button> ';
        returnHtml += '<button processId="' + item.Id + '" graphId="' + item.GraphId + '" type="button" name="gotoProcess" value="gotoProcess" class="btn btn-info taskExecutionBtn"><img title="' + translations.GotoGraph + '" src="../../content/images/goto.png"></button> ';
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
            html += "<img src='../content/images/dash.png' /></li> ";
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
        console.log("data", response);
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

        for (i = 0; i < result.length; i++) {
            $('#multi-select-' + result[i].Id + '').multiselect();
        }
    }

    function getUserRoles(role, users) {
        var returnHtml = '';
        returnHtml = '<div class="form-group clearfix" style="width:100%"><label class="labelStyling">' + role.title + '</label>' +
            '<select name="multi-select" class="form-control formFieldStyling" userId="' + role.title + '" id="multi-select-' + role.title + '">';
        returnHtml += '<option value="0">' + translations.SelectResponsible + '</option>';
        if (users.length > 0) {
            $.each(users, function (index, user) {
                returnHtml += '<option value="' + user.Id + '">' + user.Name + '</option>';
            });
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
        returnHtml = '<tr class="trStyleClass"><td> <a href="../Instance?id=' + item.Id + '">' + item.Title + '</a></td><td>' + item.CaseNoForeign + '</td><td>' + item.Responsible + '</td><td>' + item.IsOpen + '</td></tr>';
        return returnHtml;
    }

    function setTexts() {
        $('[data-locale]').each(function (index, element) {
            if ($(this).attr('data-apply') === 'attribute') {
                $(this).attr($(this).attr('data-attribute'), translations[$(this).attr('data-key')]);
            }
            else if ($(this).attr('data-apply') === 'text') {
                $(this).text(translations[$(this).attr('data-key')]);
            }
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
        var message = exception.responseText;
        if (exception.responseJSON != null) {
            try {
                message = JSON.parse(exception.responseJSON);
            }
            catch (e) {
                message = JSON.parse(exception.responseText);
            }
        }
        if (message != null) {

            new Noty({
                type: 'error',
                theme: 'mint',
                layout: 'topRight',
                text: message,
                timeout: 5000,
                container: '.custom-container'
            }).show();
        }
        else {
            new Noty({
                type: 'error',
                theme: 'mint',
                layout: 'topRight',
                text: exception.responseJSON.ExceptionMessage,
                timeout: 5000,
                container: '.custom-container'
            }).show();
        }
    }

    function showErrorMessage(message) {
        new Noty({
            type: 'error',
            theme: 'mint',
            layout: 'topRight',
            text: message,
            timeout: 5000,
            container: '.custom-container'
        }).show();
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
            Custom[fn[0]].apply(undefined, (new Function("return [" + params[0] + "];")()))
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

    var app = function () {
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
        this.hideDocumentWebpart = hideDocumentWebpart;
        this.setResponsible = setResponsible;
    };

    getTranslations(locale);

    return window.App = new app;
}(window));

$(document).ready(function () {
    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
        App.setResponsible();
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