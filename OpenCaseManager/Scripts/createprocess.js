$(document).ready(function () {
    var app = window.App;

    async function setProcess() {
        var processData = JSON.parse(await app.getProcessAsync());
        var processesSelector = $("#processes");

        processData.forEach(function (p) {
            processesSelector.append(app.getProcessHtml(p));
        });

        setRoles(processesSelector.children("option:selected").val());

        processesSelector.change(function () {
            var selectedGraphId = $(this).children("option:selected").val();
            setRoles(selectedGraphId);
        });
    }

    function setRoles(graphId) {
        app.getRoles(graphId, null, "responsible");
    }

    setProcess();

    
    $('#create-process').on('click', function (e) {
        var title = $('#process-title').val();
        var graphId = $('#processes').find(":selected").val();
        var childId = (window.location.pathname == "/child") ? App.getParameterByName("id", window.location.href) : null;

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
            App.addInstance(title, graphId, userRoles, childId);
            $('#create-process-modal').modal('toggle');
        }
        else {
            App.showWarningMessage(translations.InstanceCreateError);
        }
        e.preventDefault();
    });
});