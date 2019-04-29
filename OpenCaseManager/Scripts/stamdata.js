

$(function () {

    async function setStamDataContent(childId) {

        var result = await getStamData(childId)
        console.log(result);
        var firstElement = result[0];
        
        var sagsnrp = $("<p>").text(firstElement.Sagsnummer);
        var navnp = $("<p>").text(firstElement.Name);
        var addressep = $("<p>").text(firstElement.Addresse);
        var forældremyndighedp = $("<p>").text(firstElement.Forældremyndighed);
        var skolep = $("<p>").text(firstElement.Skole);
        var alderp = $("<p>").text(firstElement.Alder);
     

        $(".sagsnr").append(sagsnrp);
        $(".name").append(navnp);
        $(".address").append(addressep);
        $(".parentalrights").append(forældremyndighedp);
        $(".school").append(skolep);
        $(".age").append(alderp);

        var i;
        for (i = 0; i < result.length; i++) {

            var current = result[i];

            var div = $("<div></div>");

            var role = $("<h5></h5>");
            role.append($("<b></b>").text(current.Relation));
            div.append(role);

            var cpr = $("<p></p>");
            cpr.text(current.CPR);
            div.append(cpr);

            var name = $("<p></p>");
            name.text(current.StamdataName);
            div.append(name);

            var address = $("<p></p>");
            address.text(current.Address);
            div.append(address);

            var city = $("<p></p>");
            city.text(current.Postcode + " " + current.City);
            div.append(city);
            $(".expandedStamdata").append(div);
        }

    }

    var childId = getParameterByName("id", window.location.href);

    setStamDataContent(childId);

   /*
    <h5><b>Sagsnummer</b></h5> <!--AcadreSagsnummer -->
        <p>#73283</p>
        <h5><b>Navn</b></h5>
        <p>Louise Nielsen</p>
        <h5><b>Adresse</b></h5>
        <p>Gade 2, 2740 Skovlunde</p>
        <h5><b>Forældremyndighed</b></h5>
        <p>Fælles</p>
        <h5><b>Skole</b></h5>
        <p>Holmegårdskolen</p>
        <h5><b>Alder</b></h5>
        <p>13</p>
*/


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

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}