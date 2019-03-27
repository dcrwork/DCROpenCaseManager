function CreateJournalNoteView() {
    var myWindow = window.open("/JournalNote/Create", "", "width=1200,height=1200");
}

function formatDate(date) {
    var value = new Date(date);
    return `${value.getDate()}/${value.getMonth()}/${value.getFullYear()}`;
}

function changedate(inputId, lableId) {
    var value = $('#' + inputId).val();
    var applyTo = $('#' + lableId)[0];
    applyTo.value = formatDate(value).toString();
    applyTo.textContent = formatDate(value).toString();
}


if (window.location.pathname === "/journalnote/create") {
    var header = $("header")[0];
    header.style.display = 'none';
}