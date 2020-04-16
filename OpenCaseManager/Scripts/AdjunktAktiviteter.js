(function (window) {
    function tasksHtml(id, response, showCaseInfo, onlyMyTasks, currentUserId) {
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
                        ownList += getTaskHtml(result[i], showCaseInfo, currentUserId);
                    }
                }
                else {
                    ownList += getTaskHtml(result[i], showCaseInfo, currentUserId);
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
                App.executeEvent(data, showCaseInfo, uievent, null, null, true);
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
            '<td><a href="' + instanceLink + '">' + item.Title + '</a></td>' +
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
        var taskStatus = (item.IsPending) ? "<img src='../Content/Images/priorityicon.svg' height='16' width='16'/>" : '&nbsp;';

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
            '<td><a href="' + instanceLink + '">' + item.EventTitle + '</a></td>' +
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

    function getTaskHtml(item, showCaseInfo, currentUserId) {
        var returnHtml = '';
        var taskStatusCssClass = 'includedTask';
        var taskStatus = '&nbsp;';
        if (item.IsPending) {
            taskStatus = '<i class="fas fa-exclamation-circle pending-icon"></i>';
        } else if (item.IsExecuted) {
            taskStatus = "<img src='../Content/Images/check.png' />";;
            taskStatusCssClass = 'executedTask';
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
            '<td class="' + taskStatusCssClass + '">' + taskStatus + '</td >' +
            '<td><a class="' + overDueCssClass + (item.ParentId !== null ? 'subprocess-child' : '') + '" href="../Instance?id=' + item.InstanceId + '">' +
            item.Title +
            '</a></td>' +
            '<td  class="' + overDueCssClass + '" >' + (item.Due === null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td>' +
            '<td><a href="../Instance?id=' + item.InstanceId + '" class="linkStyling responsibleSelectOptions">' + item.SamAccountName + '</a></td>' +
            '<td>';
        if (item.IsEnabled && item.IsIncluded && item.Responsible == currentUserId && item.Type.toLowerCase() !== "form" && item.Type.toLowerCase() !== "subprocess") {
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

        return returnHtml;
    }

    function setWarningDelayDoIt() {
        $('#trTasksWarning').show();
    }

    function showChildInstancesX(AcadreResult) {
        var childId = App.getParameterByName("id", window.location.href);
        var query = {
            "type": "SELECT",
            "entity": "ChildInstances('$(loggedInUserId)')",
            "resultSet": ["*"],
            "filters": new Array(),
            "order": [{ "column": "Pending", "descending": true }, { "column": "PendingAndEnabled", "descending": true }]

        }

        var whereChildIdMatchesFilter = {
            "column": "ChildId",
            "operator": "equal",
            "value": childId,
            "valueType": "int",
            "logicalOperator": "and"
        };
        query.filters.push(whereChildIdMatchesFilter);

        API.service('records', query)
            .done(function (response) {
                showChildInstances(AcadreResult, response);
            })
            .fail(function (e) {
                App.showErrorMessage(e.responseJSON.ExceptionMessage);
            });
    }

    function displayChildName(response) {
        var result = JSON.parse(response);
        var childName = (result[0] == undefined) ? 'Intet barn at finde' : ((result[0].Name == null) ? "Intet navn på barn" : result[0].Name);
        $("#childName").html("").append(childName);
        $('head title', window.parent.document).text(childName);
    }

    function showAdjunktAktiviteter(result, OCMresponse) {
        var OCMresult = JSON.parse(OCMresponse);
        var list = "";
        if (result.length === 0 && OCMresult.length === 0) {
            list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
        } else {
            for (i = 0; i < OCMresult.length; i++) {
                list += getChildInstanceHtml(OCMresult[i], 0, null);
            }
            for (i = 0; i < result.length; i++) {
                list += getChildInstanceHtml(result[i], 1, OCMresult);
            }
        }
        $("#childInstances").html("").append(list);
        setClosedInstancesToFadedAndMoveDown();

        $('#tableChildInstances').sortable({
            // DIV selector before table
            divBeforeTable: '',
            // DIV selector after table
            divAfterTable: '',
            // initial sortable column
            initialSort: '',
            // ascending or descending order
            initialSortDesc: false,
            // language
            locale: locale,
            // use table array
            tableArray: []
        });
    }

    function getTasks() {
        window.location.reload();
    }

    var adjunktaktiviteter = function () {
        this.getTasks = getTasks;
        this.tasksHtml = tasksHtml;
    };


    return window.AdjunktAktiviteter = new adjunktaktiviteter;
}(window));

$(document).ready(function () {
    App.hideDocumentWebpart();

    var userId = App.getParameterByName("userid", window.location.href);
    var adjunktId = App.getParameterByName("adjunktid", window.location.href);

    $('#tableAdjunktAktiviteter').sortable({
        // DIV selector before table
        divBeforeTable: '',
        // DIV selector after table
        divAfterTable: '',
        // initial sortable column
        initialSort: '',
        // ascending or descending order
        initialSortDesc: false,
        // language
        locale: locale,
        // use table array
        tableArray: []
    });

    if (userId == null && adjunktId == null) { //default behaviour, showing all activities current user is responsible for as well as all activities under the corresponding adjunkt
        API.service('records/GetMineAktiviteterNoInput', null)
            .done(function (response) {
                API.service('records/GetCurrentUserId', null).done(function (response2) {
                    userId = JSON.parse(response2);
                    AdjunktAktiviteter.tasksHtml("adjunktAktiviteter", response, false, false, userId);
                }).fail(function (e) {
                    App.showErrorMessage(e.responseJSON.ExceptionMessage);
                });;
            })
            .fail(function (e) {
                App.showErrorMessage(e.responseJSON.ExceptionMessage);
            });
    } else {
        var query = { "AdjunktId": adjunktId, "UserId": userId };
        API.service('records/GetMineAktiviteter', query)
            .done(function (response) {
                AdjunktAktiviteter.tasksHtml("adjunktAktiviteter", response, false, false, userId);
            })
            .fail(function (e) {
                App.showErrorMessage(e.responseJSON.ExceptionMessage);
            });
    }
});