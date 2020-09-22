namespace AcadreLib
{
    public class SimplePerson
    {
        private bool aNameAddressProtection;
        private string aFirstName;
        private string aMiddleName;
        private string aSurname;
        private string cpr;
        private Address aAddress;
        

        public SimplePerson()
        {
            Address = new Address();
        }

        public Address Address
        {
            get { return aAddress; }
            set { aAddress = value; }
        }
        public string CPR
        {
            get { return cpr ?? ""; }
            set { cpr = value; }
        }

        public bool NameAddressProtection
        {
            get { return aNameAddressProtection; }
            set { aNameAddressProtection = value; }
        }

        public string FirstName
        {
            get { return aFirstName ?? ""; }
            set { aFirstName = value; }
        }

        public string MiddleName
        {
            get { return aMiddleName ?? ""; }
            set { aMiddleName = value; }
        }

        public string Surname
        {
            get { return aSurname ?? ""; }
            set { aSurname = value; }
        }

        public string AddressLine1
        {
            get { return aAddress.AddressLine1 ?? ""; }
            set { aAddress.AddressLine1 = value; }
        }

        public string AddressLine2
        {
            get { return aAddress.AddressLine2 ?? ""; }
            set { aAddress.AddressLine2 = value; }
        }

        public string AddressLine3
        {
            get { return aAddress.AddressLine3 ?? ""; }
            set { aAddress.AddressLine3 = value; }
        }

        public string AddressLine4
        {
            get { return aAddress.AddressLine4 ?? ""; }
            set { aAddress.AddressLine4 = value; }
        }

        public string AddressLine5
        {
            get { return aAddress.AddressLine5 ?? ""; }
            set { aAddress.AddressLine5 = value; }
        }

        public string FullName
        {
            get
            { return (FirstName + " " + MiddleName + " " + Surname).Replace("  ", " "); }
        }
               
        public string Age
        {
            get { return CPR; }
            set { Age = value; }
        }
        public string EnvelopeAddress
        {
            get
            {
                string result = "";
                result += this.AddressLine1 == "" ? "" : aAddress.AddressLine1;
                result += this.AddressLine2 == "" ? "" : "\n" + aAddress.AddressLine2;
                result += this.AddressLine3 == "" ? "" : "\n" + aAddress.AddressLine3;
                result += this.AddressLine4 == "" ? "" : "\n" + aAddress.AddressLine4;
                result += this.AddressLine5 == "" ? "" : "\n" + aAddress.AddressLine5;
                return result;
            }
        }
    }
}