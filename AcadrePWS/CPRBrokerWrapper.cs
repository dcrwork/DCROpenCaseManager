using System;
using System.Text.RegularExpressions;

namespace AcadreLib
{
	class CPRBrokerWrapper
	{
        private CPRBroker.Part partClient = new CPRBroker.Part();
        public CPRBrokerWrapper(string url)
        {
            partClient.Url = url;
        }

        public void SetApplicationHeader( string userToken, string applicationToken)
        {
            partClient.ApplicationHeaderValue = new CPRBroker.ApplicationHeader
            {
                ApplicationToken = applicationToken,
                UserToken = userToken
            };
        }

        public CPRBroker.GetUuidOutputType GetUuid(string cpr)
        {
            CPRBroker.GetUuidOutputType uuid = partClient.GetUuid(cpr);
            if (uuid.StandardRetur.StatusKode != "200")
            {
                throw new CPRBrokerWrapperException(String.Format("CPRBrokerWrapper: Call to GetUuid failed. StatusKode:{0}. FejlbeskedTekst: {1}", uuid.StandardRetur.StatusKode, uuid.StandardRetur.FejlbeskedTekst));
            }
            return uuid;
        }

        public CPRBroker.RegistreringType1 GetItem(string uuid)
        {
            // Read: Finds and returns a single person object. It will return the latest registration within the specified range. 
            // It first looks in the local database, and attempts external data providers if no data is found locally.
            CPRBroker.LaesOutputType output = partClient.Read(new CPRBroker.LaesInputType { UUID = uuid });
            //CPRBroker.LaesOutputType output = partClient.Read(new CPRBroker.ReadRequest(applicationHeader, new CPRBroker.SourceUsageOrderHeader(), )).LaesOutput;
            if (output.StandardRetur.StatusKode != "200")
            {
                throw new CPRBrokerWrapperException(String.Format("CPRBrokerWrapper: Call to GetUuid failed. StatusKode:{0}. FejlbeskedTekst: {1}", output.StandardRetur.StatusKode, output.StandardRetur.FejlbeskedTekst));
            }
            else
            {
                if (!(output.LaesResultat.Item is CPRBroker.RegistreringType1))
                {
                    throw new CPRBrokerWrapperException(String.Format("CPRBrokerWrapper: Unexpected RegistreringType derived type: {0}", output.LaesResultat.Item.GetType().Name));
                }
                return (CPRBroker.RegistreringType1)output.LaesResultat.Item;
            }
        }

        public Address GetAddress(CPRBroker.RegistreringType1 Item)
        {
            Address address = new Address();
            var registerOplysningItem = (CPRBroker.CprBorgerType)Item.AttributListe.RegisterOplysning[0].Item;

            if (registerOplysningItem.FolkeregisterAdresse != null)
            {
                CPRBroker.AdresseBaseType adresse = registerOplysningItem.FolkeregisterAdresse.Item;
                if (adresse is CPRBroker.DanskAdresseType)
                {
                    CPRBroker.AddressPostalType danishAddress = ((CPRBroker.DanskAdresseType)adresse).AddressComplete.AddressPostal;
                    string streetNameAndNumber = danishAddress.StreetName + " " + ParseAddressField(danishAddress.StreetBuildingIdentifier);
                    string floorSuite = ParseAddressField(danishAddress.FloorIdentifier) == "" ? ParseAddressField(danishAddress.SuiteIdentifier) : (ParseAddressField(danishAddress.FloorIdentifier) + (ParseAddressField(danishAddress.SuiteIdentifier) == "" ? "" : "," + ParseAddressField(danishAddress.SuiteIdentifier)));
                    floorSuite = Regex.Replace(floorSuite, @"(^|\s)(st|tv|th)\b", "$1$2.", RegexOptions.IgnoreCase);
                    address.AddressLine1 = (streetNameAndNumber + (floorSuite == "" ? "" : ", " + floorSuite)).Trim();
                    address.AddressLine2 = (ParseAddressField(danishAddress.PostCodeIdentifier) + " " + ParseAddressField(danishAddress.DistrictName)).Trim();
                }
                else if (adresse is CPRBroker.GroenlandAdresseType)
                {
                    CPRBroker.AddressCompleteGreenlandType greenlandAddress = ((CPRBroker.GroenlandAdresseType)adresse).AddressCompleteGreenland;
                    string streetNameAndNumber = greenlandAddress.StreetName + " " + ParseAddressField(greenlandAddress.StreetBuildingIdentifier);
                    string floorSuite = ParseAddressField(greenlandAddress.FloorIdentifier) + ParseAddressField(greenlandAddress.SuiteIdentifier) == "" ? "" : "," + ParseAddressField(greenlandAddress.SuiteIdentifier);
                    address.AddressLine1 = (streetNameAndNumber + (floorSuite == "" ? "" : ", " + floorSuite)).Trim();
                    address.AddressLine2 = (ParseAddressField(greenlandAddress.PostCodeIdentifier) + " " + ParseAddressField(greenlandAddress.DistrictName)).Trim();
                    address.AddressLine3 = "Grønland";
                }
                else if (adresse is CPRBroker.VerdenAdresseType)
                {
                    CPRBroker.ForeignAddressStructureType foreignAddress = ((CPRBroker.VerdenAdresseType)adresse).ForeignAddressStructure;
                    address.AddressLine1 = foreignAddress.PostalAddressFirstLineText;
                    address.AddressLine2 = foreignAddress.PostalAddressSecondLineText;
                    address.AddressLine3 = foreignAddress.PostalAddressThirdLineText;
                    address.AddressLine4 = foreignAddress.PostalAddressFourthLineText;
                    address.AddressLine5 = foreignAddress.PostalAddressFifthLineText;
                }
                else
                {
                    throw new CPRBrokerWrapperException(String.Format("CPRBrokerWrapper: Unexpected AdresseBaseType derived type: {0}", adresse.GetType().Name));
                }     
            }
            return address;
        }

		private static string ParseAddressField(string field)
		{
			// return empty string if null. Remove leading zeros
			return field == null ? "" : field.TrimStart('0');
		}

		public class CPRBrokerWrapperException : Exception
		{
			public CPRBrokerWrapperException(string message) : base(message) { }
		}


	}
}

