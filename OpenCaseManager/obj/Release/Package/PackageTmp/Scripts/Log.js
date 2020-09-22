(function (window) {

    function getLogs() {
        var query = {
            "type": "SELECT",
            "entity": "Log",
            "resultSet": [
                "Id", "Logged", "Level", "UserName", "ServerName", "Port", "Url", "Https", "Message", "Exception", "XML"
            ],
            "order": [{ "column": "Logged", "descending": true }]
        };

        API.service('records', query)
            .done(function (response) {
                var data = JSON.parse(response);
                var html = '';
                for (var i = 0; i < data.length; i++) {
                    html += '<tr><td>' + data[i].Id + '</td>' +
                        '<td>' + data[i].Logged + '</td>' +
                        '<td>' + (data[i].Message === null ? '&nbsp;' : data[i].Message) + '</td > ' +
                        '<td class="comment">' + (data[i].Exception === null ? '&nbsp;' : data[i].Exception) + '</td > ' +
                        '<td>' + (data[i].XML === null ? '&nbsp;' : data[i].XML) + '</td > ' +
                        '</tr>';
                }
                $('#logs').html(html);
                $('#logTable').DataTable({
                    "paging": true,
                    "info": true,
                    "order": [[1, "desc"]]
                });
                $(".comment").shorten();

                $('#logTable')
                    .on('draw.dt', function () {
                        console.log('test');
                        $(".comment").shorten();
                    });
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }


    var Logs = function () {
        this.getLogs = getLogs;


    };
    return window.logs = new Logs;
}(window));
