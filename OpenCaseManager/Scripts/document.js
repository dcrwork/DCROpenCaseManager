
var uploadFiles;
var isAdd = true;
var dropArea;

$(document).ready(function () {
    dropArea = document.getElementById("drop-area");
    uploadFiles = new Array();
    initializeDragnDrop();
    getDocuments();

    // To create a template funtion
    $('body').on('click', 'div table.tasksTable tbody td span[name="deleteDoc"]', function () {

        elem = $(this);

        App.showConfirmMessageBox(translations.DeleteDocumentMessage, translations.Yes, translations.No, function () {

            var docId = elem.attr('documentId');
            deleteDocument(docId);

        }, null, translations.DeleteDocument + '?');
    })

    // To create a template funtion
    $('body').on('click', 'div table.tasksTable tbody td span[name="editDoc"]', function () {
        elem = $(this);
        var docId = elem.attr('documentId');
        getDocumentDetails(docId);
    })

    // To create a template funtion
    /*
        $('body').on('click', 'table.tasksTable tbody td a[name="downloadDoc"]', function () {
            elem = $(this);
            var link = elem.attr('documentLink');
            var win = window.open(window.location.origin + "/File/DownloadFile?link=" + link, '_blank');
            win.focus();
        })
    */

    $('body').on('click', '#addNewDocumentBtn', function () {
        initializeForm();
        $('#addNewDocumentModal').modal('toggle');
        isAdd = true;
        $('#documentName').focus();
        $('.instanceModalHeading').text(translations.AddDocument);
        $('#addDocument').text(translations.Add);
    })

});


function deleteDocument(docId) {
    var query = {
        "Id": docId,
        "Type": webPortalType + "Document",
        "InstanceId": instanceId
    }
    API.service('records/deleteDocument', query)
        .done(function (response) {
            getDocuments();
        })
        .fail(function (e) {
            //showExceptionErrorMessage(e);
        });
}

function getDocumentDetails(docId) {
    var query = {
        "type": "SELECT",
        "entity": "Document",
        "filters": [
            {
                "column": "Id",
                "operator": "equal",
                "value": docId,
                "valueType": "int"
            }
        ],
        "resultSet": ["Title", "Link"],
        "order": [{ "column": "Title", "descending": false }]
    }

    API.service('records', query)
        .done(function (response) {
            initializeForm();
            var JSONResponse = JSON.parse(response);
            $('#documentName').val(JSONResponse[0].Title);
            $('#documentId').val(docId);
            uploadFiles = new Array();
            $('#addNewDocumentModal').modal('toggle');
            isAdd = false;
            $('.instanceModalHeading').text(translations.EditDocument);
            $('#addDocument').text(translations.Update);
            $('#documentName').show();
            $('#documentNameLabel').show();
        })
        .fail(function (e) {
            //showExceptionErrorMessage(e);
        });
}

function getDocumentById(docId) {
    var query = {
        "type": "SELECT",
        "entity": "Document",
        "filters": [
            {
                "column": "Id",
                "operator": "equal",
                "value": docId,
                "valueType": "int"
            }
        ],
        "resultSet": ["Title", "Link"],
        "order": [{ "column": "Title", "descending": false }]
    }

    API.service('records', query)
        .done(function (response) {
            console.log(response);
        })
        .fail(function (e) {
            //showExceptionErrorMessage(e);
        });
}

function initializeDragnDrop() {
    dropArea.addEventListener('dragenter', preventDefaults, false)
    document.body.addEventListener('dragenter', preventDefaults, false)
    dropArea.addEventListener('dragover', preventDefaults, false)
    document.body.addEventListener('dragover', preventDefaults, false)
    dropArea.addEventListener('dragleave', preventDefaults, false)
    document.body.addEventListener('dragleave', preventDefaults, false)
    dropArea.addEventListener('drop', preventDefaults, false)
    document.body.addEventListener('drop', preventDefaults, false)
    dropArea.addEventListener('dragenter', highlight, false)
    dropArea.addEventListener('dragover', highlight, false)
    dropArea.addEventListener('dragleave', unhighlight, false)
    dropArea.addEventListener('drop', unhighlight, false)
    // Handle dropped files
    dropArea.addEventListener('drop', handleDrop, false)
}

function preventDefaults(e) {
    e.preventDefault()
    e.stopPropagation()
}

function highlight(e) {
}

function unhighlight(e) {
}

function handleDrop(e) {
    var dt = e.dataTransfer
    var files = dt.files

    handleFiles(files)
}

function handleFiles(files) {
    uploadFiles = new Array();
    for (var i = 0; i < 1; i++) {
        uploadFiles.push(files[i]);
        $('#selectedFileName').text(files[i].name);
        dropArea.classList.add('highlight')
        $('#documentName').val(files[i].name.substring(0, files[i].name.lastIndexOf('.')) || files[i].name);
        $('#documentName').show();
        $('#documentNameLabel').show();
    }
}

function submitFilesDocuments() {
    var docId = $('#documentId').val();
    if (uploadFiles.length > 0) {
        uploadFileDocuments(uploadFiles[0], docId);
    }
    else if (docId != '') {
        uploadFileDocuments(null, docId);
    }
}

function uploadFileDocuments(file, docId) {
    if (instanceId == null)
        instanceId = getParameterByName("id");
    var data = {};
    incrementLoaderCount("before uploadFileDocuments");
    if (isAdd && $('#documentName').val() != '') {
        var filename = $('#documentName').val();
        var i = file.name.lastIndexOf('.');
        if (i > -1) filename += file.name.substr(i);
        $.ajax({
            url: window.location.origin + "/api/services/UploadDocumentAcadre",
            type: 'POST',
            headers: {
                'documentCategoryCode': 'Notat', //'documentCategoryCode',
                'documentTitleText': filename, //'documentTitleText',
                'documentStatusCode': 'B', //'documentStatusCode',
                'documentAccessCode': 'BO', //'documentAccessCode',
                'documentCaseId': '$(InternalCaseId)',
                'documentDescriptionText': 'Uploaded fra OCM', //'documentDescriptionText',
                'documentAccessLevel': 'BO', //'documentAccessLevel',
                'documentTypeCode': 'N', //'documentTypeCode',
                'recordStatusCode': 'J', //'recordStatusCode',
                'documentUserId': '$(loggedInUser)',
                'recordPublicationIndicator': '1', //'recordPublicationIndicator'
                'documentDate': moment(new Date()).format('MM/DD/YYYY HH:mm:ss'),
                //                'type': webPortalType + "Document",
                'instanceId': instanceId,
                //                'givenFileName': $('#documentName').val(),
                'filename': file.name
            },
            data: file,
            async: false,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            success: function (response) {
                $('#addNewDocumentModal').modal('toggle');
                getDocuments();
                dropArea.classList.remove('highlight')
                var $el = $('#fileElem');
                $el.wrap('<form>').closest('form').get(0).reset();
                $el.unwrap();
                decrementLoaderCount("uploadfiledocuments succes");
            },
            error: function (e) {
                decrementLoaderCount("uploadfiledocuments error");
            }
        });
    }
    else if ($('#documentName').val() != '') {
        $.ajax({
            url: window.location.origin + "/api/records/UpdateDocument",
            type: 'POST',
            headers: {
                'id': docId,
                'filename': (file == null ? "" : file.name),
                'type': webPortalType + "Document",
                'instanceId': instanceId,
                'givenFileName': $('#documentName').val(),
                'isNewFileAdded': (file == null ? "false" : "true")
            },
            data: file,
            async: false,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            success: function (response) {
                $('#addNewDocumentModal').modal('toggle');
                getDocuments();
                dropArea.classList.remove('highlight')
                var $el = $('#fileElem');
                $el.wrap('<form>').closest('form').get(0).reset();
                $el.unwrap();
                decrementLoaderCount("update document succes");
            },
            error: function (e) {
                decrementLoaderCount("update document error");
            }
        });
    }
}

async function getDocuments() {
    var prefix = await App.getKeyValue('AcadreFrontEndBaseURL');
    if (instanceId === null && webPortalType === "Instance") {
        var caseNoForeign = App.getParameterByName("casenoforeign", window.location.href.toLowerCase());

        var query = {
            "type": "SELECT",
            "entity": "Instance",
            "resultSet": ["Id"],
            "filters": [
                {
                    "column": "CaseNoForeign",
                    "operator": "equal",
                    "value": caseNoForeign,
                    "valueType": "string"
                }
            ],
            "order": [{ "column": "Title", "descending": true }]
        };

        API.service('records', query)
            .done(function (response) {
                var result = JSON.parse(response)
                instanceId = result[0].Id;

                //query = {
                //    "type": "SELECT",
                //    "entity": "Document",
                //    "filters": [
                //        {
                //            "column": "IsActive",
                //            "operator": "equal",
                //            "value": true,
                //            "valueType": "boolean",
                //            "logicalOperator": "and"
                //        },
                //        {
                //            "column": "Type",
                //            "operator": "equal",
                //            "value": webPortalType + "Document",
                //            "valueType": "string",
                //            "logicalOperator": "and"
                //        }
                //    ],
                //    "resultSet": ["Id", "Title", "Link", "IsActive"],
                //    "order": [{ "column": "Title", "descending": false }]
                //}
                //query.filters.push({
                //    "column": "InstanceId",
                //    "operator": "equal",
                //    "value": instanceId,
                //    "valueType": "string"
                //});
                query =
                    {
                        "InstanceId": instanceId
                    }

                API.service('services/GetChildCaseDocuments', query)
                    .done(function (response) {

                        console.log("data", response);
                        var result = JSON.parse(response)
                        var list = "";
                        if (result.length === 0)
                            list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
                        else {
                            for (i = 0; i < result.length; i++) {
                                var item = result[i];
                                var Link = GetDocumentLink(prefix, item.DocumentID);

                                var returnHtml = '';
                                returnHtml = '<tr class="trStyleClass">' +
                                    '<td style="display:none"> ' + item.DocumentID + ' </td><td>' + GetIconType(item.Type) + '</td><td><a name="downloadDoc" target="_blank" href="' + Link + '" documentLink="' + item.Type + '" documentId="' + item.DocumentID + '" > ' + item.Title + '</a> </td><td>';
                                //returnHtml += '<span documentId=' + item.Id + ' name="editDoc" value="editDoc" class="spanMUS floatLeftPro fa fa-pencil-alt" title="' + translations.Edit + '"></span> ';
                                //returnHtml += '<span documentId=' + item.Id + ' name="deleteDoc" value="deleteDoc" class="spanMUS floatLeftPro fa fa-trash" title="' + translations.Delete + '"></span> ';
                                if (item.LastChangedDate != null)
                                    returnHtml += item.LastChangedDate.substr(8, 2) + '/' + item.LastChangedDate.substr(5, 2) + '-' + item.LastChangedDate.substr(0, 4);
                                returnHtml += '</td>' + '</tr>';

                                list += returnHtml;
                            }
                        }
                        $("#files").html("").append(list);

                    })
                    .fail(function (e) {
                        //showExceptionErrorMessage(e);
                    });
                initializeForm();

            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }
    else {

        query = {
            "type": "SELECT",
            "entity": "[Instance]",
            "resultSet": ["Id", "CaseNoForeign"],
            "filters": new Array(),
            "order": []
        }

        var whereInstanceIdMatchesFilter = {
            "column": "Id",
            "operator": "equal",
            "value": instanceId,
            "valueType": "int",
            "logicalOperator": "and"
        };
        query.filters.push(whereInstanceIdMatchesFilter);
        var result2 = await API.service('records', query);
        var instance2 = JSON.parse(result2);

        //query = {
        //    "type": "SELECT",
        //    "entity": "Document",
        //    "filters": [
        //        {
        //            "column": "IsActive",
        //            "operator": "equal",
        //            "value": true,
        //            "valueType": "boolean",
        //            "logicalOperator": "and"
        //        },
        //        {
        //            "column": "Type",
        //            "operator": "equal",
        //            "value": webPortalType + "Document",
        //            "valueType": "string",
        //            "logicalOperator": "and"
        //        }
        //    ],
        //    "resultSet": ["Id", "Title", "Link", "IsActive"],
        //    "order": [{ "column": "Title", "descending": false }]
        //}

        //switch (webPortalType) {
        //    case 'Personal':
        //        query.filters.push({
        //            "column": "Responsible",
        //            "operator": "equal",
        //            "value": "$(loggedInUser)",
        //            "valueType": "string"
        //        });
        //        break;
        //    case 'Instance':
        //        query.filters.push({
        //            "column": "InstanceId",
        //            "operator": "equal",
        //            "value": instanceId,
        //            "valueType": "string"
        //        });
        //        break;
        //}
        if (instance2[0].CaseNoForeign == null || instance2[0].CaseNoForeign == '') {
            list = "";

            $("#files").html("<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + "Ingen Acadre sag fundet" + " </td></tr>")
        }
        else {
            query =
                {
                    "InstanceId": instanceId
                }

            API.service('services/GetChildCaseDocuments', query)
                .done(function (response) {

                    var result = JSON.parse(response)
                    var list = "";
                    if (result.length === 0)
                        list = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
                    else {
                        for (i = 0; i < result.length; i++) {
                            var item = result[i];

                            var returnHtml = '';
                            var Link = GetDocumentLink(prefix, item.DocumentID);
                            returnHtml = '<tr class="trStyleClass">' +
                                '<td style="display:none"> ' + item.DocumentID + ' </td><td>' + GetIconType(item.Type) + '</td><td><a target="_blank" name="downloadDoc" href="' + Link + '" documentLink="' + item.Type + '" documentId="' + item.DocumentID + '" > ' + item.Title + '</a> </td><td>';
                            //returnHtml += '<span documentId=' + item.DocumentID + ' name="editDoc" value="editDoc" class="spanMUS floatLeftPro glyphicon glyphicon-edit" title="' + translations.Edit + '"></span> ';
                            //returnHtml += '<span documentId=' + item.DocumentID + ' name="deleteDoc" value="deleteDoc" class="spanMUS floatLeftPro glyphicon glyphicon-trash" title="' + translations.Delete + '"></span> ';
                            if (item.LastChangedDate != null)
                                returnHtml += item.LastChangedDate.substr(8, 2) + '/' + item.LastChangedDate.substr(5, 2) + '-' + item.LastChangedDate.substr(0, 4);
                            returnHtml += '</td>' + '</tr>';

                            list += returnHtml;
                        }
                    }
                    $("#files").html("").append(list);

                })
                .fail(function (e) {
                    //showExceptionErrorMessage(e);
                });
            initializeForm();
        }
    }
}

function initializeForm() {
    $('#documentName').val('');
    $('#documentId').val('');
    $('#documentName').hide();
    $('#selectedFileName').text('');
    $('#documentNameLabel').hide();
    uploadFiles = new Array();
}

function GetIconType(link) {
    switch (link.split('.').pop().toLowerCase()) {
        case 'doc':
        case 'docx':
            return '<i title="Word" class="fa fa-file-word faIconStyling"></i>';
        case 'ppt':
        case 'pptx':
            return '<i title="PowerPoint" class="fa fa-file-powerpoint faIconStyling"></i>';
        case 'xls':
        case 'xlsx':
            return '<i title="Excel" class="fa fa-file-excel faIconStyling"></i>';
        case 'pdf':
            return '<i title="PDF" class="fa fa-file-pdf faIconStyling"></i>';
        case 'zip':
        case '7z':
        case 'rar':
            return '<i title="Compressed" class="fas fa-file-archive faIconStyling"></i>';
        case 'png':
        case 'jpeg':
        case 'jpg':
            return '<i title="Image" class="fas fa-file-image faIconStyling"></i>';
        case 'mp3':
            return '<i title="Audio" class="fa fa-file-audio faIconStyling"></i>';
        case 'mp4':
        case 'wmv':
        case 'mkv':
        case 'avi':
            return '<i title="Video" class="fa fa-file-movie faIconStyling"></i>';
        case 'txt':
            return '<i title="Text" class="fa fa-file-text faIconStyling"></i>';
        default:
            return '<i title="Unknown Type" class="fas fa-file faIconStyling"></i>';
    }
}

function GetDocumentLink(prefix, docId) {
    return prefix + '/MainDocument/Details?documentId=' + docId;
}