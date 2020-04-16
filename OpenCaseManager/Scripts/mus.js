
// MUS Libray
(function (window) {
    var musGraphId = 0;

    // get graph Id to initialise mus against that graph
    function getMUSGraphId(callback) {
        API.service('services/getmusgraphId')
            .done(function (response) {
                graphIdForMUS = JSON.parse(response);
                setMUSGraphId(graphIdForMUS);
                makeCallback(callback, response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    // get all mus details
    function musDetails(callback, sortColumn, sortDirection) {
        if (sortColumn == null) {
            sortColumn = 'Department';
            sortDirection = false;
            var orderByName = { "column": 'FullName', "descending": false }
        }

        var query = {
            "type": "SELECT",
            "entity": "MUS",
            "resultSet": ["Id", "Username", "FullName", "InstanceTitle", "CaseNoForeign", "CaseLink", "CurrentPhaseNo", "SimulationId", "InstanceId", "ManagerId", "Department"],
            "filters": [
                {
                    "column": "ManagerId",
                    "operator": "equal",
                    "value": '$(loggedInUserId)',
                    "valueType": "string",
                    "logicalOperator": "and"
                },
                {
                    "column": "Year",
                    "operator": "equal",
                    "value": ($('#multi-select-year').val() == null ? (new Date()).getFullYear() : $('#multi-select-year').val()),
                    "valueType": "int"
                }
            ],
            "order": [{ "column": sortColumn, "descending": sortDirection }]
        }

        if (orderByName != null)
            query.order.push(orderByName);

        API.service('records', query)
            .done(function (response) {
                makeCallback(callback, response);
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    // add MUS for a user
    function addMUS(title, graphId, userName) {

        // first get the MUS Graph Id
        var promise = new Promise(function (resolve, reject) {
            var data = {
                title: title,
                graphId: graphId,
                userRoles: [],
                employee: userName
            };
            Core.addInstance(data, resolve, App.showExceptionErrorMessage);
        });
        promise.then(function (data) {

            var promise = new Promise(function (resolve, reject) {
                addMUSRoles(data, resolve);
            });
            promise.then(function (data) {

                var promise = new Promise(function (resolve, reject) {
                    Core.initializeGraph(data.instanceId, data.graphId, resolve, App.showExceptionErrorMessage);
                    addCustomInstanceAttributes(data.instanceId, data.employee);
                });
                promise.then(function () {
                    musDetails(showMUS);
                }, function (e) {
                    App.showExceptionErrorMessage(e);
                });

            }, function (e) {
                App.showExceptionErrorMessage(e);
            });
        }, function (e) {
            App.showExceptionErrorMessage(e);
        });
    }

    // show MUS Details 
    function showMUS(response) {
        // first get the MUS Graph Id
        var promise = new Promise(function (resolve, reject) {
            getMUSGraphId(resolve);
        });

        promise.then(function () {

            var result = JSON.parse(response)
            var list = "";
            if (result.length === 0)
                list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
            else {
                for (i = 0; i < result.length; i++) {
                    list += getMUSItemHtml(result[i], i, result.length);
                }
            }
            $("#mus").html("").append(list);
            // bind add mus btton with events
            bindAddMUSButton();
            // get phases with each instance
            $('td[instanceId]').each(function (index, element) {
                var instanceId = $(element).attr('instanceId');
                var promise = new Promise(function (resolve, reject) {
                    Core.getInstancePhases(instanceId, resolve, reject)
                });

                promise.then(function (phasesResponse) {
                    var response = phasesResponse.response;
                    var instanceId = phasesResponse.instanceId;

                    // get UI Events with each instance
                    var promise = new Promise(function (resolve, reject) {
                        Core.getUIEvents(resolve, reject, instanceId);
                    });
                    promise.then(function (uiEventsResponse) {
                        var phases = JSON.parse(response);
                        var html = '<div class="phasesMainBlock" style=""><ul class="phasesUl"; id="phasesList' + instanceId + '">';
                        // phases html
                        var currentPhaseIndex = -1;
                        $(phases).each(function (index, phase) {
                            var style = '', cssClass = '';
                            if (phase.PhaseId == phase.CurrentPhase) {
                                cssClass = "currentPhase";
                                currentPhaseIndex = index;
                            }
                            html += '<li data-id="' + phase.PhaseId + '" class="' + cssClass + '"><h4>' + phase.Title + '</h4></li>';
                        });
                        html += '</ul></div>';
                        $('#phases' + instanceId).html(html);

                        // ui events icon html
                        var uiIconHtml = '<div class="actionBlockMUS">';
                        $(uiEventsResponse).each(function (index, result) {
                            if (result.UIEventCssClass == null || result.UIEventCssClass == '0' || result.UIEventCssClass == '')
                                result.UIEventCssClass = 'fa-check-square';
                            uiIconHtml += '<i name="executeUIEvent" title="' + result.EventTitle + '" eventType="' + result.EventType + '" taskid="' + result.EventId + '" graphid="' + result.GraphId + '" simulationid="' + result.SimulationId + '" instanceid="'
                                + result.InstanceId + '" id="' + result.EventId + '" uievent="' + result.UIEventValue + '" isUIEvent="' + result.IsUIEvent + '" class="spanMUS fa ' + result.UIEventCssClass + '"></i><div class="title" style="display: none;">' + result.EventTitle + '</div> <div class="description" style="display: none;">' + result.Description + '</div>';
                        });
                        uiIconHtml += '</div>';
                        $('#phases' + instanceId).next().html(uiIconHtml);

                        $('#phasesList' + instanceId).slick({
                            centerMode: true,
                            centerPadding: '60px',
                            infinite: false,
                            variableWidth: true,
                            responsive: [
                                {
                                    breakpoint: 768,
                                    settings: {
                                        arrows: false,
                                        centerMode: false,
                                        centerPadding: '40px'
                                    }
                                },
                                {
                                    breakpoint: 480,
                                    settings: {
                                        arrows: false,
                                        centerMode: false,
                                        centerPadding: '40px'
                                    }
                                }
                            ]
                        })
                        $('#phasesList' + instanceId).slick('slickGoTo', currentPhaseIndex)
                    }, function (err) {
                        App.showExceptionErrorMessage(err);
                    });

                }, function (e) {
                    App.showExceptionErrorMessage(e);
                });
            });
        }, function (e) {
            App.showExceptionErrorMessage(e);
        });
    }

    // private functions
    // call a callback if asked
    function makeCallback(callback, response) {
        if (callback != null)
            callback(response);
    }

    // add Employee attributes
    function addCustomInstanceAttributes(instanceId, employee) {
        var data = {
            instanceId: instanceId,
            employee: employee,
        }

        API.service('records/addInstanceCustomAttributes', data)
            .done(function (response) {
            })
            .fail(function (e) {
                showExceptionErrorMessage(e);
            });
    }

    // add MUS Roles
    function addMUSRoles(data, callback) {
        var graphId = data.graphId;
        var graphId = data.graphId;

        // first get the MUS Graph Id
        var promise = new Promise(function (resolve, reject) {
            App.getRoles(graphId, resolve);
        });
        promise.then(function (response) {
            var roles = [];
            $.each(response, function (index, value) {
                roles.push(value.title);
            });

            var query = {
                instanceId: data.instanceId,
                roles: roles,
                employee: data.employee
            }
            API.service('records/AddMUSRole', query)
                .done(function (response) {
                    callback(data);
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e);
                });
        }, function (e) {
            App.showExceptionErrorMessage(e);
        });
    }

    // retain MUS Graph Id
    function setMUSGraphId(graphIdForMUS) {
        musGraphId = graphIdForMUS;
        if (MUS != null)
            MUS.musGraphId = graphIdForMUS;
    }

    // get each MUS Item html
    function getMUSItemHtml(item) {
        var returnHtml = '';
        if (item.InstanceId === null) {
            var date = new Date();
            var year = date.getFullYear();
            returnHtml = '<tr class="trStyleClass">' +
                '<td width="1%" class="includedTask">&nbsp;</td >' +
                '<td width="10%">' + (item.Department == null ? '&nbsp' : item.Department) + '</td >' +
                '<td width="15%">' + item.FullName + '</td >' +
                '<td width="20%"><button type= "button" title= "MUS ' + year + ' ' + item.Username + '" graphid= "' + graphIdForMUS + '" responsible= "' + item.Username + '" name="createInstance" value="execute" class="btn btn-info">' + translations.Create + '</button> </td>' +
                '<td width="15%">&nbsp;</td><td width="12%">&nbsp;</td>' + '</tr>';
        }
        else {
            returnHtml = '<tr class="trStyleClass">' +
                '<td  width="1%" class="includedTask">&nbsp;</td >' +
                '<td width="10%">' + (item.Department == null ? '&nbsp' : item.Department) + '</td >' +
                '<td width="15%">' + item.FullName + '</td >' +
                '<td width="20%" instanceId=' + item.InstanceId + ' id="phases' + item.InstanceId + '"></td> ' +
                '<td width="15%">&nbsp;</td> ' +
                '<td width="12%"><a href="../Instance?id=' + item.InstanceId + '">' +
                item.InstanceTitle +
                '</a></td>';
            returnHtml += '</tr>';
        }
        return returnHtml;
    }

    // bind create mus button
    function bindAddMUSButton() {
        $('button[name="createInstance"]').on('click', function (e) {
            var title = e.currentTarget.attributes.title.value;
            var graphId = e.currentTarget.attributes.graphid.value;
            var createMUSFor = e.currentTarget.attributes.responsible.value;
            var data = { title: title, graphId: graphId, createMUSFor: createMUSFor };
            addMUS(data.title, data.graphId, data.createMUSFor);
            e.preventDefault();
        });
    }

    // mus library
    var mus = function () {
        this.getMUSGraphId = getMUSGraphId;
        this.musDetails = musDetails;
        this.musGraphId = musGraphId;
        this.showMUS = showMUS;
        this.addMUS = addMUS;
    };
    return window.MUS = new mus;
}(window));

// on document ready initialization
$(document).ready(function () {

    // get all years from MUS for YEAR dropdown
    var query = {
        "type": "SELECT",
        "entity": "MUS",
        "resultSet": ["Year"],
        "filters": [
            {
                "column": "ManagerId",
                "operator": "equal",
                "value": '$(loggedInUserId)',
                "valueType": "string"
            }
        ],
        "order": [{ "column": "Year", "descending": true }]
    }
    API.service('records', query)
        .done(function (response) {
            var years = JSON.parse(response);

            var yearNumbers = years.map(function (obj) { return obj.Year; });
            var date = new Date();
            var year = date.getFullYear();
            yearNumbers.push(year);
            yearNumbers = yearNumbers.filter(function (v, i) { return yearNumbers.indexOf(v) == i; });
            yearNumbers.sort();
            yearNumbers.reverse();
            var html = '';
            $(yearNumbers).each(function (index, year) {
                html += '<option value="' + year + '">' + year + '</option>';
            });
            $('#multi-select-year').html(html);
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });

    // get MUS
    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
    });
    promise.then(function (result) {
        MUS.musDetails(MUS.showMUS);
        $('#addNewDocumentText').text(translations.PersonalDocument);
    }, function (e) {
        App.showExceptionErrorMessage(e);
    });

    // on year change, get MUS of that year
    $('#multi-select-year').change(function () {
        $('.sort-arrow-head').remove();
        $('table#mus-table tr th').first().next().children('a').attr('sort-direction', 'asc');
        $('table#mus-table tr th').first().next().children('a').append('<span class="sort-arrow-head">&#9650;</span>');
        MUS.musDetails(MUS.showMUS);
    });

    // sorting on columns
    $('#mus-table tr th a').on('click', function () {
        var sortColumn = $(this).attr('sort-on');
        var sortDirectionText = $(this).attr('sort-direction');
        var sortDirection = true;
        $('.sort-arrow-head').remove();
        $($('table#mus-table tr th').not($(this).parent())).children('a').attr('sort-direction', '')
        if (sortDirectionText == null) {
            $(this).append('<span class="sort-arrow-head">&#9650;</span>');
            $(this).attr('sort-direction', 'asc');
            var sortDirection = false;
        }
        else if (sortDirectionText == 'asc') {
            $(this).append('<span class="sort-arrow-head">&#9660;</span>');
            $(this).attr('sort-direction', 'desc');
            var sortDirection = true;
        }
        else {
            $(this).append('<span class="sort-arrow-head">&#9650;</span>');
            $(this).attr('sort-direction', 'asc');
            var sortDirection = false;
        }

        MUS.musDetails(MUS.showMUS, sortColumn, sortDirection);
    });

    // execute UI Events
    $('body').on('click', 'i[name="executeUIEvent"]', function (e) {
        var elem = $(e.currentTarget);
        if (!elem.hasClass('disabled')) {
            var eventId = elem.attr('id');
            var taskId = elem.attr('taskId');
            var instanceId = elem.attr('instanceId');
            var graphId = elem.attr('graphId');
            var simulationId = elem.attr('simulationId');
            var uievent = elem.attr('uievent');
            var isUIEvent = elem.attr('isUIEvent');
            var eventType = elem.attr('eventType');

            if (isUIEvent == "0") {
                uievent = null;
            }
            $('i[name = "executeUIEvent"]').not(elem).addClass('disabled');
            var data = { taskId: taskId, instanceId: instanceId, graphId: graphId, simulationId: simulationId, eventId: eventId };
            if (eventType === "TasksWNote") {
                App.showTaskWithNotePopup(data, elem, false, uievent, true);
            }
            else {
                App.executeEvent(data, false, uievent, true);
            }
            e.preventDefault();
        }
    });

    // get instructions mus instruction html on top of page
    Instruction.getInstructionHtml('mus');
    App.hideDocumentWebpart(true);
});