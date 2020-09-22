// on document ready initialization
$(document).ready(function () {
    $('#cancelChangeResponsibleModal').on('click', function () {
        $('#responsible-modal').modal('toggle');
    });

    $('body').on('click', '.responsibleSelectOptions', function (e) {
        var query = {
            "type": "SELECT",
            "entity": "UserDetail",
            "resultSet": ["Name", "Id", "sAMAccountName"],
            "order": [{ "column": "name", "descending": false }]
        };

        API.service('records', query)
            .done(function (response) {
                var responsibles = JSON.parse(response);
                var element = $(e.currentTarget);

                $('#instanceActivityTitle').text($(element).attr('itemTitle'));
                $('#responsible-modal').modal('toggle');
                $('#changeResponsibleOptions').empty();

                for (i = 0; i < responsibles.length; i++) {
                    $('#changeResponsibleOptions').append('<option value="' + responsibles[i].Id + '" >' + responsibles[i].Name + '</option>');
                    //                    $('#changeResponsibleOptions').append('<option value="' + responsibles[i].sAMAccountName + '" >' + responsibles[i].Name + '</option>');
                }

                var eventId = $(element).attr('itemEventId');
                var instanceId = $(element).attr('itemInstanceId');
                var itemResponsible = $(element).attr('itemResponsible');
                var childId = $(element).attr('itemChildId');
                var changeResponsibleFor = $(element).attr('changeResponsibleFor');
                var oldResponsible = $(element).attr('oldResponsible');

                if (eventId != null) {
                    $('#chanageResponsibleTitle').text(translations.ChangeResponsibleOfActivity);
                }
                if (instanceId != null) {
                    $('#chanageResponsibleTitle').text(translations.ChangeResponsibleOfActivities);
                }
                if (childId != null) {
                    $('#chanageResponsibleTitle').text(translations.ChangeResponsibleOfChild);
                }

                $('#instanceIdChangeResponsible').val(instanceId);
                $('#eventIdChangeResponsible').val(eventId);
                $('#childIdChangeResponsible').val(childId);
                $('#changeResponsibleFor').val(changeResponsibleFor);
                $('#oldResponsible').val(oldResponsible);
                $('#curResponsible').attr('placeholder', itemResponsible);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    });

    // change responsible button event
    $('#changeResponsible').on('click', function (e) {
        var query = {
            "trueEventId": $('#eventIdChangeResponsible').val(),
            "instanceId": $('#instanceIdChangeResponsible').val(),
            "responsible": $('#changeResponsibleOptions').val(),
            "childId": $('#childIdChangeResponsible').val(),
            "changeFor": $('#changeResponsibleFor').val(),
            "oldResponsible": $('#oldResponsible').val()
        };

        API.service('records/UpdateResponsible', query)
            .done(function (response) {
                $('#responsible-modal').modal('toggle');
                window.location.reload();
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e.responseJSON.ExceptionMessage);
            });
    });
});