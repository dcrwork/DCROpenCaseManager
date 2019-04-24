/// <reference path='../Scripts/journalNote.js' />
/// <reference path='../Scripts/jasmine/jasmine.js' />

describe("Add minuts work", function () {
    it('given a time i can add minuts and get new time back', function () {
        //Arragen
        var time = "10:30"
        //Act
        var result = addMinutsToTime(time, 30)
        //Assert
        expect(result).toBe("11:00");
    });

    it('adding 60 minuts to 23:30 returns 0:30', function () {
        //Arragen
        var time = "23:30"
        //Act
        var result = addMinutsToTime(time, 60)
        //Assert
        expect(result).toBe("0:30");
    })

    it('given two hours in minuts (120 m) adds 2 hours to the new time', function () {
        //Arragen
        var time = "22:00"
        //Act
        var result = addMinutsToTime(time, 120)
        //Assert
        expect(result).toBe("0:00");
    })

    it('given negative minuts set the time back', function () {
        //Arragen
        var time = "10:30"
        //Act
        var result = addMinutsToTime(time, -30)
        //Assert
        expect(result).toBe("10:00");
    })

    it('more than two hours in negative minuts', function () {
        //Arragen
        var time = "10:00"
        //Act
        var result = addMinutsToTime(time, -130)
        //Assert
        expect(result).toBe("7:50");
    })
})