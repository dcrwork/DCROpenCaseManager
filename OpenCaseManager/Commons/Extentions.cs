using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Commons
{
    public static class Extentions
    {
        public static DateTime parseDanishDateToDate(this string _date)
        {
            var now = DateTime.Now;
            var dateTime = _date.Split(' ');

            var date = dateTime.Length >= 1 ? dateTime[0].Split('/') : null ;
            var time = dateTime.Length >= 2 ? dateTime[1].Split(':') : null ;

            if( date != null)
            {
                var year = date.Length >= 3 ? Convert.ToInt16(date[2]) : now.Year ;
                var month = date.Length >= 2 ? Convert.ToInt16(date[1]) : now.Month;
                var day = date.Length >= 1 ? Convert.ToInt16(date[0]) : now.Day;

                if (time == null) time = new string[0];

                var hours = time.Length >= 1 ? Convert.ToInt16(time[0]) : 0;
                var minutes = time.Length >= 2 ? Convert.ToInt16(time[1]) : 0;
         
                return new DateTime(year, month, day, hours, minutes, 0);
            }

            return now;
        }
    }
}