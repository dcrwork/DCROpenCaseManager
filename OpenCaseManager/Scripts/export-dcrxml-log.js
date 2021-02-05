$(document).ready(function () {
    $('#exportDCRXMLLog, button[name="exportTestCaseModal"]').on('click', function () {
        var graphId = 1335;
        var from = null;
        var to = null;
        var isAccepting = false;
        var toXES = false;
        var testId = null;

        graphId = $('#graphId').val();
        from = $('#startFrom').val();
        to = $('#endTo').val();
        isAccepting = $('#isAccepting').is(':checked');
        toXES = $('#toXES').is(':checked');
        testId = $(this).attr('testId') != undefined ? $(this).attr('testId') : null;

        window.open('../File/DownloadDCRXMLLog?graphId=' + graphId + '&isaccepting=' + isAccepting + '&toXES=' + toXES + '&from=' + from + '&to=' + to+ '&testId=' + testId + '', '_blank');
    });
});