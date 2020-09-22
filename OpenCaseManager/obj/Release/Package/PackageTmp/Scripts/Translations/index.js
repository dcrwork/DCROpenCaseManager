var Converter = require("csvtojson").Converter;
var fs = require("fs");
var rs = fs.createReadStream("./DCR translations - data.csv");
var finalObj = {};
var csvConverter = new Converter({});

csvConverter.on("end_parsed", function() {
    writeToDisk();
});


function writeToDisk() {
    var currentLang;
    for (var key in finalObj) {
        currentLang = key;
    }
    require('fs').writeFile(

        './' + currentLang + '.js',

        'var translations = ' + JSON.stringify(finalObj[currentLang]),

        'utf8',

				writeCallback
    );
    delete finalObj[currentLang];
    console.log(currentLang);
}

function writeCallback() {
    if (Object.keys(finalObj).length > 0) {
        writeToDisk();
    }
}

csvConverter.on("record_parsed", function(resultRow, rawRow, rowIndex) {
    for (var key in resultRow) {
        var entry = {};
        if (key !== 'COMMENTS' && key !== 'token') {
            entry[resultRow.token] = resultRow[key];

            if (!finalObj.hasOwnProperty(key)) {
                finalObj[key] = {};
            }
            if (resultRow[key] === '' || resultRow[key] === undefined || resultRow[key] === null) {
              finalObj[key][resultRow.token] = '!!' + resultRow['en-US'];
            } else {
              finalObj[key][resultRow.token] = resultRow[key];
            }
        }
    }
});
rs.pipe(csvConverter);
