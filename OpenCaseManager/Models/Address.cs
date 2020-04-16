using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class Address
    {
        private string aAddressLine1;
        private string aAddressLine2;
        private string aAddressLine3;
        private string aAddressLine4;
        private string aAddressLine5;

        public string AddressLine1
        {
            get { return aAddressLine1 ?? ""; }
            set { aAddressLine1 = value; }
        }

        public string AddressLine2
        {
            get { return aAddressLine2 ?? ""; }
            set { aAddressLine2 = value; }
        }

        public string AddressLine3
        {
            get { return aAddressLine3 ?? ""; }
            set { aAddressLine3 = value; }
        }

        public string AddressLine4
        {
            get { return aAddressLine4 ?? ""; }
            set { aAddressLine4 = value; }
        }

        public string AddressLine5
        {
            get { return aAddressLine5 ?? ""; }
            set { aAddressLine5 = value; }
        }
    }
}