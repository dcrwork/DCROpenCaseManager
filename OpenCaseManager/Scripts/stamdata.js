$(function () {

    async function setStamDataContent(id, isInstance) {

        var x = id;

        if (isInstance) {
            var result2 = await getChildId(id);
            var y = result2[0];
            x = y.ChildId;
        }

        var result = await getStamData(x);
        var firstElement = result[0];

        var sagsnrt = firstElement.Sagsnummer == null ? "" : firstElement.Sagsnummer;
        var navnt = firstElement.Name == null ? "" : firstElement.Name;
        var addresset = firstElement.Addresse == null ? "" : firstElement.Addresse;
        var forældremyndighedt = firstElement.Forældremyndighed == null ? "" : firstElement.Forældremyndighed;
        var skolet = firstElement.Skole == null ? "" : firstElement.Skole;
        var aldert = firstElement.Alder == null ? "" : firstElement.Alder;
        
        var sagsnrp = $("<p>").text(sagsnrt);
        var navnp = $("<p>").text(navnt);
        var addressep = $("<p>").text(addresset);
        var forældremyndighedp = $("<p>").text(forældremyndighedt);
        var skolep = $("<p>").text(skolet);
        var alderp = $("<p>").text(aldert);

        $(".sagsnr").after(sagsnrp);
        $(".name").after(navnp);
        $(".address").after(addressep);
        $(".parentalrights").after(forældremyndighedp);
        $(".school").after(skolep);
        $(".age").after(alderp);

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

    var id = getParameterByName("id", window.location.href);
    var isInstance = false;
    var windowLocation = window.location;
    var pname = windowLocation.pathname.toLowerCase();
    if (pname == "/childinstance") {
        isInstance = true;
    }
    setStamDataContent(id, isInstance);
})



async function getStamData(childId) {
    var query = {
        "type": "SELECT",
        "entity": "StamdataView",
        "resultSet": ["*"],
        "filters": new Array(),
        "order": []
    }

    var whereChildIdMatchesFilter = {
        "column": "Id",
        "operator": "equal",
        "value": childId,
        "valueType": "int",
        "logicalOperator": "and"
    };
    query.filters.push(whereChildIdMatchesFilter);

    var result = await API.service('records', query);
    return JSON.parse(result)
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
    return JSON.parse(result)}


function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}
