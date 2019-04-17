function addDocument() {
    dropArea = document.getElementById("drop-area");
    uploadFiles = new Array();
    initializeDragnDrop();
    getDocuments();

    // To create a template funtion
    $('body').on('click', 'div table.tasksTable tbody td span[name="deleteDoc"]', function () {

        elem = $(this);

        App.showConfirmMessageBox(translations.DeleteDocumentMessage, translations.Yes, translations.No, function () {

            var docId = elem.attr('documentId');
            deleteDocument(docId);

        }, null, translations.DeleteDocument + '?');
    })

    // To create a template funtion
    $('body').on('click', 'div table.tasksTable tbody td span[name="editDoc"]', function () {
        elem = $(this);
        var docId = elem.attr('documentId');
        getDocumentDetails(docId);
    })

    // To create a template funtion
    $('body').on('click', 'table.tasksTable tbody td a[name="downloadDoc"]', function () {
        elem = $(this);
        var link = elem.attr('documentLink');
        var win = window.open(window.location.origin + "/File/DownloadFile?link=" + link, '_blank');
        win.focus();
    })


    console.log("addDocument.js called");
    initializeForm();
    $('#addNewDocumentModal').modal('toggle');
    isAdd = true;
    $('#documentName').focus();
    $('.instanceModalHeading').text(translations.AddDocument);
    $('#addDocument').text(translations.Add);
}
/**
function addDocument() {
    console.log("addDocument.js called");
    initializeForm();
    $('#addNewDocumentModal').modal('toggle');
    isAdd = true;
    $('#documentName').focus();
    $('.instanceModalHeading').text(translations.AddDocument);
    $('#addDocument').text(translations.Add);
    
}
*/