/// <reference path='../Scripts/BowlingGame.js' />
/// <reference path='../Scripts/jasmine/jasmine.js' />

describe('The bowling game', function () {
    it('correctly calulates the gutter game', function () {
        // Arrange
        for (i = 0; i < 20; i++) {
            roll(1);
        }

        var expected = 0;

        //Act
        var result = score();
        //Assert

        expect(result).toBe(expected);
    });
});