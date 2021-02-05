using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class Enums
    {
        public enum SQLOperation
        {
            SELECT,
            INSERT,
            UPDATE,
            SP
        }

        public enum ParameterType
        {
            _int,
            _string,
            _decimal,
            _boolean,
            _datetime,
            _xml,
            _guid
        }

        public enum CompareOperator
        {
            equal,
            not_equal,
            greater_than,
            less_than,
            equal_greater_than,
            equal_less_than,
            like,
            _null,
            not_null
        }

        public enum LogicalOperator
        {
            none = 0,
            and,
            or
        }
    }
}