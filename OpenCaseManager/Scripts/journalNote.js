function CreateJournalNoteView() {
    window.open("/JournalNote/Create", "", "width=1200,height=1200");
}

function formatDate(date) {
    var value = new Date(date);
    console.log(value);
    return value.getDate() + "/" + (value.getMonth()+1) + "/" + value.getFullYear();
}

$(function () {
    $("#datepicker").datepicker();
    $("#datepicker").datepicker("option", "dateFormat", "d/m/yy");
});

function changedate(inputId, lableId) {
    var value = $('#' + inputId).val();
    var applyTo = $('#' + lableId)[0];
    applyTo.value = value;
    applyTo.textContent = value;
}

