async function getTimelineData(childId) {
    try {
        var query = {
            "JournalCaseId": childId
        };

        var result = await API.service('services/GetChildJournalDocuments', query);
        return JSON.parse(result);
    }
    catch (e) {
        App.showErrorMessage(e.responseJSON.ExceptionMessage);
    }
}