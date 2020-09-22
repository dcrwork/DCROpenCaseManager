
$(document).ready(function () {
    var processId = App.getParameterByName("id", window.location.href);

    var query = {
        "type": "SELECT",
        "entity": "Process",
        "resultSet": ["Title", "GraphId"],
        "filters": [
            {
                "column": "Id",
                "operator": "equal",
                "value": processId,
                "valueType": "int"
            }
        ]
    }

    API.service('records', query)
        .done(function (response) {
            var data = JSON.parse(response);
            $('#ProcessName').text(data[0].Title);
            getAllResponsibles(data[0].GraphId);
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });

    $('body').on('click', '.clickme', function () {
        $(this).nextAll('div').show();
        $(this).hide();
    });


    $('body').on('change', '#multi-select-Leder', function () {
        getProcessInstances();
    });

});

function getAllResponsibles(graphId) {
    $('#multi-select-Leder').html('');

    $('#multi-select-Leder')
        .append($("<option></option>")
            .attr("value", 0)
            .text(translations.AllResponsibles));

    var query = {
        "type": "SELECT",
        "entity": "ResponsibleInstancesCount",
        "resultSet": ["Id", "Name"],
        "filters": [
            {
                "column": "GraphId",
                "operator": "equal",
                "value": graphId,
                "valueType": "int"
            }
        ]
    }

    API.service('records', query)
        .done(function (response) {
            var data = JSON.parse(response);

            $.each(data, function (key, value) {
                $('#multi-select-Leder')
                    .append($("<option></option>")
                        .attr("value", value.Id)
                        .text(value.Name));
            });

            getProcessInstances();
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

function getProcessInstances() {
    $('#phaseInstances').html('');
    var processId = App.getParameterByName("id", window.location.href);
    var query = {
        "type": "SELECT",
        "entity": "PhaseInstances",
        "resultSet": ["Id", "InstanceCount", "Title"],
        "filters": [
            {
                "column": "ProcessId",
                "operator": "equal",
                "value": processId,
                "valueType": "int",
                "logicalOperator": 'and'
            }
        ]
    }

    if ($('#multi-select-Leder').val() != 0) {
        query.filters.push({
            "column": "Responsible",
            "operator": "equal",
            "value": $('#multi-select-Leder').val(),
            "valueType": "int"
        });
    }

    var html = "";

    API.service('records', query)
        .done(function (response) {

            var phases1 = JSON.parse(response);
            var phases = [];

            if ($('#multi-select-Leder').val() == 0) {
                var phases2 = phases1.map(function (obj) {
                    var rObj = {};
                    rObj['Id'] = obj.Id;
                    rObj['Title'] = obj.Title;
                    rObj['InstanceCount'] = obj.InstanceCount;
                    return rObj;
                });

                $.each(phases2, function (index, phase) {
                    var temp = $.grep(phases, function (e) {
                        return phase.Id === e.Id;
                    });
                    if (temp.length === 0) {
                        phases.push(phase);
                    }
                });
            }
            else {
                phases = phases1;
            }

            for (var i = 0; i < phases.length; i++) {
                console.log(phases[i]);

                html = '<div id="' + phases[i].Id + '" class="col-lg-3 col-md-3 col-sm-3 col-xs-3 instance-col"><div class="phases-txt">' + phases[i].Title + '<span class="instance-counter">(' + phases[i].InstanceCount + ')</span></div></div>';
                $('#phaseInstances').append(html);

                var query = {
                    "type": "SELECT",
                    "entity": "Instance",
                    "resultSet": ["Id", "Title", "CurrentPhaseNo", "CaseNoForeign"],
                    "filters": [
                        {
                            "column": "CurrentPhaseNo",
                            "operator": "equal",
                            "value": phases[i].Id,
                            "valueType": "int",
                            "logicalOperator": "and"
                        },
                        {
                            "column": "IsOpen",
                            "operator": "equal",
                            "value": "1",
                            "valueType": "int",
                            "logicalOperator": "and"
                        }
                    ]
                }

                if ($('#multi-select-Leder').val() != 0) {
                    query.filters.push({
                        "column": "Responsible",
                        "operator": "equal",
                        "value": $('#multi-select-Leder').val(),
                        "valueType": "int"
                    });
                }


                API.service('records', query)
                    .done(function (response) {
                        var data = JSON.parse(response);
                        console.log(data);

                        for (var i = 0; i < data.length; i++) {
                            if (i > 1) {
                                html = '';
                                if (i == 2) {
                                    html = '<a class="clickme" href="#">see all<span class="instance-counter">(' + data.length + ')</span></a>';
                                }

                                html += "<div style='display:none;'>";
                                if (i < data.length) {
                                    console.log(data[i]);
                                    html += '<div class="instance-block">';
                                    html += '<div class="ins-title">' + data[i].Title + '</div><div class="case-num-block"><a href="../Instance?id=' + data[i].Id + '">' + (data[i].CaseNoForeign == null ? 'Id:' + data[i].Id : translations.CaseNoForeign + ':' + data[i].CaseNoForeign) + '</a></div></div>';
                                }
                                html += "</div>";
                            }
                            else {
                                html = '<div class="instance-block">';
                                html += '<div class="ins-title">' + data[i].Title + '</div><div class="case-num-block"><a href="../Instance?id=' + data[i].Id + '">' + (data[i].CaseNoForeign == null ? 'Id:' + data[i].Id : 'Case:' + data[i].CaseNoForeign) + '</a></div></div>';
                            }
                            $('#' + data[0].CurrentPhaseNo).append(html);
                        }
                    })
                    .fail(function (e) {
                        App.showExceptionErrorMessage(e);
                    });
            }

        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}