
$(document).ready(function () {
    //todo:mytasks-Search Template
    $('#searchTemplate').on("input", function () {
        var input1 = this.value;

        var input2 = $('#searchTemplate').val();
        var query = {
            "type": "SELECT",
            "entity": "FORM",
            "resultSet": ["Id", "Title"],
            "filters": [
                { "column": "IsTemplate", "value": "true", "valueType": "boolean", "operator": "equal", "logicalOperator": "and" },
                { "column": "Title", "value": "%" + input2 + "%", "valueType": "string", "operator": "like" }
            ]
        }

        API.service('records', query)
            .done(function (response) {
                console.log(response);
                var data = JSON.parse(response);
                var html = '';
                if (data.length === 0)
                    html = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
                $(data).each(function (index, item) {
                    html += '<tr><td> <a class="formLink" id=' + item.Id + ' href="#">' + item.Title + '</a></td></tr>';
                });
                $('#templateFormsTableBody').html(html);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    $('#buttonShowMyForms').click(function (e) {
        $('#formQuestions').hide();
        $('#myFormLists').show();
        $('#templateForms').hide();
        $('#buttonShowMyForms').hide();
        // get my forms
        getMyForms();
    });

    $('#buttonFromTemplate').click(function (e) {
        $('#formQuestions').hide();
        $('#myFormLists').hide();
        $('#templateForms').show();
        $('#buttonShowMyForms').show();

        // get template forms
        var query = {
            "type": "SELECT",
            "entity": "FORM",
            "resultSet": ["Id", "Title"],
            "filters": [
                { "column": "IsTemplate", "value": "true", "valueType": "boolean", "operator": "equal" }
            ]
        }

        API.service('records', query)
            .done(function (response) {
                console.log(response);
                var data = JSON.parse(response);
                var html = '';
                if (data.length === 0)
                    html = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
                $(data).each(function (index, item) {
                    html += '<tr><td> <a class="formLink" id=' + item.Id + ' href="#">' + item.Title + '</a></td></tr>';
                });
                $('#templateFormsTableBody').html(html);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    $('#buttonNewForm').click(function (e) {

        // add new form and form from template
        var data = {
            "isFromTemplate": "false",
            "templateFormId": null
        }

        API.service('records/addForm', data)
            .done(function (response) {
                openFormQuestions(response);
                getFormDetails(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });

    });

    $("#formName").blur(function () {
        var value = $(this).val();
        var itemId = $('#formQuestions').attr('formid');

        // edit form details

        var data = {
            "id": itemId,
            "title": value,
            "isTemplate": $('#formIsTemplate').prop('checked')
        }

        API.service('records/updateForm', data)
            .done(function (response) {
                console.log(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    $("#formIsTemplate").change(function () {
        var value = $('#formName').val();
        var itemId = $('#formQuestions').attr('formid');

        // edit form details

        var data = {
            "id": itemId,
            "title": value,
            "isTemplate": $('#formIsTemplate').prop('checked')
        }

        API.service('records/updateForm', data)
            .done(function (response) {
                console.log(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    $('.questions-table').treegrid({
        source: null,
        enableMove: true,
        onMove: function (item, target, position) {
            var itemId = $('.' + $(item).attr('class').replace(/\ /g, '.')).treegrid('getId');
            var targetId = $('.' + $(target).attr('class').replace(/\ /g, '.')).treegrid('getId');
            // make a parent item , when no child exists
            if ($(item).hasClass('isChild')) {
                var parent = $('.treegrid-' + itemId).treegrid('getParent');
                var parentId = $('.' + $(parent).attr('class').replace(/\ /g, '.')).treegrid('getId');
                var children = $('.treegrid-' + parentId).treegrid('getChildNodes');

                if ($(item).hasClass('isChild') && children.length == 1 && target != parent) {
                    $(parent).removeClass('isParent');
                    $(parent).addClass('newItem');
                }
            }

            // if child is moving on root position, make it an item
            if ($(item).hasClass('isChild') && ($(target).hasClass('isParent') || $(target).hasClass('newItem')) && position != 1) {
                $(item).removeClass('isChild');
                $(item).addClass('newItem');
                $('.' + $(item).attr('class').replace(/\ /g, '.') + ' span[class="Questions-actions"]').prepend('<img questionId=' + itemId + ' class="addQuestion addQuestion-' + itemId + '" src="../content/images/addNewQuestion.png">');
                addChildEvent('.addQuestion-' + itemId);
            }
            // if child/item is moving in an another item , make item Parent
            if ($(target).hasClass('newItem') && position == 1) {
                $(target).removeClass('newItem');
                $(target).addClass('isParent');
            }
            // if item is moving aside a child, make item a child
            if ($(item).hasClass('newItem') && $(target).hasClass('isChild') && position != 1) {
                $(item).removeClass('newItem');
                $(item).addClass('isChild');
                $('.' + $(item).attr('class').replace(/\ /g, '.') + ' span[class="Questions-actions"] :first').remove();
            }
            // if item is moving in a parent, make item a child
            if ($(item).hasClass('newItem') && $(target).hasClass('isParent') && position == 1) {
                $(item).removeClass('newItem');
                $(item).addClass('isChild');
                $('.' + $(item).attr('class').replace(/\ /g, '.') + ' span[class="Questions-actions"] :first').remove();
            }
            setSequenceNumber(itemId, targetId, position);
        },
        onMoveOver: function (item, helper, target, position) {
            if (item.hasClass('isChild') && target.hasClass('isChild') && position != 0) return false;
            if (item.hasClass('isParent') && target.hasClass('isParent') && position != 0) return false;
            if (item.hasClass('newItem') && target.hasClass('isChild') && position != 0) return false;
            if (item.hasClass('isParent') && target.hasClass('isChild')) return false;
            if (item.hasClass('isParent') && target.hasClass('newItem')) return false;
            return true;
        }
    });

    // get my forms
    getMyForms();

    //Add new Question
    $('.add-question-btn').on('click', function () {
        addQuestion();
    });

    // get instructions from instruction html on top of page
    Instruction.getInstructionHtml('form');
});

function getMyForms() {
    var query = {
        "type": "SELECT",
        "entity": "FORM",
        "resultSet": ["Id", "Title"],
        "filters": [
            { "column": "UserId", "value": "$(loggedInUserId)", "valueType": "string", "operator": "equal" }
        ]
    }

    API.service('records', query)
        .done(function (response) {
            var data = JSON.parse(response);
            var html = '';
            if (data.length === 0)
                html = "<tr class=\"trStyleClass\"><td colspan=\"100%\"> " + translations.NoRecordFound + " </td></tr>";
            $(data).each(function (index, item) {
                html += '<tr><td> <a class="formLink" id=' + item.Id + ' href="#">' + item.Title + '</a></td>';
                html += '<td style="width:15%;"><select class="form-control dlDocument" formName="' + item.Title + '"  formId="' + item.Id + '"><option value="0">' + translations.Select + '</option><option value="1">' + translations.DownloadAsPDF + '</option><option value="2">' + translations.SavePDFToPersonalFolder + '</option><option value="3">' + translations.DownloadAsWord + '</option><option value="4">' + translations.SaveWordToPersonalFolder + '</option></select></td>';
                html += "</tr>";
            });
            $('#myForms').html(html);

            $('.formLink').click(function (e) {

                var id = e.target.attributes["id"].value;
                console.log(id);
                openFormQuestions(id);
                getFormDetails(id);
            });

            $('.dlDocument').change(function (element) {
                var _formName = $(this).attr('formName');
                var _formId = $(this).attr('formId');
                if ($(this).val() === "1") {
                    $(element.currentTarget).val("0");
                    var getUrl = window.location;
                    var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
                    window.open(baseUrl + "Form/getpdf?formName=" + _formName + "&formId=" + _formId);
                }
                else if ($(this).val() === "2") {
                    API.service("records/UploadFormToPersonalFolder", { "formId": _formId, "formName": _formName, type: "pdf" })
                        .done(function () {
                            App.showSuccessMessage(translations.UploadedToPersonalFolder);
                            $(element.currentTarget).val("0");
                        })
                        .fail(function (e) {
                            if (e.status === 200) {
                                App.showSuccessMessage(translations.UploadedToPersonalFolder);
                                $(element.currentTarget).val("0");
                            }
                            else
                                App.showExceptionErrorMessage(e);
                        });
                }
                else if ($(this).val() === "3") {
                    $(element.currentTarget).val("0");
                    var getUrl = window.location;
                    var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
                    window.open(baseUrl + "Form/getword?formName=" + _formName + "&formId=" + _formId);
                }
                else if ($(this).val() === "4") {
                    API.service("records/UploadFormToPersonalFolder", { "formId": _formId, "formName": _formName, type: "word" })
                        .done(function () {
                            App.showSuccessMessage(translations.UploadedToPersonalFolder);
                            $(element.currentTarget).val("0");
                        })
                        .fail(function (e) {
                            if (e.status === 200) {
                                App.showSuccessMessage(translations.UploadedToPersonalFolder);
                                $(element.currentTarget).val("0");
                            }
                            else
                                App.showExceptionErrorMessage(e);
                        });
                }
            });
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

function treeGridHTML(id, parentId, itemText, newItemParent) {
    var parentClassName = newItemParent != undefined ? 'newItem' : 'isParent';
    var html = '';
    if (parentId == null)
        html = '<tr id=' + id + ' class="' + parentClassName + ' treegrid-' + id + '"><td><span questionId=' + id + ' class="title-question">' + itemText + '</span><span class="Questions-actions"><img questionId=' + id + ' class="backgroundImagesCss  addQuestion addQuestion-' + id + '" src=""><img questionId=' + id + ' class="backgroundDeleteImagesCss deleteQuestion deleteQuestion-' + id + '" src="" /></span></td></tr>';
    else
        html = '<tr id=' + id + ' class="isChild treegrid-' + id + ' treegrid-parent-' + parentId + '" > <td><span questionId=' + id + ' class="title-question">' + itemText + '</span><span class="Questions-actions"><img questionId=' + id + ' class="backgroundDeleteImagesCss deleteQuestion deleteQuestion-' + id + '"" src="" /></span></td></tr>';
    return html;
}

function addQuestion(parentId) {
    // add question to form

    var nextSequenceNumber = 0;
    var addChildAt = '.questions-table';
    if (parentId == null) {
        nextSequenceNumber = $('.isParent,.newItem').length;
    }
    else {
        nextSequenceNumber = $('.treegrid-parent-' + parentId).length;
        addChildAt = '.treegrid-' + parentId;
    }
    var formId = $('#formQuestions').attr('formId');

    var data = {
        "formId": formId,
        "itemId": parentId,
        "itemText": "New Item",
        "isGroup": parentId == null ? 'true' : 'false',
        "sequenceNumber": nextSequenceNumber + 1
    }

    API.service('records/addQuestion', data)
        .done(function (response) {
            console.log(response);
            var html = treeGridHTML(response, parentId, 'New Item', parentId == null ? true : undefined);
            $(addChildAt).treegrid('add', [html]);
            addChildEvent('.addQuestion-' + response);
            deleteItemEvent('.deleteQuestion-' + response);
            $('.treegrid-' + parentId).treegrid('expand');
            $('tr.treegrid-' + response + '>td').dblclick();
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

function addChildEvent(addQuestionClassName) {
    $(addQuestionClassName).click(function (e) {
        var parentId = e.target.attributes["questionId"].value;
        addQuestion(parentId);
    });
}

function setSequenceNumber(itemId, targetId, position) {
    var data = {
        "itemId": itemId,
        "targetId": targetId,
        "position": position
    }

    API.service('records/setQuestionSequence', data)
        .done(function (response) {
            console.log(response);
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

function deleteItemEvent(deleteQuestionClassName) {
    $(deleteQuestionClassName).click(function (e) {
        var itemId = e.target.attributes["questionId"].value;
        deleteQuestion(itemId);
    });
}

function deleteQuestion(itemId) {
    var data = {
        "itemId": itemId
    }

    API.service('records/deleteQuestion', data)
        .done(function (response) {
            console.log(response);
            $('.treegrid-' + itemId).treegrid('remove');
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

function updateQuestion(itemId, itemText) {
    var data = {
        "itemId": itemId,
        "itemText": itemText
    }

    API.service('records/UpdateQuestion', data)
        .done(function (response) {
            console.log(response);
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });
}

// get form Item
function openFormQuestions(formId) {

    $('#templateForms').hide();
    $('#myFormLists').hide();
    $('#formQuestions').show().attr('formId', formId);
    $('#buttonShowMyForms').show();


    var query = {
        "type": "SELECT",
        "entity": "FORMITEM",
        "resultSet": ["Id", "ItemText", "ItemId", "SequenceNumber"],
        "filters": [
            { "column": "FormId", "value": formId, "valueType": "int", "operator": "equal" }
        ],
        "order": [{ "column": "ItemId", "descending": false }, { "column": "SequenceNumber", "descending": false }]
    }

    API.service('records', query)
        .done(function (response) {
            console.log(response);
            $("#treegrid-body").empty();
            var data = JSON.parse(response);

            var parents = data.filter(function (item) {
                return item.ItemId == null;
            })

            $(parents).each(function (index, parent) {
                var children = data.filter(function (item) {
                    return item.ItemId == parent.Id;
                });
                var html = treeGridHTML(parent.Id, parent.ItemId, parent.ItemText, children.length == 0 ? true : undefined);

                $(children).each(function (index, child) {
                    html += treeGridHTML(child.Id, child.ItemId, child.ItemText);
                });
                $('.questions-table').treegrid('add', [html]);
                addChildEvent('.addQuestion-' + parent.Id);
                deleteItemEvent('.deleteQuestion-' + parent.Id);

                $(children).each(function (index, child) {
                    deleteItemEvent('.deleteQuestion-' + child.Id);
                });
            });
            $('#formName').focus();
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });

}

function getFormDetails(formId) {
    var query = {
        "type": "SELECT",
        "entity": "FORM",
        "resultSet": ["Id", "Title", "IsTemplate"],
        "filters": [
            { "column": "Id", "value": formId, "valueType": "int", "operator": "equal" }
        ]
    }

    API.service('records', query)
        .done(function (response) {
            var data = JSON.parse(response);
            $('#formName').val(data[0].Title);
            $('#formIsTemplate').prop('checked', data[0].IsTemplate);
        })
        .fail(function (e) {
            App.showExceptionErrorMessage(e);
        });

}

//Edit with double click on text
$('body').on('dblclick', '.questions-table tr>td:nth-child(1)', function () {
    var titleVal = $(this).parents('tr').find('span.title-question').html();
    var itemId = $(this).parents('tr').find('span.title-question').attr('questionId');
    var $tdData = $(this).children('div.treegrid-container').css({ 'display': 'none' });
    var $input = $(this).append('<input questionId=' + itemId + ' class="title-question" type="text" autofocus />').find('input');
    $input.val('' + titleVal);
    $($input).focus();
})

//Save edited input data
$('body').on('blur', '.questions-table tr>td:nth-child(1) input', function () {
    var inpText = $(this).val();
    var divEle = $(this).prev();
    var itemId = $(this).attr('questionId');

    $(divEle).find('span.title-question').html('' + inpText + '');

    $(divEle).css('display', 'block');
    $(this).unbind('keypress');
    $(this).remove();
    updateQuestion(itemId, inpText);
});

$('body').on('keypress', '.questions-table tr>td:nth-child(1) input', function (e) {
    if (e.keyCode == 13) {
        $(this).blur();
    }
});

// To create a template funtion
$('body').on('click', '#templateForms table.forms-listing-table tbody td a:nth-child(1)', function () {

    elem = $(this);

    App.showConfirmMessageBox(translations.FormFromTemplate, translations.Yes, translations.No, function () {

        var data = {
            "isFromTemplate": "true",
            "templateFormId": elem.attr('id')
        }

        API.service('records/addForm', data)
            .done(function (response) {
                openFormQuestions(response);
                getFormDetails(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });

    }, null, translations.CreateTemplate + ' ?');
})