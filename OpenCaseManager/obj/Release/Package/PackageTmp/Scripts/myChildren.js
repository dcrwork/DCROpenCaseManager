$(document).ready(function () {

    SearchChildren();

    $('#addbuttonDiv').on('click', function (e) {
        $('#child-CPRnummer').val('');
        $('#Barnets-navn').val('');
        $('#add-child-modal').modal('toggle');

    });

    $('#add-child').on('click', function (e) {
        var CPRnummer = $('#child-CPRnummer').val();
        var childName = $('#Barnets-navn').val();

        if (CPRnummer !== '' && childName !== '') {
            //App.addChild(childName, caseNumber, responsible);
            var query = {
                ChildCPR: CPRnummer,
                AcadreOrgID: 274,
                CaseManagerInitials: "$(loggedInUser)"
            };

            API.service('services/CreateChildJournal', query)
                .done(function (response) {
                    // 15.5.2019 - MM - parse result of child id, if -1
                    var result = JSON.parse(response);
                    window.location.reload(false);
                })
                .fail(function (e) {
                    App.showErrorMessage(e.responseText);
                    console.log(e);
                });

        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });

    $('#add-getName').on('click', function (e) {
        var CPRnummer = $('#child-CPRnummer').val();
        CPRnummer = CPRnummer.replace('-', '');
        if (CPRnummer.length != 10)
            return false;
        if (CPRnummer !== '') {
            var query = {
                "CPR": CPRnummer
            };

            API.service('services/getChildInfo', query)
                .done(function (response) {
                    var result = JSON.parse(response);
                    if (result.CaseID == 0) {
                        $('#Barnets-navn').val(result.SimpleChild.FullName);
                        $("#add-child").removeClass("hide")
                    }
                    else {
                        App.showWarningMessage('Der eksisterer en lukket sag på ' + result.SimpleChild.FullName + ', kontakt venligs Acadre Superbruger for genåbning.');
                        $('#Barnets-navn').val('Barn findes allerede');
                    }
                })
                .fail(function (e) {
                    $('#Barnets-navn').val(' ');
                    App.showErrorMessage('Ukendt CPR<br>' + e.responseText);
                    console.log(e);
                });
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });

    onLoad();
});

function SearchChildren(query) {
    if (query == undefined) {
        query = 0;
        var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (m, key, value) {
            query = value;
        });
    }

    if (query == 0) {
        var data = {
            "type": "SELECT",
            "entity": "[User]",
            "resultSet": ["Id"],
            "filters": [
                {
                    "column": "Id",
                    "operator": "equal",
                    "value": "$(loggedInUserId)",
                    "valueType": "string",
                }
            ]
        }

        API.service('records', data)
            .done(function (response) {
                var user = JSON.parse(response);
                API.service('records/GetAdjunkter', { AdjunktId: user[0].Id }, 30000)
                    .done(function (response) {
                        displayChildren(response);
                    })
                    .fail(function (e) {
                        if (e.statusText == 'timeout')
                            App.showErrorMessage('timeout');
                        else
                            App.showErrorMessage(e.responseJSON.ExceptionMessage);
                        console.log(e);
                    });
            })
            .fail(function (e) {
                showExceptionErrorMessage(e)
            });
    } else {
        API.service('records/GetAdjunkter', { AdjunktId: query }, 30000)
            .done(function (response) {
                displayChildren(response);
            })
            .fail(function (e) {
                if (e.statusText == 'timeout')
                    App.showErrorMessage('timeout');
                else
                    App.showErrorMessage(e.responseJSON.ExceptionMessage);
                console.log(e);
            });
    }
}

function displayChildren(response) {
    try {
        var result = JSON.parse(response);
        var list = "";
        var childList = "";
        var iCount = 0;
        if (result.length === 0) {
            list = "<tr class='trStyleClass'><td colspan='100%'>" + translations.NoRecordFound + " </td></tr>";
            $("#myChildrenTitle").html("Adjunkter");
        } else {
            for (i = 0; i < result.length; i++) {
                if (!result[i].IsClosed) {
                    list += getAdjunktInstanceHtml(result[i]);
                    childList += result[i].Id + ",";
                    iCount += 1;
                }
            }
            $("#myChildrenTitle").html("Adjunkter (" + (iCount) + ")");
        }
        // for sorting purposes
        // clear all previous enteries and taking a template table into new div
        // and adding new rows to that div

        $('#divTableContainerMyChildren').html('');
        var tableHtml = $('#divTableContainerMyChildrenHidden').html();
        $('#divTableContainerMyChildren').html(tableHtml);
        $('#divTableContainerMyChildren').children('table').attr('id', 'myChildrenTable');
        $('#myChildrenTable').children('tbody').attr('id', 'allMyChildren');
        $("#allMyChildren").html(list);

        getChildStatus(childList.substr(0, childList.length - 1));
    }
    catch (e) {
        App.showErrorMessage(e);
    }
}

function getChildStatus(childList) {
    query = {
        "type": "SELECT",
        "entity": "GetDataForChildren('" + childList + "')",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": []
    }

    API.service('records', query)
        .done(function (response) {
            updateChildStatus(response);

            $('#myChildrenTable').sortable({
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
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseText);
            console.log(e);
        });
}

function updateChildStatus(response) {
    var result = JSON.parse(response);
    var status;
    for (var i = 0; i < result.length; i++) {
        var o = document.getElementById('trchild' + result[i].ChildId);
        if (o == null) { // 26.8.2019
            console.log('Child id missing ' + result[i].ChildId);
        }
        else {
            status = getStatus(result[i]);
            o.cells[0].innerHTML = status;
            o.cells[2].innerHTML = "<div title='" + result[i].SumOfEvents + " afventende aktiviteter'>" + result[i].SumOfEvents + "</div>";
            o.cells[3].innerHTML = "<div>" + result[i].SumOfEvents + "</div>";
            var statusClass = $(status).attr('class');
            switch (statusClass) {
                case 'dot dotRed':
                    o.cells[1].innerHTML = 0;
                    break;
                case 'dot dotYellow':
                    o.cells[1].innerHTML = 1;
                    break;
                case 'dot dotGreen':
                    o.cells[1].innerHTML = 2;
                    break;
                case 'dot dotGrey':
                    o.cells[1].innerHTML = 3;
                    break;
                default:
                    o.cells[1].innerHTML = 4;
                    break;
            }
            if (result[i].NextDeadline == null)
                o.cells[6].innerHTML = "Ingen kommende frister";
            else {
                var objDate = new Date(result[i].NextDeadline);
                var date = objDate.getDate() + '/' + (objDate.getMonth() + 1) + '-' + objDate.getFullYear();
                var time = addZero(objDate.getHours()) + ":" + addZero(objDate.getMinutes());
                o.cells[6].innerHTML = date + " " + time;
                o.cells[7].innerHTML = objDate;
            }
        }
    } // 26.8.2019
}

function addZero(i) {
    if (i < 10) {
        i = "0" + i;
    }
    return i;
}
function GetName(s) {
    if (debugMode) {
        var i = s.indexOf(' ');
        if (i > 0)
            s = s.substr(0, i) + ' Xxxxxxxx';
    }
    return s;
}
function FormatCPR(s) {
    if (s.length == 10)
        s = s.substr(0, 6) + '-' + s.substr(6, 4);
    if (debugMode)
        s = s.substr(0, 6) + '-XXXX';
    return s;
}

function getAdjunktInstanceHtml(item) {
    var childLink = "../Adjunkt?id=" + item.Id;

    var returnHtml = "<tr class='trStyleClass' id='trchild" + item.Id + "'>";
    returnHtml += "<td><span class='dot dotGrey' title='...'></span></td>";
    returnHtml += "<td style='display:none;'>4</td>";
    returnHtml += "<td>" + '' + "</td>";
    returnHtml += "<td style='display:none;'>" + '' + "</td>";
    returnHtml += "<td><a class='linkStyling' href='" + childLink + "'>" + item.Name + "</a></td>";
    //returnHtml += "<td>" + FormatCPR("") + "</td>";
    if (item.CaseManagerInitials != undefined) {
        returnHtml += '<td><a href="#" class="linkStyling responsibleSelectOptions" oldresponsible="' + item.CaseManagerInitials + '" itemResponsible="' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '" itemTitle="' + item.ChildName + '" itemchildid="' + item.CaseID + '" changeResponsibleFor="child">' + item.CaseManagerName.substr(0, 1).toUpperCase() + item.CaseManagerName.substr(1) + '</a></td>';
    } else {
        returnHtml += '<td><a href="#" class="linkStyling responsibleSelectOptions" itemResponsible="' + item.ResponsibleName.substr(0, 1).toUpperCase() + item.ResponsibleName.substr(1) + '" itemTitle="' + item.Name + '" itemchildid="' + item.Id + '" changeResponsibleFor="child">' + item.ResponsibleName.substr(0, 1).toUpperCase() + item.ResponsibleName.substr(1) + '</a></td>';

    }
    returnHtml += "<td></td>";
    returnHtml += "<td style='display:none'>" + new Date('1002-01-01') + "</td>";
    returnHtml += "</tr>";

    return returnHtml;
}

function getStatus(item) {
    var deadline = item.NextDeadline;
    var sumOfEvents = item.SumOfEvents;
    if (deadline != null) {
        var now = new Date().getTime();
        deadline = new Date(deadline).getTime();
        if (now >= deadline) {
            return "<span title='Frist overskredet' class='dot dotRed'></span>";
        } else if (now + 604800000 >= deadline) {
            return "<span title='Frist snart overskredet' class='dot dotYellow'></span>";
        }

        if (deadline == null && sumOfEvents > 0) {
            return "<span title='Ingen kommende frister' class='dot dotGreen'></span>";
        }
    }

    return "<span title='Ingen kommende frister' class='dot dotGreen'></span>";
}

function getChild() {
    var oEle = document.getElementById('navnny');
    oEle.style.display = 'none';
    oEle = document.getElementById('opretBarnBtn');
    oEle.style.display = 'none';
    var oEle = document.getElementById('cprny');
    var CPRnummer = oEle.value.replace('-', '');
    if (CPRnummer.length != 10 && $.isNumeric(CPRnummer)) {
        App.showWarningMessage('CPR-nr skal være 10 cifre');
        return
    }

    var query;
    query = {
        ChildCPR: CPRnummer,
        CaseManagerInitials: null,
        CaseContent: "Løbende journal*"
    };
    if (!$.isNumeric(CPRnummer)) {
        query = {
            CaseManagerInitials: null,
            //		PrimaryContactsName: CPRnummer.replace(/ /g,'*') + '*',
            PrimaryContactsName: CPRnummer + '*',
            CaseContent: "Løbende journal*"
        };
    }
    API.service('Services/SearchChildren', query, 10000)
        .done(function (response) {
            var result = JSON.parse(response);
            if (result.length != 0) {
                displayChildren(response);
            }
            else if (!$.isNumeric(CPRnummer)) {
                App.showWarningMessage('Ingen børnesager fundet for: "' + CPRnummer + '"');
            }
            else {
                var query = {
                    "CPR": CPRnummer
                };

                API.service('services/getChildInfo', query)
                    .done(function (response) {
                        var result = JSON.parse(response);
                        var oEle = document.getElementById('navnny');
                        oEle.style.display = '';
                        oEle = document.getElementById('opretBarnBtn');
                        if (result.CaseID == 0) {
                            oEle.style.display = '';
                            $('#navnny').val(result.SimpleChild.FullName);
                        }
                        else {
                            oEle.style.display = 'none';
                            App.showWarningMessage('Der eksisterer en lukket sag på ' + result.SimpleChild.FullName + ', kontakt venligs Acadre Superbruger for genåbning.');
                            $('#navnny').val('Barn findes allerede');
                        }
                    })
                    .fail(function (e) {
                        $('#Barnets-navn').val(' ');
                        App.showErrorMessage('Ukendt CPR<br>' + e.responseText);
                        console.log(e);
                    });
            }
        })
        .fail(function (e) {
            if (e.statusText == 'timeout')
                App.showWarningMessage('Resulatet er for bredt. Giv flere detaljer om navnet "' + CPRnummer + '"');
            else
                App.showErrorMessage(e.responseJSON.ExceptionMessage);
            console.log(e);
        });
}

async function opretBarn() {
    var query = {
        "type": "SELECT",
        "entity": "[User]",
        "resultSet": ["sAMAccountName", "AcadreOrgID"],
        "filters": new Array(),
        "order": []
    }

    var whereUserIdMatchesFilter = {
        "column": "sAMAccountName",
        "operator": "equal",
        "value": "$(loggedInUser)",
        "valueType": "string",
        "logicalOperator": "and"
    };
    query.filters.push(whereUserIdMatchesFilter);

    var result1 = await API.service('records', query);
    var EventOK = JSON.parse(result1);
    if (EventOK.length == 0) return;
    var AcadreOrgID = EventOK[0].AcadreOrgID;
    var CaseManagerInitials = EventOK[0].sAMAccountName;

    var oEle = document.getElementById('cprny');
    var CPRnummer = oEle.value.replace('-', '');
    query = {
        ChildCPR: CPRnummer,
        AcadreOrgID: AcadreOrgID,
        CaseManagerInitials: CaseManagerInitials
    };

    API.service('services/CreateChildJournal', query)
        .done(function (response) {
            // 15.5.2019 - MM - parse result of child id, if -1
            var result = JSON.parse(response);
            window.location.reload(false);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
            console.log(e);
        });

}

function visSager() {
    var selectedResponsible = $('#responsibleny option:selected').attr('id');

    SearchChildren(selectedResponsible);
}

function onLoad() {
    var query = {
        "type": "SELECT",
        "entity": "[User]",
        "resultSet": ["sAMAccountName", "Name", "Id"],
        "filters": new Array(),
        "order": [{ "column": "Name", "descending": false }]
    }
    /*
    var whereChildIdMatchesFilter = {
        "column": "Familieafdelingen",
        "operator": "equal",
        "value": 1,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);
    */
    API.service('records', query)
        .done(function (response) {
            fillDropDown(response);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseText);
        });
}

function fillDropDown(response) {
    var dropdown = $('#responsibleny');
    dropdown.empty();
    dropdown.append('<option selected="true" disabled>Vælg ansvarlig</option>');
    dropdown.prop('selectedIndex', 0);
    $(JSON.parse(response)).map(function () {
        dropdown.append('<option id="' + this.Id + '">' + this.Name + '</option>');
    });
}

$(document).on('keypress', function (e) {
    var code = (e.keyCode ? e.keyCode : e.which);
    if (code == 13) {
        getChild();
    }
});