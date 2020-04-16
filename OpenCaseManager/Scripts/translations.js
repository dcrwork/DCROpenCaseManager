var localeDefs = {
    'da': 'da-DK',
    'da-DK': 'da-DK',
    'en': 'en-US',
    'en-US': 'en-US',
    'pt': 'pt-BR',
    'pt-BR': 'pt-BR',
    'es': 'es',
    'es-AR': 'es',
    'ca': 'ca',
    'it': 'it-IT',
    'it-IT': 'it-IT',
    'nb': 'nb-NO',
    'nb-NO': 'nb-NO',
    'sv': 'se-SV',
    'se-SV': 'se-SV'
};

var browserLocale = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
var defaultLocale = 'en-US';

var locale = (localeDefs[browserLocale] === undefined) ? defaultLocale : localeDefs[browserLocale];

function getTranslations(locale) {
    var getUrl = window.location;
    var baseUrl = getUrl.protocol + "//" + getUrl.host + "/";
    var fileUrl = baseUrl + 'scripts/translations/' + locale + '.js';

    fetch(fileUrl, { method: 'GET', mode: 'same-origin' }).then(r => {
        if (r.status == 200) {
            API.getJSFile(fileUrl)
                .done(function (response) {
                })
                .fail(function (e) {
                    alert('Error in getting locale');
                });
        }
    }).catch(e => {
        console.log(e);
    });
}

getTranslations(locale);