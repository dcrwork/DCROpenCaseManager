$(document).ready(function () {
    $('#exportDCRXMLLog').on('click', function () {
        var graphId = 1335;
        var from = null;
        var to = null;
        var isAccepting = false;
        var toXES = false;

        graphId = $('#graphId').val();
        from = $('#startFrom').val();
        to = $('#endTo').val();
        isAccepting = $('#isAccepting').is(':checked');
        toXES = $('#toXES').is(':checked');

        window.open('../File/DownloadDCRXMLLog?graphId=' + graphId + '&isaccepting=' + isAccepting + '&toXES=' + toXES + '&from=' + from + '&to=' + to + '', '_blank');
    });
});