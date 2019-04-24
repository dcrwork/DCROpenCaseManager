using System;
using System.Collections.Generic;
using System.Text;
using OpenCaseManager.Commons;
using Xunit;

namespace OpenCaseManagerTests
{
    public class ExtentionsTest
    {
        [Theory]
        [InlineData("22/4/2019", "4/22/2019")]
        [InlineData("22/4/2019 20:30", "4/22/2019 20:30")]
        [InlineData("10/12/2019 10:15", "12/10/2019 10:15")]
        public void Test_parsing_daish_date_makes_the_right_date(string _danishDate, string _usDate)
        {
            var danishDate = _danishDate;

            var dateTime = danishDate.parseDanishDateToDate();

            var expectedDate = Convert.ToDateTime(_usDate);

            Assert.Equal(expectedDate, dateTime);
        }
    }
}
