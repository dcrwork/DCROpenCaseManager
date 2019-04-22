function addDocument() {
    initializeForm();
    $('#addNewDocumentModal').modal('toggle');
    isAdd = true;
    $('#documentName').focus();
    $('.instanceModalHeading').text(translations.AddDocument);
    $('#addDocument').text(translations.Add);
}

