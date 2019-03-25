$(document).ready(function () {
    var app = window.App;

    async function setProcess() {
        var processData = JSON.parse(await app.getProcessAsync());
        var processesSelector = $("#processes");

        console.log(processData);

        processData.forEach(function (p) {
            processesSelector.append(app.getProcessHtml(p));
        });

        setRoles(processesSelector.children("option:selected").val());

        processesSelector.change(function () {
            var selectedGraphId = $(this).children("option:selected").val();
            console.log("id", selectedGraphId);
            setRoles(selectedGraphId);
        });
    }

    function setRoles(graphId) {
        app.getRoles(graphId, null, "responsible");
    }

    setProcess();

    
    $('#createProcess').on('click', function (e) {
        var title = $('#processTitle').val();
        var graphId = $('#processes').find(":selected").val();

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
            $('#createProcessModal').modal('toggle');
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });
});