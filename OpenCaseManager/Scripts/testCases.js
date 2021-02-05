function formatDate(_date) {
    var value = new Date(_date);

    var day = value.getDate() < 10 ? '0' + value.getDate() : value.getDate();
    var month = value.getMonth + 1;
    month = (value.getMonth() + 1) < 10 ? "0" + (value.getMonth() + 1) : (value.getMonth() + 1);
    var year = value.getFullYear();

    return moment(new Date(day + "/" + month + "/" + year)).format('L');
}

$(function () {
    $('#rolesContent').css('display', 'none');
    initializeDatePicker();
    App.getMyTestCases();
    App.getAllProcessesForAdministrator();
    resetModal();
    getRolesOnGraphDDChange();
    addTestCase();
    sendInvite();
});

function initializeDatePicker() {
    $("#datepickerTo, #datepickerFrom").datepicker();
    $("#datepickerTo, #datepickerFrom").datepicker("option", "dateFormat", "dd/mm/yy");

    $("#datepickerTo, #datepickerFrom").datepicker({ dayNames: ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"] });
    var dayNames = $("#datepickerTo, #datepickerFrom").datepicker("option", "dayNames");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "dayNames", ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"]);

    $("#datepickerTo, #datepickerFrom").datepicker({ dayNamesMin: ["Sø", "Ma", "Ti", "On", "To", "Fr", "Lø"] });
    var dayNamesMin = $("#datepickerTo, #datepickerFrom").datepicker("option", "dayNamesMin");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "dayNamesMin", ["Sø", "Ma", "Ti", "On", "To", "Fr", "Lø"]);

    $("#datepickerTo, #datepickerFrom").datepicker({ monthNamesShort: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"] });
    var monthNamesShort = $("#datepickerTo, #datepickerFrom").datepicker("option", "dayNamesMin");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "monthNamesShort", ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"]);

    $("#datepickerTo, #datepickerFrom").datepicker({ monthNames: ["Januar", "Februar", "Marts", "April", "Maj", "Juni", "Juli", "August", "September", "Oktober", "November", "December"] });
    var dayNamesMin = $("#datepickerTo, #datepickerFrom").datepicker("option", "monthNames");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "monthNames", ["Januar", "Februar", "Marts", "April", "Maj", "Juni", "Juli", "August", "September", "Oktober", "November", "December"]);

    $("#datepickerTo, #datepickerFrom").datepicker({ gotoCurrent: true });
    var gotoCurrent = $("#datepickerTo, #datepickerFrom").datepicker("option", "gotoCurrent");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "gotoCurrent", true);

    $("#datepickerTo, #datepickerFrom").datepicker({ firstDay: 1 });
    var firstDay = $("#datepickerTo, #datepickerFrom").datepicker("option", "firstDay");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "firstDay", 1);

    $("#datepickerTo, #datepickerFrom").datepicker({ hideIfNoPrevNext: true });
    var hideIfNoPrevNext = $("#datepickerTo, #datepickerFrom").datepicker("option", "hideIfNoPrevNext");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "hideIfNoPrevNext", true);

    $("#datepickerTo, #datepickerFrom").datepicker({ nextText: "Næste" });
    var nextText = $("#datepickerTo, #datepickerFrom").datepicker("option", "nextText");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "nextText", "Næste");

    $("#datepickerTo, #datepickerFrom").datepicker({ prevText: "Forrige" });
    var prevText = $("#datepickerTo, #datepickerFrom").datepicker("option", "nextText");
    $("#datepickerTo, #datepickerFrom").datepicker("option", "prevText", "Forrige");

    $("#datepickerTo, #datepickerFrom").val(formatDate(new Date().toString()));

    $.datepicker.formatDate = function (format, value) {
        return moment(new Date(value)).format('L');
    };

}


// $('#ui-datepicker-div').css("z-index", "9999 !important")
$("#datepickerTo, #datepickerFrom").attr('readonly', 'readonly');

function addTestCase() {
    $('#addTestCase').on('click', function () {
        var dataEdit = $(this).attr('data-edit');
        var dataId = $(this).attr('data-Id');

        var title = $('#testCaseLabel').val();
        var desc = $('#testCaseDesc').val();
        var graphId = $('#graphIdDropdown option:selected').attr('id');
        var roles = $('#rolesDropdown').val();
        var delay = $('#timeDelay').val();
        var f = parseDate($("#datepickerFrom"));
        var t = parseDate($("#datepickerTo"));
        
        if (title === "" || title === undefined || title === null) {
            App.showErrorMessage(translations.Please_enter_title);
            return false;
        }
        else if (desc === "" || desc === undefined || desc === null) {
            App.showErrorMessage(translations.Please_enter_description);
            return false;
        }
        else if (f === "" || f === undefined || f === null ) {
            App.showErrorMessage(translations.Please_select_date_from);
            return false;
        }
        else if (t === "" || t === undefined || t === null) {
            App.showErrorMessage(translations.Please_select_date_to);
            return false;
        }
        else if (graphId === "" || graphId === undefined || graphId === null) {
            App.showErrorMessage(translations.Please_select_graph);
            return false;
        }
        else if (roles === "" || roles === undefined || roles === null) {
            App.showErrorMessage(translations.Please_select_role);
            return false;
        }
        else if (delay === "" || delay === undefined || delay === null) {
            App.showErrorMessage(translations.Please_select_delay);
            return false;
        }

        

        var data = {
            id: dataId,
            title: title,
            desc: desc,
            validTo: t,
            validFrom: f,
            graphId: graphId,
            roles: roles.toString(),
            delay: delay
        }
        if (dataEdit == "true")
            App.updateTestCase(data);
        else
            App.addTestCase(data);
    });
}

function validateTestCase(val) {
    // TODO: ahmed add translation here
    App.showErrorMessage("Please enter title");
    return false;
}

function parseDate(date) {
    var d = '';
    if (date.val().indexOf('/') > 0) {
        d = date.val().split("/");
        return new Date(d[2], d[0] - 1, parseInt(d[1]) + 1)
    }
    else if (date.val().indexOf('.') > 0) {
        d = date.val().split(".");
        return new Date(d[2], d[1] - 1, parseInt(d[0]) + 1);
    }

}

function sendInvite() {
    $('#sendInviteBtn').on('click', function (e) {
        var data = {
            email: $('#userEmail').val(),
            urlToLaunch: e.currentTarget.attributes.url.value,
            testId: e.currentTarget.attributes.testId.value
        }

        API.service('services/updateChild', data)
            .done(function (response) {
                $("#obsTextArea").attr("disabled", true);
                $(".obsSaveButton").toggleClass('hide');
                $(".obsBoxEdit").toggleClass('hide');
                $(".obsCancelButton").toggleClass('hide');
                $(".textCount").toggleClass('hide');
            })
            .fail(function (e) {
                App.showErrorMessage(e.responseJSON.ExceptionMessage);
                console.log(e);
            });

    })
}

function getRolesOnGraphDDChange() {
    $('#graphIdDropdown').on('change', function () {
        const graphId = $(this).find('option:selected').attr('id');
        getRolesForTestCases(graphId);
    });
}
function resetModal() {
    $('#buttonNewTestCase').on('click', function (e) {
        var modalInputs = $('#addNewTestCase')
        modalInputs.find("input,textarea,select")
            .val('')
            .end()
            .find("input[type=checkbox], input[type=radio]")
            .prop("checked", "")
            .end();
        $('#addTestCase').attr('data-key', translations.Add).attr('data-id', "0").html(translations.Add);
        $('#rolesDropdown').prop('selectedIndex', 0);
        $('#graphIdDropdown').prop('selectedIndex', 0);
        $('#testCaseHeader').attr('data-key', translations.CreateNewTestCase).html(translations.CreateNewTestCase);
        $('#rolesContent').css('display', 'none');
    })
}
function changedate(inputId, lableId) {
    var value = $('#' + inputId).val();
    var applyTo = $('#' + lableId)[0];
    applyTo.value = value;
    applyTo.textContent = value;
}

function getRolesForTestCases(graphId, roleToTest = null) {
    var data = { graphId: graphId };
    API.service('services/getProcessRoles', data).done(function (response) {
        var roles = JSON.parse(response);
        roles = App.skipAutoRoles(roles);
        const rolesList = roleToTest != null ? roleToTest.split(',') : [];
        if (roles.length) {
            var dropdown = $('#rolesDropdown');
            dropdown.empty();
            $(roles).map(function () {
                const titleVal = this.title;
                if (roleToTest != null) {
                    let isExist = false;

                    $(rolesList).map(function (item, value) {
                        if (value == titleVal) {
                            dropdown.append('<option id="' + titleVal + '" selected>' + titleVal + '</option>');
                            isExist = true;
                        }
                    });
                    if (!isExist)
                        dropdown.append('<option id="' + titleVal + '">' + titleVal + '</option>');
                }
                else
                    dropdown.append('<option id="' + titleVal + '">' + titleVal + '</option>');
            });
            $('#rolesContent').css('display', 'flex');
            dropdown.multiselect({
                allSelectedText: 'All',
                includeSelectAllOption: true
            }).multiselect('rebuild');

            rolesList == null ? dropdown.prop('selected', 0) : null;

        }
        else {
            $('#rolesDropdown').html('');
        }
    })
        .fail(function (e) {
            App.showExceptionErrorMessage(e)
        });;
}


