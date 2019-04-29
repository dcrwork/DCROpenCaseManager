/// <reference path='../Scripts/journalNote.js' />
/// <reference path='../Scripts/jasmine/jasmine.js' />

import { describe } from "mocha";

describe("getStatus", function () {
    it('returns class dotRed when given missed deadline', function () {
        //Arrange
        var deadline = "1995-12-12T12:00:00";
        //Act
        var result = getStatus(deadline);
        //Assert
        expect(result).toBe("<span class='dot dotRed'></span>");
    });

    it('returns class dotGreen when given deadline more than a week in the future', function () {
        //Arrange
        var deadline = "2022-12-12T12:00:00";
        //Act
        var result = getStatus(deadline);
        //Assert
        expect(result).toBe("<span class='dot dotGreen'></span>");
    });

    it('returns class dotYellow when given deadline 3 days in the future', function () {
        //Arrange
        var toDay = new Date();
        var deadline = new Date(toDay.setTime(toDay.getTime() + 3 * 86400000));
        //Act
        var result = getStatus(deadline);
        //Assert
        expect(result).toBe("<span class='dot dotYellow'></span>");
    });

    it('returns class dotGreen when given null', function () {
        //Arrange
        var deadline = null;
        //Act
        var result = getStatus(deadline);
        //Assert
        expect(result).toBe("<span class='dot dotGreen'></span>");
    });

    it('returns class dotGreen when given nothing', function () {
        //Arrange
        var deadline;
        //Act
        var result = getStatus();
        //Assert
        expect(result).toBe("<span class='dot dotGreen'></span>");
    });
});

describe("getChildInstanceHtml", function () {
    it('returns class instanceClosed when given item with IsOpen = 0', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 0,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: null
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("instanceClosed");
    });

    it('writes "Lukket" when given item with IsOpen = 0', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 0,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: null
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("Lukket");
    });

    it('puts the name to uppercase', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 0,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: null
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("Thomas");
    });

    it('writes "intet gjort" when LastUpdated is null', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 0,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: null
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("intet gjort");
    });

    it('writes the LastUpdated when it is not null', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 0,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: "2012-12-12 16:00:05"
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("2012-12-12");
    });

    it('writes the status if IsOpen = 1', function () {
        //Arrange
        var item = {
            Id: 1,
            IsOpen: 1,
            NextDeadline: null,
            Title: "Hej hej",
            Process: "Hejsa",
            Name: "thomas",
            LastUpdated: "2012-12-12 16:00:05"
        }
        //Act
        var result = getChildInstanceHtml(item);
        //Assert
        expect(result).toContain("dot");
    });
});
