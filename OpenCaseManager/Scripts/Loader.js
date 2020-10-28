var loaderCount = 0;
var incit = 0;
var decit = 0;
var loaderDebug = false;

function incrementLoaderCount(source = "") {
    if (loaderDebug) {
        incit++;
        console.log("inc " + loaderCount + " -> " + (loaderCount + 1) + "    - " + incit + " from: " + source);
    }
    loaderCount++;
    $('#loadMe').show();
}

function decrementLoaderCount(source = "") {
    if (loaderDebug) {
        decit++;
        console.log("dec " + loaderCount + " -> " + (loaderCount - 1) + "    - " + decit + " from: " + source);
        if (loaderCount <= 0) alert(loaderCount + " is lower than 0 :O");
    }
    if (loaderCount > 0)
        loaderCount--;
    if (loaderCount == 0) {
        $('#loadMe').hide();
    }
}

jQuery.ajaxSetup({
    beforeSend: function (x) {
        if (loaderDebug) console.log(x);
        incrementLoaderCount("ajaxSetup");
    },
    complete: function () {
        decrementLoaderCount("ajaxSetup");
    },
    success: function () {
    }
});