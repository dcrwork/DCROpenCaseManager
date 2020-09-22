
(function (window) {
    var data;
    var uploadFilesTemp = new Array();
    var tempdropArea;
    var resolve;

    function NewAppointment(email, subject, body) {
        var ua = window.navigator.userAgent;
        var msie = ua.indexOf("MSIE ");

        if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./))  // If Internet Explorer, return version number
        {
            var promise = new Promise(function (resolve, reject) {
                App.getDocumentUrls(resolve);
            });
            promise.then(function (documents) {
                if (IsActiveXSupported('Outlook.Application')) {
                    var app = new ActiveXObject('Outlook.Application');
                    try {
                        var objNS = app.GetNameSpace('MAPI');
                        var calItem = app.CreateItem(1);
                        calItem.Subject = (subject);
                        calItem.Body = (body);
                        calItem.BodyFormat = 1; // Plain   calItem.BodyFormat = 1; // Plain
                        calItem.Body = (body.replace(/\\n/g, '\n'));
                        var recipient = calItem.Recipients.Add(email);//attendees list
                        recipient.Resolve;
                        for (var i = 0; i < documents.length; i++) {
                            calItem.Attachments.add(documents[i]);
                        }
                        calItem.Display();
                        Custom.resolvePromise();
                    }
                    catch (ex) {
                        App.showExceptionErrorMessage(ex);
                    }
                }
                else {
                    App.showWarningMessage('You must enable ActiveX in Internet Explorer to create a new appointment. Alternatively you can create the appointment manually – Click <a target= "_blank" href= "../HtmlTemplates/InstructionsForActiveXOutlook.html" > Here</a > for more details.');
                    if (Window.MUS != null)
                        MUS.musDetails(MUS.showMUS);
                }
                setTimeout(function () {
                    App.cleanupDocs(documents);
                }, 5000);
            }, function (err) {
                App.showExceptionErrorMessage(err);
            });
        }
        else {
            App.showWarningMessage('Creating new appointments assumes you’re using Internet Explorer. Please launch IE and open the application. Alternatively you can create the appointment manually – Click <a target= "_blank" href= "../HtmlTemplates/InstructionsForActiveXOutlook.html" > Here</a > for more details.');
            if (Window.MUS != null)
                MUS.musDetails(MUS.showMUS);
        }
    }

    function IsActiveXSupported(app) {
        var isSupported = false;

        if (!("ActiveXObject" in window)) {
            return isSupported;
        }

        try {
            var app = new ActiveXObject(app);
            isSupported = true;
        } catch (e) {
            isSupported = false;
        }
        return isSupported;
    }

    function UploadDocument() {
        if ($('#addDocumentDialog').attr('initiated') == null) {
            $('#addDocumentDialog').modal('toggle');
            tempdropArea = document.getElementById("drop-area-front-page");

            tempdropArea.addEventListener('dragenter', tempPreventDefaults, false)
            document.body.addEventListener('dragenter', tempPreventDefaults, false)
            tempdropArea.addEventListener('dragover', tempPreventDefaults, false)
            document.body.addEventListener('dragover', tempPreventDefaults, false)
            tempdropArea.addEventListener('dragleave', tempPreventDefaults, false)
            document.body.addEventListener('dragleave', tempPreventDefaults, false)
            tempdropArea.addEventListener('drop', tempPreventDefaults, false)
            document.body.addEventListener('drop', tempPreventDefaults, false)
            // Handle dropped files
            tempdropArea.addEventListener('drop', tempHandleDrop, false)

            // To create a template funtion
            $('body').on('change', '#fileElemTemp', function () {
                tempHandleFiles(this.files);
            })

            // To create a template funtion
            $('body').on('click', '#addDocumentTemp', function () {
                tempSubmitFiles();
            })
            $('#addDocumentDialog').attr('initiated', true);
        }
        else {
            $('#addDocumentDialog').modal('toggle');
        }
    }

    function NewDocument(template, strDefSave) {
        if (typeof (strDefSave) == 'undefined') strDefSave = '';
        var sMessage = '';
        try {
            //template = 'G:\\__FAMILIE_INSTITUTIONER\\Skabeloner OCM\\Template.dotx';
            template = template.replace(/%20/g, ' ');
            template = template.replace(/\\\\/g, '/');
            template = template.replace(/\\/g, '/');
            var urlToLaunch = 'ms-word:nft|u|file:///' + template;
            //alert(urlToLaunch);
            console.log('NewDocument(...) - ' + urlToLaunch);
            var i;
            sMessage = 'navigator.msLaunchUri';
            if (navigator.msLaunchUri) {
                navigator.msLaunchUri(urlToLaunch);
            } else {
                var win = (window.parent) ? window.parent : window;
                sMessage = 'win.location.assign';
                win.location.assign(urlToLaunch);
            }
            Custom.resolvePromise();
        }
        catch (e) {
            alert('NewDocument("' + template + '") exception - ' + sMessage + ' - ' + e.message);
        }
    }

    function resolvePromise() {
        return Custom.resolve();
    }

    function tempPreventDefaults(e) {
        e.preventDefault()
        e.stopPropagation()
    }

    function tempHandleDrop(e) {
        var dt = e.dataTransfer
        var files = dt.files

        tempHandleFiles(files)
    }

    function tempHandleFiles(files) {
        uploadFilesTemp = new Array();
        for (var i = 0; i < 1; i++) {
            uploadFilesTemp.push(files[i]);
            $('#selectedFileNameTemp').text(files[i].name);
            tempdropArea.classList.add('highlight')
        }
    }

    function tempSubmitFiles() {
        tempUploadFile(uploadFilesTemp[0]);
    }

    function tempUploadFile(file) {
        if (file != null) {
            $.ajax({
                url: window.location.origin + "/api/records/AddDocument",
                type: 'POST',
                headers: {
                    'filename': file.name,
                    'type': 'Temp',
                    'givenFileName': file.name,
                    'eventId': Custom.eventData["eventId"],
                    'instanceId': Custom.eventData["instanceId"]
                },
                data: file,
                async: false,
                cache: false,
                contentType: false,
                enctype: 'multipart/form-data',
                processData: false,
                success: function (response) {
                    $('#addDocumentDialog').modal('toggle');
                    tempdropArea.classList.remove('highlight')
                    uploadFilesTemp = new Array();
                    $('#selectedFileNameTemp').text('');
                    var $el = $('#fileElemTemp');
                    $el.wrap('<form>').closest('form').get(0).reset();
                    $el.unwrap();
                    Custom.resolvePromise();
                }
            });
        }
    }

    function OpenURL(link) {
        window.open(link);
        Custom.resolvePromise();
    }

    var custom = function () {
        this.NewAppointment = NewAppointment;
        this.UploadDocument = UploadDocument;
        this.NewDocument = NewDocument;
        this.eventData = data;
        this.OpenURL = OpenURL;
        this.resolvePromise = resolvePromise;
        this.resolve = resolve;
    };

    return window.Custom = new custom;

}(window));