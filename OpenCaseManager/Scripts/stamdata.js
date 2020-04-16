
function calculateAge(birthday) { // birthday is a date
    var ageDifMs = Date.now() - birthday.getTime();
    var ageDate = new Date(ageDifMs); // miliseconds from epoch
    return Math.abs(ageDate.getUTCFullYear() - 1970);
}
$(function () {

    async function setStamDataContent(id, isInstance) {

        var x = id;

        if (isInstance) {
            var result2 = await getChildId(id);
            var y = result2[0];
            if (y == null) return;
            x = y.ChildId;
        }

        var result = await getStamData(x);
        var firstElement = result;
        if (firstElement == null) return;

        var sagsnrt = firstElement.CaseNumberIdentifier == null ? "" : firstElement.CaseNumberIdentifier;
        var navnt = firstElement.SimpleChild.FullName == null ? "" : firstElement.SimpleChild.FullName;
        var addresset = firstElement.SimpleChild.EnvelopeAddress == null ? "" : firstElement.SimpleChild.EnvelopeAddress;
        var forældremyndighedt = firstElement.CustodyOwnersNames == null ? [""] : firstElement.CustodyOwnersNames;
        var skolet = firstElement.SchoolName == null ? "" : firstElement.SchoolName;
        var aldert = firstElement.SimpleChild.Age == null ? "" : firstElement.SimpleChild.Age;
        var cpr = aldert;
        if (aldert != null) {
            var year = 1.0 * aldert.substr(4, 2);
            var aDate = new Date((year > 60 ? '19' : '20') + (year < 10 ? '0' : '') + year, 1.0 * aldert.substr(2, 2) - 1, aldert.substr(0, 2));
            aldert = '' + calculateAge(aDate);
        }
        var Mom = firstElement.Mom == null ? "" : firstElement.Mom;
        var MomData = ''
        for (var i = 0; i < Mom.length; i++) {
            MomData = MomData + Mom[i].CPR + '</br>' + Mom[i].FullName + '</br>' + Mom[i].EnvelopeAddress + '</br></br>';
        }
        var Dad = firstElement.Dad == null ? "" : firstElement.Dad;
        var Daddata = ''
        for (var i = 0; i < Dad.length; i++) {
            Daddata = Daddata + Dad[i].CPR + '</br>' + Dad[i].FullName + '</br>' + Dad[i].EnvelopeAddress + '</br></br>';
        }
        if (!isInstance) {
            var prefix = await App.getKeyValue('AcadreFrontEndBaseURL');
            $('#childCaseNo').text(firstElement.CaseNumberIdentifier);
            $('.caseLink').show();
            $('#entityLink').attr('href', prefix + '/Case/Details?caseId=' + firstElement.CaseID);
        }
        var SiblingsAktivSag = 0;
        var Siblings = firstElement.Siblings == null ? "" : firstElement.Siblings;
        var SiblingsData = ''
        for (var i = 0; i < Siblings.length; i++) {
            //            SiblingsData = SiblingsData + Siblings[i].CPR + '</br>' + Siblings[i].FullName + '</br>' + Siblings[i].EnvelopeAddress + '</br></br>';
            SiblingsData = SiblingsData + Siblings[i].SimpleChild.CPR + '</br>' + Siblings[i].SimpleChild.FullName + '</br>';
            if (Siblings[i].SimpleChild.EnvelopeAddress != '')
                SiblingsData = SiblingsData + Siblings[i].SimpleChild.EnvelopeAddress + '</br>';
            if (Siblings[i].CaseID > 0) {
                SiblingsData = SiblingsData + '<a href="/Child?id=' + Siblings[i].CaseID + '">' + Siblings[i].CaseNumberIdentifier + '</a><br>';
                if (!Siblings[i].CaseIsClosed) SiblingsAktivSag = 1;
            }
        }

        if (firstElement.Note != null) {
            $("#obsTextArea").val(firstElement.Note);
        }
        //var aldert = firstElement.Alder == null ? "" : firstElement.Alder;
        //var aldert = firstElement.Alder == null ? "" : firstElement.Alder;

        var sagsnrp = $("<p>").text(sagsnrt);
        sagsnrp.attr('id', 'instanceCaseNo');
        var cprp;
        cprp = $("<p>").text(cpr);
        if (SiblingsAktivSag != 0) {
            var xx = $("<p>").html('<b style="color:#9C1A5E">Aktiv søskende sag</b>');
            $(".cpr").before(xx);
        }

        var navnp = $("<p>").text(navnt);
        navnp.attr('id', 'childName');
        var addressep = $("<p>").text(addresset);
        var forældremyndighedp = $("<p>").html(forældremyndighedt.join("<br>"));
        var skolep = $("<p>").text(skolet);
        skolep.attr('id', 'schoolName');
        var alderp = $("<p>").text(aldert);
        getChildSchoolName(cpr);

        $(".sagsnr").after(sagsnrp);

        if (!debugMode) {
            $(".cpr").after(cprp);
            $(".name").after(navnp);
            $(".address").after(addressep);
            $(".parentalrights").after(forældremyndighedp);
            $(".school").after(skolep);
            $(".age").after(alderp);
            $(".Mordata").after(MomData);
            $(".Fardata").after(Daddata);
            $(".Søskendedata").after(SiblingsData);

            var i;
            for (i = 0; i < result.length; i++) {

                var current = result[i];

                var div = $("<div></div>");

                if (current.Relation == null) {
                    var nothingFound = $("<h5></h5>");
                    nothingFound.append($("<b></b>").text("Nothing found"));
                    div.append(nothingFound);
                    $(".expandedStamdata").append(div);
                    continue;
                }

                var role = $("<h5 class='h5stamdata'></h5>");
                role.append($("<b></b>").text(current.Relation));
                div.append(role);

                var cpr = $("<p class='pstamdata'></p>");
                cpr.text(current.CPR);
                div.append(cpr);

                var name = $("<p class='pstamdata'></p>");
                name.text(current.StamdataName);
                div.append(name);

                var address = $("<p class='pstamdata'></p>");
                address.text(current.Address);
                div.append(address);

                var city = $("<p class='pstamdata'></p>");
                city.text(current.Postcode + " " + current.City);
                div.append(city);
                $(".expandedStamdata").append(div);
            }
        }

        // Breadcrumb
        if (debugMode) {
            var i = navnt.indexOf(' ');
            navnt = navnt.substr(0, i);
        }
        if (isInstance) {
            var o = document.getElementById('instanceTitle');
            var instanceName = o.innerText;
            setInstancePageBreadcrumbX(navnt, x, instanceName);
            if (typeof (DoHeader) == 'function')
                DoHeader(null);
        }
        else {
            displayChildNameX(navnt);
            setChildPageBreadcrumbX(navnt, x);
            setChildResponsible(firstElement, id);
        }
    }

    var id = getParameterByName("id", window.location.href);
    var isInstance = false;
    var windowLocation = window.location;
    var pname = windowLocation.pathname.toLowerCase();
    if (pname == "/adjunktinstance") {
        isInstance = true;
    }
    setStamDataContent(id, isInstance);
})

async function getStamData(childId) {
    try {
        var query = {
            "JournalCaseId": childId
        };

        //var result = await API.service('services/getChildInfo', query);
        return JSON.parse(result)
    }
    catch (e) {
        App.showErrorMessage(e.responseText);
    }
}

async function getChildId(instanceId) {
    var query = {
        "type": "SELECT",
        "entity": "InstanceExtension",
        "resultSet": ["ChildId"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "InstanceId",
        "operator": "equal",
        "value": instanceId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);

    var result = await API.service('records', query);
    return JSON.parse(result)
}

function getChildSchoolName(cpr) {
    var query = {
        "type": "SELECT",
        "entity": "Skoleliste",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "CPR",
        "operator": "equal",
        "value": cpr,
        "valueType": "string",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);

    API.service('records', query)
        .done(function (response) {
            var result = JSON.parse(response);
            if (result.length > 0)
                $('#schoolName').text(result[0].Skole);
            else
                $('#schoolName').text('Ukendt skole');

        })
        .fail(function (e) {
            var sMessage = 'An error happened';
            if (e.responseJSON != null)
                sMessage = e.responseJSON.ExceptionMessage;
            else if (e.responseText != null)
                sMessage = e.responseText;
            App.showErrorMessage(sMessage);
        });
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

