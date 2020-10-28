$(document).ready(function () {

    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
        App.hideDocumentWebpart();
    });

    promise.then(function (result) {
        var instanceId = App.getParameterByName("id", window.location.href);
        if (instanceId === null) {
            var internalCaseId = App.getParameterByName("internalcaseid", window.location.href.toLowerCase());

            if (internalCaseId == null) {
                window.location.href = "../../";
            }

            var query = {
                "type": "SELECT",
                "entity": "Instance",
                "resultSet": ["Id"],
                "filters": [
                    {
                        "column": "InternalCaseId",
                        "operator": "equal",
                        "value": internalCaseId,
                        "valueType": "int"
                    }
                ],
                "order": [{ "column": "Title", "descending": true }]
            };

            API.service('records', query)
                .done(function (response) {
                    var result = JSON.parse(response)
                    console.log(result);
                    Task.InstanceId = result[0].Id;
                    document.title = App.getInstanceTitle(result[0].Id);
                    App.getPhases(result[0].Id);
                    App.getTasks(result[0].Id);
                    App.getInstanceDetails(result[0].Id);
                    $('#addNewDocumentText').text(translations.Documents);
                    Task.addTaskHtml(result[0].Id);
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e);
                });
        }
        else {
            document.title = App.getInstanceTitle(instanceId);
            App.getPhases(instanceId);
            App.getTasks(instanceId);
            App.getJournalHistoryForInstance(instanceId);
            App.getInstanceDetails(instanceId);
            $('#addNewDocumentText').text(translations.Documents);
            Task.addTaskHtml(instanceId);
        }
    }, function (err) {
        console.log(err); // Error: "It broke"
    });

    $('#responsibleDropdown').change(function () {
        window.localStorage.setItem('responsibleDD', $('#responsibleDropdown').val());
        var instanceId = App.getParameterByName("id", window.location.href);
        App.getTasks(Task.InstanceId);
    });

    if (window.localStorage.getItem('responsibleDD') != null) {
        $('#responsibleDropdown').val(window.localStorage.getItem('responsibleDD'));
    }

    $('#taskStatusDropDown').change(function () {
        window.localStorage.setItem('taskStatusDD', $('#taskStatusDropDown').val());
        var instanceId = App.getParameterByName("id", window.location.href);
        App.getTasks(instanceId);
    });

    if (window.localStorage.getItem('taskStatusDD') != null) {
        $('#taskStatusDropDown').val(window.localStorage.getItem('taskStatusDD'));
    }

    API.serviceGET('services/GetKeyValue?key=DCRPortalURL')
        .done(function (response1) {
            API.getJSFile(response1 + 'scripts/modules/iframemode.js')
                .done(function () {

                    var internalCaseId = App.getParameterByName("internalcaseid", window.location.href.toLowerCase());

                    var query = {
                        "type": "SELECT",
                        "entity": "Instance",
                        "resultSet": ["Id"],
                        "filters": [
                            {
                                "column": "InternalCaseId",
                                "operator": "equal",
                                "value": internalCaseId,
                                "valueType": "int"
                            }
                        ],
                        "order": [{ "column": "Title", "descending": true }]
                    };

                    API.service('records', query)
                        .done(function (response) {
                            var result = JSON.parse(response)
                            var query = {
                                "type": "SELECT",
                                "entity": "Instance",
                                "resultSet": ["DCRXML"],
                                "filters": [
                                    {
                                        "column": "Id",
                                        "operator": "equal",
                                        "value": result[0].Id,
                                        "valueType": "int"
                                    }
                                ],
                                "order": [{ "column": "title", "descending": false }]
                            };

                            API.service('records', query)
                                .done(function (response) {
                                    var result = JSON.parse(response);

                                    iFrameModule.init({
                                        iframeHolder: "editorIframe",
                                        attrs: {
                                            frameBorder: "0",
                                            height: "800",
                                            width: "100%",
                                            id: "DCRFrame",
                                            src: response1 + "/tool/main/Iframe"
                                        },
                                        data: {
                                            xml: result[0].DCRXML,
                                            callback: "DCREditorIframeCallback",
                                            cancel: "DCREditorIframeCancelCallback"
                                        }
                                    });

                                })
                                .fail(function (e) {
                                    App.showExceptionErrorMessage(e);
                                });
                        })
                        .fail(function (e) {
                            App.showExceptionErrorMessage(e);
                        });

                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e)
                });
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e)
        });

    $('#instanceTitle').on('click', function () {
        if (window.event.ctrlKey) {
            $('#saveDCRXML').hide();
            $('#closeEditorIframe').hide();
            $('#dcrDesignerIframeModal').modal('show');
        }
        else {
            $('#updateInstanceTitleDialog').modal('toggle');
            var instanceTitle = $('#instanceTitle').text();
            $('#updateInstanceTitle').val(instanceTitle);
        }
    });

    $('#btnUpdateInstanceTitle').click(function () {
        var query = {
            "title": $('#updateInstanceTitle').val(),
            "instanceId": App.getParameterByName("id", window.location.href)
        };

        if (query.title === "") {
            App.showWarningMessage("Instance Title is missing");

        }
        else {
            API.service('records/UpdateInstanceTitle', query)
                .done(function (response) {

                    var query = {
                        "type": "SELECT",
                        "entity": "AllInstances",
                        "resultSet": ["Id", "Title", "CaseNoForeign", "CaseLink", "CurrentPhaseNo", "Description", "GraphId", "NextDeadline", "IsOpen", "Responsible"],
                        "filters": [
                            {
                                "column": "Id",
                                "operator": "equal",
                                "value": App.getParameterByName("id", window.location.href),
                                "valueType": "int"
                            }
                        ],
                        "order": [{ "column": "title", "descending": false }]
                    };

                    API.service('records', query)
                        .done(function (response) {
                            var result = JSON.parse(response);

                            $('#instanceTitle').text(result[0].Title);
                            $("li.instance").text(result[0].Title);
                            if (result[0].IsOpen) $('#instanceTitle').append(getStatus(result[0].NextDeadline));
                            else $('#instanceTitle').append("<span class='dot dotGrey'></span>");
                            $('#updateInstanceTitleDialog').modal('toggle');
                        })
                        .fail(function (e) {
                            App.showExceptionErrorMessage(e);
                        });
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e);
                });
        }
    });

    $('#updateInstanceTitle').keypress(function (e) {
        if (e.keyCode === 13) {
            // Cancel the default action, if needed
            e.preventDefault();
            // Trigger the button element with a click
            document.getElementById("btnUpdateInstanceTitle").click();
        }
    });

    $('#dcrDesignerIframeModal').on('shown.bs.modal', function () {

        var internalCaseId = App.getParameterByName("internalcaseid", window.location.href.toLowerCase());

        var query = {
            "type": "SELECT",
            "entity": "Instance",
            "resultSet": ["Id"],
            "filters": [
                {
                    "column": "InternalCaseId",
                    "operator": "equal",
                    "value": internalCaseId,
                    "valueType": "int"
                }
            ],
            "order": [{ "column": "Title", "descending": true }]
        };

        API.service('records', query)
            .done(function (response) {
                var result = JSON.parse(response);
                var query = {
                    "type": "SELECT",
                    "entity": "Instance",
                    "resultSet": ["DCRXML"],
                    "filters": [
                        {
                            "column": "Id",
                            "operator": "equal",
                            "value": result[0].Id,
                            "valueType": "int"
                        }
                    ],
                    "order": [{ "column": "title", "descending": false }]
                };

                API.service('records', query)
                    .done(function (response) {
                        var result = JSON.parse(response);

                        window.DCREditorIframeCallback = function (xml) {
                            dcrEditorIframeCallback(xml);
                        };

                        window.DCREditorIframeCancelCallback = function () {
                            DCREditorIframeCancelCallback();
                        };

                        iFrameModule.postData({
                            xml: result[0].DCRXML,
                            callback: "DCREditorIframeCallback",
                            cancel: "DCREditorIframeCancelCallback"
                        });

                    })
                    .fail(function (e) {
                        App.showExceptionErrorMessage(e);
                    });
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    $('#dcrDesignerIframeModal').on('hidden.bs.modal', function () {
    });

    $('#updateInstanceTitleDialog').on('shown.bs.modal', function () {
        $('#updateInstanceTitle').focus();
    });

    $('#updateInstanceTitleDialog').on('hidden.bs.modal', function () {
        $('#updateInstanceTitle').blur();
    });

    $('#dcrDesignerIframe').load(function () {
        $('.loading').hide();
        $('#saveDCRXML').show();
        $('#closeEditorIframe').show();
    });

    function dcrEditorIframeCallback(dcrXML) {

        App.showConfirmMessageBox(translations.SaveChangesToGraphXML, translations.Yes, translations.No, function () {

            var internalCaseId = App.getParameterByName("internalcaseid", window.location.href.toLowerCase());

            var query = {
                "type": "SELECT",
                "entity": "Instance",
                "resultSet": ["Id"],
                "filters": [
                    {
                        "column": "InternalCaseId",
                        "operator": "equal",
                        "value": internalCaseId,
                        "valueType": "int"
                    }
                ],
                "order": [{ "column": "Title", "descending": true }]
            };

            API.service('records', query)
                .done(function (response) {
                    var result = JSON.parse(response)
                    console.log(result);
                    document.title = App.getInstanceTitle(result[0].Id);

                    var query = {
                        "DCRXML": dcrXML,
                        "instanceId": result[0].Id
                    }
                    API.service('services/UpdateInstanceDCRXML', query)
                        .done(function (response) {
                            location.reload(true);
                        })
                        .fail(function (e) {
                            App.showExceptionErrorMessage(e);
                        });
                })
                .fail(function (e) {
                    App.showExceptionErrorMessage(e);
                });
        });
    }

    function DCREditorIframeCancelCallback() {
        $('#dcrDesignerIframeModal').modal('hide');
    }
});