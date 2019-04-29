
$(document).ready(function () {
    $('#createInstance').on('click', function (e) {

        var title = $('#instanceTitle').val();
        var graphId = $('#instanceProcesses').find(":selected").val();

        var userRoles = new Array()
        $('select[name="multi-select"]').each(function (index, select) {
            var userRole = { roleId: $(select).attr('userid'), userId: '' }
            $(select.selectedOptions).each(function (index, item) {
                if (item.value !== '0')
                    userRole.userId = item.value;
            })
            if (userRole.userId !== '')
                userRoles.push(userRole)
        })

        if (title !== '' && graphId > 0) {
            App.addInstance(title, graphId, userRoles);
            App.getMyInstances();
            $('#addNewInstance').modal('toggle');
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });

    var promise = new Promise(function (resolve, reject) {
        App.responsible(resolve);
    });

    promise.then(function (result) {
        App.getProcess(true, true);
        App.getMyInstances();
        App.getTasks();
    }, function (err) {
        console.log(err); // Error: "It broke"
    });

    $('#instanceProcesses').on('change', function () {
        var graphId = $('#instanceProcesses').find(":selected").val();
        App.getRoles(graphId);
    });
});