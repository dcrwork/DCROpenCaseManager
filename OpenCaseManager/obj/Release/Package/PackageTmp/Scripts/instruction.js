
// instruction web part library
(function (window) {
    // set instructions text
    function setText(text) {
        $('.panel').html(text);
    }

    // hide instruction web part
    function hideWebPart() {
        $('#instructionWebPart').hide();
    }

    // show instruction web part
    function showWebPart() {
        $('#instructionWebPart').show();
    }

    // get instruction html from a template page
    function getInstructionHtml(page) {
        var query = { "page": page }
        API.service('services/getInstructionHtml', query)
            .done(function (response) {
                if (response == null || response == '')
                    hideWebPart();
                setText(response);
            })
            .fail(function (e) {
                App.showExceptionErrorMessage(e);
            });
    }

    // set title
    function setTitleText(text) {
        $('#titleInstruction').html(text);
    }

    // set instance is accepting
    function setInstanceIsAccepting(isOpen) {
        $('#isAccepting').show();
        if (!isOpen) {
            $('#isAccepting').addClass('instanceOpen');
        }
    }

    // instruction library
    var instruction = function () {
        this.setText = setText;
        this.hideWebPart = hideWebPart;
        this.getInstructionHtml = getInstructionHtml;
        this.showWebPart = showWebPart;
        this.setTitleText = setTitleText;
        this.setInstanceIsAccepting = setInstanceIsAccepting;
    };

    return window.Instruction = new instruction;
}(window));

// on document ready initialization
$(document).ready(function () {
    // hide/show text of instruction web part
    $('.accordion').click(function () {
        var isCollapse = false;
        if ($('.panel').is(":visible")) {
            isCollapse = true;
            $("#collapse-icon").attr('class', '');
            $('#collapse-icon').addClass('glyphicon glyphicon-chevron-right');
            $('.panel').hide(1000);
        } else {
            isCollapse = false;
            $("#collapse-icon").attr('class', '');
            $('#collapse-icon').addClass('glyphicon glyphicon-chevron-down');
            $('.panel').show(1000);
        }
        window.localStorage.setItem('instructionCollapse', isCollapse);
    });

    // get value from cookie/ or by default expand the instruction web part
    var isCollapsed = window.localStorage.getItem('instructionCollapse');
    if (isCollapsed == null || isCollapsed == "false") {
        $("#collapse-icon").attr('class', '');
        $('#collapse-icon').addClass('glyphicon glyphicon-chevron-down');
        $('.panel').show(1000);
    }
});