
// Task Libray
(function (window) {

    // get tasks
    function getTasks(entityName, onlyMyTasks, onlyOpenInstance, instanceId, resolve, reject) {
        // base query get all tasks
        var query = {
            "type": "SELECT",
            "entity": entityName,
            "resultSet": ["EventId", "Responsible", "InstanceId", "EventTitle", "Due", "SimulationId", "GraphId", "IsPending", "IsExecuted", "CanExecute", "ResponsibleName", "Description", "IsUIEvent", "UIEventValue", "EventType", "[Type]", "[Case]", "CaseLink", "CaseTitle"],
            "filters": new Array(),
            "order": [{ "column": "IsPending", "descending": true }, { "column": "Due", "descending": true }, { "column": "IsEnabled", "descending": true }, { "column": "IsExecuted", "descending": false },
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
        if (instanceId != null && instanceId > 0) {
            var instanceFilter = {
                "column": "InstanceId",
                "operator": "equal",
                "value": instanceId,
                "valueType": "int",
                "logicalOperator": 'and'
            }
            query.filters.push(instanceFilter);
        }

        // get my tasks only
        if (onlyMyTasks) {
            var myTasksFilter = {
                "column": "Responsible",
                "operator": "equal",
                "value": '$(loggedInUserId)',
                "valueType": "string",
                "logicalOperator": "and"
            };
            query.filters.push(myTasksFilter);
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

    // set single Instance Filter
    function showSingleInstanceFilter() {
        $('#singleInstanceFilters').show();
    }

    // hide these columns
    // columns will be header names
    function hideTableColumns(columns) {
        $.each(columns, function (index, value) {
            $('#tasksTable thead').children().first().children().each(function (index, elem) {
                if (value == $(elem).children('span[data-key]').attr('data-key').trim().toLowerCase()) {
                    $('#tasksTable td:nth-child(' + (index + 1) + '),#tasksTable th:nth-child(' + (index + 1) + ')').hide();
                }
            });
        })
    }

    // tasks html
    function tasksHtml(id, response, showCaseInfo) {
        var result = JSON.parse(response);
        var user = window.App.user;
        var ownList = "";
        var othersList = "";
        var doneList = "";
        if (result.length === 0)
            ownList = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
        else {
            for (i = 0; i < result.length; i++) {
                if (i == 2) console.log(result[i]);
                if (result[i].Responsible != user.Id) {
                    if (result[i].IsExecuted == false) othersList += getTaskHtmlForOthersTasks(result[i], showCaseInfo);
                } else {
                    ownList += getTaskHtml(result[i], showCaseInfo);
                }
            }
        }
        $("#" + id).html("").append(ownList);
        $('.others-tasks').html('').append(othersList);
        $('.done-tasks').html('').append(doneList);

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

        // bind execute event
        $('button[name="execute"]').on('click', function (e) {
            Swal.fire({
                title: 'Er du sikker på at du vil markere denne aktivitet som udført?',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Udført'
            }).then((result) => {
                if (result.value) {
                    var elem = $(e.currentTarget);

                    var eventId = elem.attr('id');
                    var eventType = elem.attr('eventType');
                    var taskId = elem.attr('taskId');
                    var instanceId = elem.attr('instanceId');
                    var graphId = elem.attr('graphId');
                    var simulationId = elem.attr('simulationId');
                    var uievent = elem.attr('uievent');
                    var data = { taskId: taskId, instanceId: instanceId, graphId: graphId, simulationId: simulationId, eventId: eventId };

                    if (eventType === "TasksWNote") {
                        App.showTaskWithNotePopup(data, elem, showCaseInfo, uievent);
                    } else {
                        App.executeEvent(data, showCaseInfo, uievent);
                    }

                    e.preventDefault();
                }
            });
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
                                if (eventType === "TasksWNote") {
                                    App.showTaskWithNotePopup(data, elem, showCaseInfo, uievent);
                                }
                                else {
                                    App.executeEvent(data, showCaseInfo, uievent);
                                }

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
                $(this).find('iframe').attr('src', 'http://localhost:65466/dynamicform.html?loadWithXML=true');
            })

            $('#dcrFormIframeModal').on('hidden.bs.modal', function () {
                $(this).find('iframe').attr('src', '');
            })

            $('#dcrFormIframe').load(function () {
                $('.loading').hide();
            });
            e.preventDefault();
        })
    }

    function getTaskHtmlForDoneTasks(item, isFrontPage) {
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
            instanceLink = "../Instance?id=" + item.InstanceId;
        }

        returnHtml = '<tr isfrontPage="' + isFrontPage + '" name="description" class="trStyleClass">' +
            '<td class="' + taskStatusCssClass + '">' + taskStatus + '</td >' +
            '<td><a href="' + instanceLink + '">' + item.EventTitle + '</a></td>' +
            '<td>' + (item.Due == null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td></tr>';

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
            instanceLink = "../Instance?id=" + item.InstanceId;
        }

        returnHtml = '<tr isfrontPage="' + isFrontPage + '" name="description" class="trStyleClass">' +
            '<td class="' + taskStatusCssClass + '">' + taskStatus + '</td >' +
            '<td><a href="' + instanceLink + '">' + item.EventTitle + '</a></td>' +
            '<td>' + item.ResponsibleName.substr(0, 1).toUpperCase() + item.ResponsibleName.substr(1) + '</td>' +
            '<td>' + (item.Due == null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td></tr>';

        if (item.Description !== '' && !isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%">' + item.Description + '</td></tr>';
        } else if (item.Description !== '' && isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%"><p>' + translations.Description + " : " + item.Description + '</td></tr>' +
                '<tr class="showMe" style="display:none"><td></td><td colspan="100%"> ' + translations.CaseNo + ' :  <a target="_blank" href="' + caseLink + '">' + caseTitle + '</a> </td></tr>';
        }
        return returnHtml;
    }

    // html of each task
    function getTaskHtml(item, isFrontPage) {
        var returnHtml = '';
        var taskStatusCssClass = 'includedTask';
        var taskStatus = '&nbsp;';
        if (item.IsPending) {
            taskStatus = "<img src='../Content/Images/priorityicon.svg' height='16' width='16'/>";
        } else if (item.IsExecuted) {
            taskStatus = 'b';
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
        var instanceLink = "#";
        if (isFrontPage) {
            instanceLink = "../Instance?id=" + item.InstanceId;
        }

        returnHtml = '<tr isfrontPage="' + isFrontPage + '" name="description" class="trStyleClass">' +
            '<td class="' + taskStatusCssClass + '">' + taskStatus + '</td >' +
            '<td><a href="' + instanceLink + '">' + item.EventTitle + '</a></td>' +
            '<td>' + (item.Due == null ? '&nbsp;' : moment(new Date(item.Due)).format('L LT')) + '</td>' +
            '<td>';
        if (item.CanExecute && item.Type.toLowerCase() !== "form") {
            returnHtml += '<button';
            if (item.IsUIEvent) {
                returnHtml += ' uievent="' + item.UIEventValue + '"';
            }
            returnHtml += ' type="button" taskid="' + item.EventId + '" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" instanceid="'
                + item.InstanceId + '" id="' + item.EventId + '" name="execute" value="execute" class="taskExecutionButton">Udført</button><div class="title" style="display: none;">' + item.EventTitle + '</div> <div class="description" style="display: none;">' + item.Description + '</div>';
        }
        else if (item.CanExecute && item.Type.toLowerCase() == "form") {
            returnHtml += '<button title="Open" eventType= "' + item.EventType + '" graphid="' + item.GraphId + '" simulationid="' + item.SimulationId + '" token="' + item.Token + '" eventId="' + item.EventId + '" instanceid="' + item.InstanceId + '" id="openDcrForm" class="btn btn-info taskExecutionButton" name="btnDcrFormServer"><i class="fas fa-external-link-alt"></i></button><div class="title" style="display: none;">' + item.EventTitle + '</div> <div class="description" style="display: none;">' + item.Description + '</div>';
        }
        returnHtml += '</td>' + '</tr>';

        if (item.Description !== '' && !isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%">' + item.Description + '</td></tr>';
        }
        else if (item.Description !== '' && isFrontPage) {
            returnHtml += '<tr class="showMe" style="display:none"><td></td><td colspan="100%"><p>' + translations.Description + " : " + item.Description + '</td></tr>' +
                '<tr class="showMe" style="display:none"><td></td><td colspan="100%"> ' + translations.CaseNo + ' :  <a target="_blank" href="' + caseLink + '">' + caseTitle + '</a> </td></tr>';

        }
        return returnHtml;
    }

    // mus library
    var task = function () {
        this.getTasks = getTasks;
        this.showSingleInstanceFilter = showSingleInstanceFilter;
        this.hideTableColumns = hideTableColumns;
        this.tasksHtml = tasksHtml;
    };
    return window.Task = new task;
}(window));

// on document ready initialization
$(document).ready(function () {

});