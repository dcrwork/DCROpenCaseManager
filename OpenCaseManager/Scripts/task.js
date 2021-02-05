
// Task Libray
(function (window) {

    // get tasks
    function getTasks(entityName, onlyMyTasks, onlyOpenInstance, instanceId, resolve, reject) {
        // base query get all tasks
        var query = {
            "type": "SELECT",
            "entity": entityName,
            "resultSet": ["EventId", "TrueEventId", "Responsible", "InstanceId", "EventTitle", "Due", "SimulationId", "GraphId", "IsPending", "IsExecuted", "CanExecute", "ResponsibleName", "[Description]", "IsUIEvent", "UIEventValue", "EventType", "[Type]", "[Case]", "CaseLink", "CaseTitle", "IsOverDue", "DaysPassedDue", "Modified", "NotApplicable", "ParentId", "Roles"],
            "filters": new Array(),
            "order": [
                { "column": "NotApplicable", "descending": false },
                { "column": "ActualIsPending", "descending": true },
                { "column": "ActualIsEnabled", "descending": true },
                { "column": "ActualIsExecuted", "descending": false },
                { "column": "COALESCE(ParentId, TrueEventId)", "descending": false },
                { "column": "ParentId", "descending": false },
                { "column": "IsPending", "descending": true },
                { "column": "Due", "descending": false },
                { "column": "IsEnabled", "descending": false },
                { "column": "IsExecuted", "descending": false },
                { "column": "EventTitle", "descending": false }]
        }

        // get tasks for only open instances
        if (onlyOpenInstance) {
            var openInstanceTasks = {
                "column": "InstanceIsOpen",
                "operator": "equal",
                "value": "1",
                "valueType": "int",
                "logicalOperator": "and"
            };
            query.filters.push(openInstanceTasks);
        }

        // get tasks for selected instance
        if (instanceId !== null && instanceId > 0) {
            var instanceFilter = {
                "column": "InstanceId",
                "operator": "equal",
                "value": instanceId,
                "valueType": "int",
                "logicalOperator": 'and'
            }
            query.filters.push(instanceFilter);
        }

        // get data
        API.service('records', query)
            .done(function (response) {
                resolve(response);
            })
            .fail(function (e) {
                reject(e);
            });
    }

    function getJournalHistory(response) {
        var result = JSON.parse(response);
        var doneList = '';
        for (i = 0; i < result.length; i++) {
            doneList += getTaskHtmlForDoneTasks(result[i], false);
        }
        $('.done-tasks').html('').append(doneList);
    }

    // set single Instance Filter
    function showSingleInstanceFilter() {
        $('#singleInstanceFilters').show();
    }

    // hide these columns
    // columns will be header names
    function hideTableColumns(columns) {
        $.each(columns, function (index, value) {
            $('#tasksTable thead').children().first().children().each(function (index, elem) {
                if ($(elem).children('span[data-key]').length > 0) {
                    if (value === $(elem).children('span[data-key]').attr('data-key').trim().toLowerCase()) {
                        $('#tasksTable td:nth-child(' + (index + 1) + '),#tasksTable th:nth-child(' + (index + 1) + ')').hide();
                    }
                }
            });
        })
    }

    // tasks html
    function tasksHtml(id, response, showCaseInfo, onlyMyTasks) {
        var result = JSON.parse(response);
        var user = window.App.user;
        var ownList = "";
        var othersList = "";
        if (result.length === 0) {
            ownList = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        } else {
            for (i = 0; i < result.length; i++) {
                if (onlyMyTasks) {
                    if (result[i].Responsible !== user.Id) {
                        if (result[i].IsExecuted === false)
                            othersList += getTaskHtmlForOthersTasks(result[i], showCaseInfo);
                    }
                    else {
                        ownList += getTaskHtml(result[i], showCaseInfo);
                    }
                }
                else {
                    ownList += getTaskHtml(result[i], showCaseInfo);
                }
            }
        }

        $("#" + id).html("").append(ownList);
        $('.others-tasks').html('').append(othersList);

        // expand/collapse description
        $('tr[name="description"]').on('click', function (e) {
            var element = $(e.currentTarget);
            var isFrontPage = element.attr('isfrontPage');
            if (isFrontPage === 'false') {
                if (element.next().hasClass('showMe') && e.target.localName !== 'img' && e.target.localName !== 'button') {
                    element.next().toggle();
                    element.next().next('tr.showMe').toggle();
                }
            }
            else {
                if (element.next().hasClass('showMe') && e.target.localName !== 'img' && e.target.localName !== 'a' && e.target.localName !== 'button') {
                    element.next().toggle();
                    element.next().next('tr.showMe').toggle();
                }
            }
        });

        var elem;

        // bind execute event
        $('button[name="execute"]').on('click', async function (e) {
            var elem = $(e.currentTarget);

            var eventId = elem.attr('id');
            var eventType = elem.attr('eventType');
            var taskId = elem.attr('taskId');
            var instanceId = elem.attr('instanceId');
            var graphId = elem.attr('graphId');
            var simulationId = elem.attr('simulationId');
            var uievent = elem.attr('uievent');
            var title = elem.next('.title').html();
            var trueEventId = elem.attr('trueEventId');
            var Modified = elem.attr('Modified');

            // Check event is current - begin
            var query = {
                "type": "SELECT",
                "entity": "[InstanceEvents]",
                "resultSet": ["NextDelay", "NextDeadline", "Modified", "NeedToSetTime"],
                "filters": new Array(),
                "order": []
            }

            var whereInstanceIdMatchesFilter = {
                "column": "instanceId",
                "operator": "equal",
                "value": instanceId,
                "valueType": "int",
                "logicalOperator": "and"
            };
            query.filters.push(whereInstanceIdMatchesFilter);
            var whereEventIdMatchesFilter = {
                "column": "eventId",
                "operator": "equal",
                "value": eventId,
                "valueType": "string",
                "logicalOperator": "and"
            };
            query.filters.push(whereEventIdMatchesFilter);
            var whereModifiedMatchesFilter = {
                "column": "Modified",
                "operator": "equal",
                "value": Modified,
                "valueType": "datetime",
                "logicalOperator": "and"
            };
            query.filters.push(whereModifiedMatchesFilter);
            var result1 = await API.service('records', query);
            var EventOK = JSON.parse(result1);
            if (EventOK.length === 0) {
                App.showWarningMessage('Page not current - press F5 to refresh');
                return;
            }
            if (EventOK[0].NeedToSetTime === 1) {
                var setTimeData = {
                    "instanceId": instanceId,
                    "time": new Date().toUTCString()
                }

                var responseSetTime = await API.service('services/settime', setTimeData);

                App.showWarningMessage('Page not current - press F5 to refresh - time');
                return;
            }
            // Check event is current - end
            var data = {
                taskId: taskId,
                instanceId: instanceId,
                graphId: graphId,
                simulationId: simulationId,
                eventId: eventId,
                title: title,
                trueEventId: trueEventId,
                Modified: Modified
            };
            if (eventType === "TasksWNote") {
                App.showTaskWithNotePopup(data, elem, showCaseInfo, uievent);
            } else if (eventType === "TasksWNoteFull") {
                App.showTaskWithNoteFullPopup(data, elem, showCaseInfo, uievent);
            } else {
                App.executeEvent(data, showCaseInfo, uievent);
            }
            e.preventDefault();
        });

        // bind execute event
        $('ul[name="notApplicable"]').on('click', function (e) {
            var elem = $(e.currentTarget);

            var eventId = elem.attr('id');
            var eventType = elem.attr('eventType');
            var taskId = elem.attr('taskId');
            var instanceId = elem.attr('instanceId');
            var graphId = elem.attr('graphId');
            var simulationId = elem.attr('simulationId');
            var uievent = elem.attr('uievent');
            var trueEventId = elem.attr('trueEventId');
            var title = elem.attr('itemTitle');
            var Modified = elem.attr('Modified');

            $('#notApplicableEventTitle').text(title);
            $('#notApplicableModal').modal('toggle');
            $('#activitySelected').html(elem[0].outerHTML);
            $('#message-text-not-applicable').val('');

            // base query get all tasks
            e.preventDefault();
        });

        // bind execute event
        $('#buttonNotApplicable').on('click', async function (e) {
            var elem = $('#activitySelected').children('ul');

            var eventId = elem.attr('id');
            var eventType = elem.attr('eventType');
            var taskId = elem.attr('taskId');
            var instanceId = elem.attr('instanceId');
            var graphId = elem.attr('graphId');
            var simulationId = elem.attr('simulationId');
            var uievent = elem.attr('uievent');
            var trueEventId = elem.attr('trueEventId');
            var title = elem.attr('itemTitle');
            var Modified = elem.attr('Modified');
            var text = $('#message-text-not-applicable').val();

            // get data
            API.service('services/NotApplicable', { title: title, taskId: taskId, instanceId: instanceId, graphId: graphId, simulationId: simulationId, eventId: eventId, trueEventId: trueEventId, note: text })
                .done(function (response) {
                    var instanceId = App.getParameterByName("id");
                    App.getTasks(instanceId);
                    $('#notApplicableModal').modal('hide');
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e)
                });

            // base query get all tasks
            e.preventDefault();
        });

        // open dcr form
        $('button[name="btnDcrFormServer"').click(function (e) {
            var elem = $(e.currentTarget);

            var eventId = elem.attr('eventId');
            var eventType = elem.attr('eventType');
            var taskId = elem.attr('taskId');
            var instanceId = elem.attr('instanceId');
            var graphId = elem.attr('graphId');
            var simulationId = elem.attr('simulationId');
            var uievent = elem.attr('uievent');
            var token = elem.attr('token');
            var data = { taskId: taskId, instanceId: instanceId, graphId: graphId, simulationId: simulationId, eventId: eventId };

            $('.loading').show();
            $('#dcrFormEventTitle').html(elem.next('.title').html());

            var query = {
                "eventId": eventId,
                "instanceId": instanceId
            }

            API.service('services/GetReferXmlByEventId', query)
                .done(function (response) {
                    formObj = {
                        DCRFormXML: response,
                        DCRFormToken: token,
                        DCRFormCallBack: "DCRFormCallBack",
                        DCRFormCancelCallBack: "DCRFormCancelCallBack",
                        DCRFormIframeID: "dcrFormIframe"
                    };

                    window.DCRFormCancelCallBack = function () {
                        $('#dcrFormIframeModal').modal('toggle');
                    }

                    window.DCRFormCallBack = function (xml) {
                        var query = {
                            "eventId": eventId,
                            "instanceId": instanceId,
                            "referXml": xml
                        }

                        // get data
                        API.service('services/MergeReferXmlWithMainXml', query)
                            .done(function (response) {
                                App.getTasks(instanceId, true);
                                App.getInstanceDetails(data.instanceId);
                                App.getJournalHistoryForInstance(data.instanceId);
                                App.getPhases(data.instanceId);
                                $('#dcrFormIframeModal').modal('toggle');
                            })
                            .fail(function (e) {
                                App.showExceptionErrorMessage(e)
                            });
                    }

                    $('#dcrFormIframeModal').modal('toggle');
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e)
                });

            $('#dcrFormIframeModal').on('shown.bs.modal', function () {
                $(this).find('iframe').attr('src', window.FormServer.dcrFormServerUrl + '/dynamicform.html?loadWithXML=true');
            });

            $('#dcrFormIframeModal').on('hidden.bs.modal', function () {
                $(this).find('iframe').attr('src', '');
            });

            $('#dcrFormIframe').load(function () {
                $('.loading').hide();
            });
            e.preventDefault();
        })

        if (showCaseInfo) {
            $('#addTasks').hide();
        }
        setWarningDelay(instanceId);
    }

    async function setWarningDelay(instanceId) {
        var query = {
            "type": "SELECT",
            "entity": "[Instance]",
            "resultSet": ["NextDelay", "Id", "Datediff(millisecond,getUTCDate(),NextDelay) as DIFF", "getUTCDate() as UTC"],
            "filters": new Array(),
            "order": []
        }

        var whereInstanceIdMatchesFilter = {
            "column": "id",
            "operator": "equal",
            "value": instanceId,
            "valueType": "int",
            "logicalOperator": "and"
        };
        query.filters.push(whereInstanceIdMatchesFilter);
        var result1 = await API.service('records', query);
        var Delay = JSON.parse(result1);
        if (Delay.length > 0) {
            if (Delay[0].NextDelay !== null) {
                if (Delay[0].DIFF > 0) {
                    //alert('set delay ' + Delay[0].DIFF);
                    setTimeout('setWarningDelayDoIt();', Delay[0].DIFF);
                }
            }
        }
    }

    function getTaskHtmlForDoneTasks(item, isFrontPage) {
        var returnHtml = '';
        var taskStatusCssClass = 'includedTask';

        var caseTitle = item.Title;
        var caseLink = '#';

        var instanceLink = "#";
        if (isFrontPage) {
            instanceLink = "../AdjunktInstance?id=" + item.InstanceId;
        }

        returnHtml = '<tr isfrontPage="' + isFrontPage + '" name="description" class="trStyleClass">' +
            '<td class="' + taskStatusCssClass + '"></td >' +
            '<td style="cursor:default">' + item.Title + '</td>' +
            '<td>' + item.ResponsibleName.substr(0, 1).toUpperCase() + item.ResponsibleName.substr(1) + '</td>' +
            '<td>' + moment(new Date(item.EventDate)).format('L LT') + '</td></tr>';

        if (item.Description !== '' && !isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%">' + item.Description + '</td></tr>';
        } else if (item.Description !== '' && isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%"><p>' + translations.Description + " : " + item.Description + '</td></tr>' +
                '<tr class="showMe" style="display:none"><td></td><td colspan="100%"> ' + translations.CaseNo + ' :  <a target="_blank" href="' + caseLink + '">' + caseTitle + '</a> </td></tr>';
        }
        return returnHtml;
    }

    function getTaskHtmlForOthersTasks(item, isFrontPage) {
        var returnHtml = '';
        var taskStatusCssClass = 'includedTask';
        var taskTitle = (item.IsPending) ? "Afventende" : '&nbsp;';
        var taskStatus = (item.IsPending) ? "<img src='../Content/Images/Standard/priorityicon.svg' height='16' width='16'/>" : '&nbsp;';

        var caseTitle = item.CaseTitle;
        var caseLink = '#';

        if (item.CaseLink !== null) {
            caseLink = item.CaseLink;
        }
        if (item.Case !== null) {
            caseTitle = item.Case + ' - ' + item.CaseTitle;
        }
        var instanceLink = "#";
        if (isFrontPage) {
            instanceLink = "../AdjunktInstance?id=" + item.InstanceId;
        }

        returnHtml = '<tr isfrontPage="' + isFrontPage + '" name="description" class="trStyleClass' + (item.NotApplicable === true ? ' notapplicable' : '') + ' ">' +
            '<td class="' + taskStatusCssClass + '">' + taskStatus + '</td >' +
            '<td style="cursor:default">' + item.EventTitle + '</td>' +
            '<td>' + (item.Due === null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td>' +
            '<td><a href="#" class="linkStyling responsibleSelectOptions" itemResponsible="' + item.ResponsibleName + '" itemTitle="' + item.EventTitle + '" itemInstanceId="' + item.InstanceId + '" itemEventId="' + item.TrueEventId + '" changeResponsibleFor="activity">' + item.ResponsibleName.substr(0, 1).toUpperCase() + item.ResponsibleName.substr(1) + '</a></td></tr>';
            

        if (item.Description !== '' && !isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%">' + item.Description + '</td></tr>';
        } else if (item.Description !== '' && isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%"><p>' + translations.Description + " : " + item.Description + '</td></tr>' +
                '<tr class="showMe" style="display:none"><td></td><td colspan="100%"> ' + translations.CaseNo + ' :  <a target="_blank" href="' + caseLink + '">' + caseTitle + '</a> </td></tr>';
        }
        return returnHtml;
    }

    // html of each task
    function getTaskHtml(item, showCaseInfo) {
        var returnHtml = '';
        var taskStatusCssClass = 'includedTask';
        var taskStatus = '&nbsp;'; 
        var taskTitle = '&nbsp;';
        
        if (item.IsPending) {
            taskStatus = "<img src='../Content/Images/Standard/priorityicon.svg' height='16' width='16'/>";
            taskTitle = 'Afventende';
        } else if (item.IsExecuted) {
            taskStatus = "<img src='../Content/Images/Standard/check.png' />";;
            taskStatusCssClass = 'executedTask';
            taskTitle = 'Udført';
        }
        var caseTitle = item.CaseTitle;
        var caseLink = '#';

        if (item.CaseLink !== null) {
            caseLink = item.CaseLink;
        }
        if (item.Case !== null) {
            caseTitle = item.Case + ' - ' + item.CaseTitle;
        }

        var overDueCssClass = "";
        if (item.IsOverDue === 1) {
            if (item.DaysPassedDue === 1) {
                overDueCssClass = "yellowState";
            }
            else if (item.DaysPassedDue > 1) {
                overDueCssClass = "redState";
            }
        }
        returnHtml = '<tr name="description" class="trStyleClass' + (item.NotApplicable === true ? ' notapplicable' : '') + ' ">' +
            '<td class="' + taskStatusCssClass + '" title="' + taskTitle + '" > ' + taskStatus + '</td > ' +
            '<td style="cursor:default" class="' + overDueCssClass + (item.ParentId !== null ? 'subprocess-child' : '') + '" >' +
            item.EventTitle +
            '</td>' +
            '<td  class="' + overDueCssClass + '" >' + (item.Due === null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td>' +
            '<td>' + item.Roles + '</td>' +
            '<td><a href="#" class="linkStyling responsibleSelectOptions" itemResponsible="' + item.ResponsibleName + '" itemTitle="' + item.EventTitle + '" itemInstanceId="' + item.InstanceId + '" itemEventId="' + item.TrueEventId + '" changeResponsibleFor="activity">' + item.ResponsibleName + '</a></td>' +
            '<td>';
        if (item.CanExecute && item.Type.toLowerCase() !== "form" && item.Type.toLowerCase() !== "subprocess") {
            returnHtml += '<div class="btn-group"><button';
            if (item.IsUIEvent) {
                returnHtml += ' uievent="' + item.UIEventValue + '"';
            }
            returnHtml += ' type="button" taskid="' + item.EventId + '" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
                + item.InstanceId + '" id="' + item.EventId + '" trueEventId="' + item.TrueEventId + '" Modified="' + item.Modified + '" name="execute" value="execute" class="btn btn-default" data-toggle="modal" data-target="#executeTaskModal">' + translations.Execute + '</button><div class="title" style="display: none;">' + item.EventTitle + '</div> <div class="description" style="display: none;">' + item.Description + '</div><button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown"><span class="caret"></span></button><ul itemTitle="' + item.EventTitle + '" taskid="' + item.EventId + '" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
                + item.InstanceId + '" id="' + item.EventId + '" trueEventId="' + item.TrueEventId + '" Modified="' + item.Modified + '" class="dropdown-menu notApplicableUl" role="menu" name="notApplicable"><li><a href="#">' + translations.NotApplicable + '</a></li></ul>';
        }
        else if (item.CanExecute && item.Type.toLowerCase() === "form") {
            returnHtml += '<button title="Open" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" token="' + item.Token + '" eventId="' + item.EventId + '" trueEventId="' + item.TrueEventId + '" Modified="' + item.Modified + '" instanceid="' + item.InstanceId + '" id="openDcrForm" class="btn taskExecutionButton" name="btnDcrFormServer"><i class="fas fa-external-link-alt"></i></button><div class="title" style="display: none;">' + item.EventTitle + '</div> <div class="description" style="display: none;">' + item.Description + '</div>';
        }
        returnHtml += '</td></div>' + '</tr>';

        if (item.Description !== '' && !showCaseInfo) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%">' + item.Description + '</td></tr>';
        }
        else if (item.Description !== '' && showCaseInfo) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%"><p>' + translations.Description + " : " + item.Description + '</td></tr>' +
                '<tr class="showMe" style="display:none"><td></td><td colspan="100%"> ' + translations.CaseNo + ' :  <a target="_blank" href="' + caseLink + '">' + caseTitle + '</a> </td></tr>';

        }
        return returnHtml;
    }

    // save tasks note
    function saveTasksNote(eventId, instanceId, comment, isHtml, resolve, reject) {
        if (comment === "") {
            return alert("Comment Field Is Mandatory !");
        }
        var query = {
            eventId: eventId,
            instanceId: instanceId,
            note: comment,
            isHtml: isHtml
        };

        // get data
        API.service('records/SetTasksWNoteComment', query)
            .done(function (response) {
                resolve(response);
            })
            .fail(function (e) {
                reject(e);
            });

    }

    // add task
    function addTask(label, role, description) {
        var query = {
            "label": label,
            "role": role,
            "description": description,
            "instanceId": instanceId
        };

        // get data
        API.service('services/AddTask', query)
            .done(function (response) {
                App.getTasks(instanceId);
                $('#addNewCaseTask').modal('toggle');
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    // add task modal html
    function addTaskHtml(instanceId) {
        if (instanceId > 0) {
            var promise = new Promise(function (resolve, reject) {
                var query = {
                    "type": "SELECT",
                    "entity": "Instance",
                    "resultSet": ["GraphId"],
                    "filters": [
                        {
                            "column": "Id",
                            "operator": "equal",
                            "value": instanceId,
                            "valueType": "int"
                        }
                    ],
                    "order": [{ "column": "title", "descending": false }]
                }

                API.service('records', query)
                    .done(function (response) {
                        resolve(response);
                    })
                    .fail(function (e) {
                        App.showExceptionErrorMessage(e);
                    });
            });

            promise.then(function (response) {
                var graphId = JSON.parse(response)[0].GraphId;

                var promise2 = new Promise(function (resolve1, reject) {
                    App.getRoles(graphId, resolve1);
                });

                promise2.then(function (response) {
                    var roles = response;
                    if (roles.length === 0) {
                        $('#rolesContent').hide();
                    }
                    $.each(roles, function (index, role) {
                        $('#roles').append('<option value="' + role.title + '">' + role.title + '</option>');
                    });
                }, function (e) {
                    App.showExceptionErrorMessage(e);
                });

            }, function (e) {
                App.showExceptionErrorMessage(e);
            });

            $('#taskAdd').on('click', function () {
                var label = $('#taskCaseLabel').val();
                var roles = $('#roles').val();
                var description = $('#taskCaseDescription').val();

                if (roles === null)
                    roles = "";

                Task.addTask(label, roles, description);
            });
        }
    }

    // mus library
    var task = function () {
        this.getTasks = getTasks;
        this.showSingleInstanceFilter = showSingleInstanceFilter;
        this.hideTableColumns = hideTableColumns;
        this.tasksHtml = tasksHtml;
        this.getJournalHistory = getJournalHistory;
        this.saveTasksNote = saveTasksNote;
        this.addTask = addTask;
        this.addTaskHtml = addTaskHtml;
        this.InstanceId = 0;
    };

    return window.Task = new task;
}(window));

// on document ready initialization
$(document).ready(function () {

});

function setWarningDelayDoIt() {
    $('#trTasksWarning').show();
}
