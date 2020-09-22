$(document).ready(function () {
    getMenuItems();
});

function getMenuItems() {
    API.serviceGET('Records/GetMenuItems')
        .done(function (response) {
            displayMenu(response);
        })
        .fail(function (e) {
            App.showErrorMessage(e.responseJSON.ExceptionMessage);
            console.log(e);
        });
}

function displayMenu(response) {
    try {
        var result = JSON.parse(response);
        var list = "<li><span><a><i class='glyphicon glyphicon-search' onclick='toggleSearch(this)'></i></a></span></li>";
        if (result.length === 0) {
            list += "<li><a href='/'><span data-locale data-apply='text' data-key='Home'>Home</span ></a ></li >";
        }
        else {
            for (i = 0; i < result.length; i++) {
                list += getMenuItemHtml(result[i]);
            }
        }
        $("#menu-header-ul-items").html("").append(list);
    }
    catch (e) {
        App.showErrorMessage(e);
    }
    App.setTexts();
}

function getMenuItemHtml(item) {
    var url = item.RelativeURL;
    var Title = item.DisplayTitle;
    var dataKey = item.DisplayTitle.replace(' ', '');
    var returnHtml = "<li>";
    returnHtml += "<a href='" + url + "'>";
    returnHtml += "<span data-locale data-apply='text' data-key='" + dataKey + "'>" + Title + "</span >";
    returnHtml += "</a>";
    returnHtml += "</li>";
    return returnHtml;
}