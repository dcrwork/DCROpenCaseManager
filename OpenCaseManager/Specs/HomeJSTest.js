/// <reference path='../Scripts/Home.js' />
/// <reference path='../Scripts/jasmine/jasmine.js' />

describe("getStatus", function () {

    it('returns class dotGreen when NextDeadline is null and it does have events', function () {
        //Arrange
        var deadline = null;
        var sumOfEvents = 1;
        
        var item = {
            NextDeadline: deadline,
            SumOfEvents: sumOfEvents 
        }
        //Act
        var result = getStatus(item);
        //Assert
        expect(result).toBe("<span class='dot dotGreen'></span>");
    });

    it("returns class dotRed given deadline is passed", function () {
        //Arrange
        var item = {
            NextDeadline: "1995-12-12T12:00:00",
            SumOfEvents: "0"
        }
        //Act
        var result = getStatus(item);
        //Assert
        expect(result).toBe("<span class='dot dotRed'></span>");
    });

    it('returns class dotYellow when given deadline 3 days in the future', function () {
        //Arrange
        var toDay = new Date();
        var deadline = new Date(toDay.setTime(toDay.getTime() + 3 * 86400000));
        var item = {
            NextDeadline: deadline,
            SumOfEvents: "0"
        }

        //Act
        var result = getStatus(item);
        //Assert
        expect(result).toBe("<span class='dot dotYellow'></span>");
    });

});

describe("addZero", function () {
    it('append 0 to I, if I only has one digit', function () {
        for (i = 0; i < 10; i++) {
            // Arrange
            var expected = '0' + i.toString() ;
            //Act
            var result = addZero(i);
            //Assert    
            expect(result).toEqual(expected);
        }
    });

    it('does not append 0 to I, if I only has one digit', function () {
            // Arrange
            var i = '10'
            var expected = i.toString() ;
            //Act
            var result = addZero(i);
            //Assert    
            expect(result).toEqual(expected);
    });
});

describe("getChildInstanceHtml", function () {
    it("returns the correct uri with a child id", function () {
        // Arrange
        var item = {
            ChildId: 1,
        };
        var expected = "/Child?id=1";
        // Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain(expected);
    });

    it("returns the dotGrey since there isn't a deadline", function () {
        // Arrange
        var item = {
            NextDeadline: null,
        };
        var expected = "dotGrey";
        // Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain(expected);
    });

    it("returns 'Ingen kommende deadlines' if the NextDeadine is null", function () {
        //Arrange
        var item = {
            NextDeadline: undefined 
        };
        var expected = "Ingen kommende deadlines";

        //Act
        var result = getChildInstanceHtml(item);

        //Assert
        expect(result).toContain(expected);
    })
});
