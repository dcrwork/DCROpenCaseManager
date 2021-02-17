var instanceId = getParameterByName("id", window.location.href);
var delay = null;
var CountDown = null;
var t = null;
$(document).ready(function () {
    var webPortalType = "LaunchInstance";
    //getMyTestCase(instanceId)
    getPhases(instanceId);
    getTasksList(instanceId, true);
    getAIRoboticEvents(instanceId, getRecommendation, showExceptionErrorMessage, true);
    
    $('#tasksTable thead > tr:eq(1) > th:eq(4)').remove();
});

function countdown() {

    CountDown = (function ($) {

        var GuiTimer = $('#demo');
        var GuiPause = $('#pause');
        var GuiResume = $('#resume').hide();
        
        var count;

        function cddisplay() {
            GuiPause.show();
            GuiResume.hide();
            GuiTimer.html(count);
        }

        var Start = function () {
            // starts countdown
            cddisplay();
            if (count === 0) {
                // time is up
                console.log("Countdown reached 0");
                $('button[name="executeNonUserRoles_0"]').click();
            } else {
                count--;
                t = setTimeout(Start, 1000);
            }
        }

        var Pause = function () {
            // pauses countdown
            clearTimeout(t);
            t = null;
            GuiPause.hide();
            GuiResume.show();
        }

        var Reset = function () {
            // resets countdown
            Pause();
            count = delay;
            cddisplay();
            Start();
            //return true;;
        }

        var Clear = function () {
            Pause();
            GuiTimer.css('color', 'red')
            GuiTimer.html(0);
        }

        GuiPause.hide();
        GuiResume.hide();

        

        return {
            Pause: Pause,
            Start: Start,
            Reset: Reset,
            Clear: Clear
        };
    })(jQuery);

     $('#pause').on('click', CountDown.Pause);
     $('#resume').on('click', CountDown.Start);

    // ms

}

function getRolesOnGraphDDChange() {
    $('#graphIdDropdown').on('change', function () {
        const graphId = $(this).find('option:selected').attr('id');
        getRolesForTestCases(graphId);
    });
}

function getPhases(InstanceId) {
    getInstancePhases(InstanceId, getPhasesCallback, showExceptionErrorMessage);
}

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

function getInstancePhases(instanceId, callback, errorCallback) {
    API.serviceGET('records/getInstancePhases?id=' + instanceId)
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

function getAIRoboticEvents(instanceId, callback, errorCallback, isAIRoboticEvent) {
    API.serviceGET('records/getAIRoboticEvents?id=' + instanceId)
        .done(function (response) {
            var results = {
                response: response,
                instanceId: instanceId
            };
            callback(results, isAIRoboticEvent);
        })
        .fail(function (e) {
            errorCallback(e);
        });
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
    else {
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

async function getMyTestCase(id) {

    var response = await API.serviceGET('records/getMyTestCase?id=' + id);
    var result = JSON.parse(response);
    delay = parseInt(result[0].Delay);
    countdown();
    CountDown.Reset()
    $('#demo').css('color', '#333');
    console.log(response);
}

// get tasks
function getTasksList(instanceId, isAIRoboticEvent = false) {
    // get data
    API.serviceGET('records/getTasks?id=' + instanceId)
        .done(function (response) {
            tasksHtml('tasks', response, false, false, isAIRoboticEvent);
            if (instanceId == 0) {
                hideTableColumns(['responsible']);
            }
            else {
                if (window.location.href.toLocaleLowerCase().indexOf("MyActivities".toLocaleLowerCase()) > 0)
                    showSingleInstanceFilter();
            }
        })
        .fail(function (e) {
            showExceptionErrorMessage(e);
        });
}

// set single Instance Filter
function showSingleInstanceFilter() {
    $('#singleInstanceFilters').show();
}

async function execute(e, isAIRoboticEvent) {
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
    var query = isAIRoboticEvent ? {
        "type": "SELECT",
        "entity": "InstanceAIRoboticEvents",
        "resultSet": ["NextDelay", "NextDeadline", "Modified", "NeedToSetTime"],
        "filters": new Array(),
        "order": []
    } : {
            "type": "SELECT",
            "entity": "InstanceEvents",
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
    var result1 = await API.service('records/anonymous', query);
    var EventOK = JSON.parse(result1);
    if (EventOK.length === 0) {
        showWarningMessage('Page not current - press F5 to refresh');
        return;
    }
    if (EventOK[0].NeedToSetTime === 1) {
        var setTimeData = {
            "instanceId": instanceId,
            "time": new Date().toUTCString()
        }

        var responseSetTime = await API.service('services/settime', setTimeData);

        showWarningMessage('Page not current - press F5 to refresh - time');
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
        showTaskWithNotePopup(data, elem, false, uievent);
    } else if (eventType === "TasksWNoteFull") {
        showTaskWithNoteFullPopup(data, elem, false, uievent);
    } else {
        executeEvent(data, false, uievent, null, null, null, isAIRoboticEvent);
    }
    e.preventDefault();
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

// tasks html
function tasksHtml(id, response, showCaseInfo, onlyMyTasks, isAIRoboticEvent) {
    var result = JSON.parse(response);
    var ownList = "";
    var othersList = "";
    if (result.length === 0) {
        ownList = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        $('#demo').css('color', 'red');
        countdown();
        CountDown.Clear();
    } else {
        var canExecute = false;
        var index = 0;
        for (i = 0; i < result.length; i++) {
            var rolesToTest = result[i].RoleToTest.split(',');
            var roleExist = rolesToTest.indexOf(result[i].Roles) < 0;
            if (onlyMyTasks) {
                if (result[i].Responsible !== user.Id) {
                    if (result[i].IsExecuted === false)
                        othersList += getTaskHtmlForOthersTasks(result[i], showCaseInfo);
                }
                else {

                    ownList += getTaskHtml(result[i], showCaseInfo, index, rolesToTest);
                    index = roleExist ? index + 1 : index;
                }
            }
            else {
                if (((result[i].IsEnabled === true && result[i].IsPending === true)) && roleExist) canExecute = true;
                ownList += getTaskHtml(result[i], showCaseInfo, index, rolesToTest);
                index = roleExist ? index + 1 : index;
            }
        }
        if (canExecute) {
            // if (isAIRoboticEvent) {
            // if (delay == null) {
                getMyTestCase(instanceId);
            // }
            
            // }
        }
        else {
            countdown();
            CountDown.Clear();
        }
    }
    $('[data-col="' + id + '"]').remove();
    $('#responsibleHead').html(translations.Role);
    $('#myActivitiesHead').attr('data-key', translations.Activities).html(translations.Activities);
    $('#addTasks').css('display', 'none');
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
                var instanceId = getParameterByName("id");
                getTasksList(instanceId);
                $('#notApplicableModal').modal('hide');
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
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
                            getTasksList(data.instanceId);
                            getPhases(data.instanceId);
                            $('#dcrFormIframeModal').modal('toggle');
                        })
                        .fail(function (e) {
                            showExceptionErrorMessage(e)
                        });
                }

                $('#dcrFormIframeModal').modal('toggle');
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
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

    // bind execute event
    $('button[name="execute"]').on('click', async function (e) {
        execute(e, false);
    });

    // bind execute event
    $('button[name="executeAIRobot"]').on('click', async function (e) {
        execute(e, true);
    });

    
    $('button[name="executeNonUserRoles_0"]').on('click', async function (e) {
        execute(e, false);
    });

    if (showCaseInfo) {
        $('#addTasks').hide();
    }
    setWarningDelay(instanceId);

}

async function executeEvent(data, isFrontPage, uiEvent, isMUS, type, adjunkt, isAIRoboticEvent) {
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
                            //getMyInstances();
                            getTasksList();
                        }
                        else if (adjunkt) {
                            AdjunktAktiviteter.getTasks(data.instanceId, true);
                            getPhases(data.instanceId);
                        } else {
                            getTasksList(data.instanceId, isAIRoboticEvent);
                            getAIRoboticEvents(data.instanceId, getRecommendation, showExceptionErrorMessage, isAIRoboticEvent);
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
                            getTasksList();
                        }
                        else {
                            getTasksList(data.instanceId);
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
            showErrorMessage(globalEvents[0].Message);
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
    getTasksList(instanceId);
    getPhases(instanceId);
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


// html of each task
function getTaskHtml(item, showCaseInfo, index, rolesToTest) {
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
        '<td>';

    if (item.CanExecute && item.Type.toLowerCase() !== "form" && item.Type.toLowerCase() !== "subprocess") {
        var display = '';
        var name = "execute";
        if (rolesToTest.indexOf(item.Roles) < 0) {
            name = "executeNonUserRoles_" + index;
            display = 'style="visibility: hidden;"';
        }
        returnHtml += '<div class="btn-group" ' + display + ' ><button';
        if (item.IsUIEvent) {
            returnHtml += ' uievent="' + item.UIEventValue + '"';
        }
        returnHtml += ' type="button" taskid="' + item.EventId + '" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
            + item.InstanceId + '" id="' + item.EventId + '" trueEventId="' + item.TrueEventId + '" Modified="' + item.Modified + '" name="' + name + '" value="execute" class="btn btn-default" data-toggle="modal" data-target="#executeTaskModal">' + translations.Execute + '</button><div class="title" style="display: none;">' + item.EventTitle + '</div> <div class="description" style="display: none;">' + item.Description + '</div><button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown"><span class="caret"></span></button><ul itemTitle="' + item.EventTitle + '" taskid="' + item.EventId + '" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
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

function getRecommendation(result, isAIRoboticEvent) {
    var response = JSON.parse(result.response)

    for (var i = 0; i < response.length; i++) {
        var html = getRecommendationHtml(response[i], i);
        $(".recommendation" + (i + 1) + "Col").css("display", "block");
        $("#recommendationHead" + (i + 1)).html(response[i].EventTitle);
        $("#recommendation" + (i + 1)).html("").append(html);

    }
    $(".recommendation" + (response.length + 1) + "Col").css("display", "none");
    $("#recommendation" + (response.length + 1)).html("");

     // bind execute event
    
    $('button[name="execute"]').on('click', async function (e) {
        execute(e, false);
    });
}

function getRecommendationHtml(item, index) {
    var returnHtml = '';

    returnHtml = '<tr name="description" class="trStyleClass ">' +
        '<td style="cursor:default" class="subprocess-child" >' +
        item.Description +
        '</td>' +
        ' <td>';

    returnHtml += '<div class="btn-group" style="min-width:115px;"><button style="border-radius: 5px !important;"';
    returnHtml += ' type="button" taskid="' + item.EventId + '" eventType= "' + item.EventType + '"  graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
        + item.InstanceId + '" id="' + item.EventId + '" trueEventId="' + item.TrueEventId + '"  Modified="' + item.Modified + '" name="executeAIRobot" value="execute" class="btn btn-default" data-toggle="modal" data-target="#executeTaskModal">' + translations.Understood + '</button><div class="title" style="display: none;">' + item.EventTitle + '</div>';
    + '</div></td>' + '</tr>';
    return returnHtml;
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

async function setWarningDelay(instanceId) {
    var result1 = await API.serviceGET('records/getWarningDelay?id=' + instanceId);
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