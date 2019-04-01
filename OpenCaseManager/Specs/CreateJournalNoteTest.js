/// <reference path='../Scripts/journalNote.js' />
/// <reference path='../Scripts/jasmine/jasmine.js' />

describe("Date will be formated right", function () {
    it('given an dateTime it retuns the date in the right format', function () {
        //Arragen
        var date = new Date(2011,11,1).toString();
        //Act
        var result = formatDate(date);
        //Assert
        expect(result).toBe("1/11/2011");
    })
})