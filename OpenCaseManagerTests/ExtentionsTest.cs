using System;
using System.Collections.Generic;
using System.Text;
using OpenCaseManager.Commons;
using Xunit;

namespace OpenCaseManagerTests
{
    public class ExtentionsTest
    {
        [Fact]
        public void Test_parsing_daish_date_makes_the_right_date_without_time()
        {
            var danishDate = "22/4/2019";

            var dateTime = danishDate.parseDanishDateToDate();

            var expectedDate = new DateTime(2019,4,22,0,0,0);

            Assert.Equal(expectedDate, dateTime);
        }

        [Fact]
        public void Test_parsing_daish_date_makes_the_right_date_date_higher_than_12()
        {
            var danishDate = "22/4/2019 20:30";

            var dateTime = danishDate.parseDanishDateToDate();

            var expectedDate = new DateTime(2019, 4, 22, 20, 30,0);

            Assert.Equal(expectedDate, dateTime);
        }

        [Fact]
        public void Test_parsing_daish_date_makes_the_right_date_month_is_12()
        {
            var danishDate = "10/12/2019 10:15";

            var dateTime = danishDate.parseDanishDateToDate();

            var expectedDate = new DateTime(2019, 12, 10, 10, 15, 0);

            Assert.Equal(expectedDate, dateTime);
        }
    }
}
