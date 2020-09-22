using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AcadreLib
{
    public class CPRBrokerService
    {
        private CPRBrokerWrapper wrapper;

        public CPRBrokerService(string endpointUrl, string CPRBroker_UserToken, string CPRBroker_ApplicationToken)
        {
            wrapper = new CPRBrokerWrapper(endpointUrl);
            wrapper.SetApplicationHeader(CPRBroker_UserToken, CPRBroker_ApplicationToken);
        }

        public SimplePerson GetSimplePersonByCPR(string cpr)
        {
            
            SimplePerson simplePerson = new SimplePerson();

            var laesResultatItem = wrapper.GetItem(wrapper.GetUuid(cpr).UUID);

            return GetSimplePersonByItem(laesResultatItem);
        }

        public SimplePerson GetSimplePersonByItem(CPRBroker.RegistreringType1 OutputItem)
        {

            SimplePerson simplePerson = new SimplePerson();

            simplePerson.FirstName = OutputItem.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonGivenName;
            simplePerson.MiddleName = OutputItem.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonMiddleName;
            simplePerson.Surname = OutputItem.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonSurnameName;

            Address address = wrapper.GetAddress(OutputItem);

            simplePerson.NameAddressProtection = ((CPRBroker.CprBorgerType)OutputItem.AttributListe.RegisterOplysning[0].Item).NavneAdresseBeskyttelseIndikator;
            simplePerson.Address = address;
            simplePerson.CPR = ((CPRBroker.CprBorgerType)OutputItem.AttributListe.RegisterOplysning[0].Item).PersonCivilRegistrationIdentifier;
            return simplePerson;
        }
        public Child GetChild(string cpr)
        {            
            Child child = new Child();
            List<SimplePerson> siblings = new List<SimplePerson>();
            CPRBroker.PersonFlerRelationType[] FatherChildren = null;
            CPRBroker.PersonFlerRelationType[] MotherChildren = null;
            CPRBroker.RegistreringType1 FatherOutput;
            CPRBroker.RegistreringType1 MotherOutput;

            var ChildUUID = wrapper.GetUuid(cpr).UUID;
            var ChildOutput = wrapper.GetItem(ChildUUID);

            child.SimpleChild = GetSimplePersonByItem(ChildOutput);
            
            
            // Værge
            if (ChildOutput.RelationListe.RetligHandleevneVaergeForPersonen != null)
            {
                var GuardianOutput = wrapper.GetItem(ChildOutput.RelationListe.RetligHandleevneVaergeForPersonen[0].ReferenceID.Item);
                child.Guardian = GetSimplePersonByItem(GuardianOutput);
            }
            // Far
            if (ChildOutput.RelationListe.Fader != null)
            {
                List<SimplePerson> Fathers = new List<SimplePerson>();
                foreach (var Father in ChildOutput.RelationListe.Fader)
                {
                    FatherOutput = wrapper.GetItem(Father.ReferenceID.Item);
                    Fathers.Add(GetSimplePersonByItem(FatherOutput));
                    FatherChildren = FatherOutput.RelationListe.Boern;
                }
                child.Dad = Fathers; 
            }
            // Mor
            if (ChildOutput.RelationListe.Moder != null)
            {
                List<SimplePerson> Mothers = new List<SimplePerson>();
                foreach (var Mother in ChildOutput.RelationListe.Moder)
                {
                    MotherOutput = wrapper.GetItem(Mother.ReferenceID.Item);
                    Mothers.Add(GetSimplePersonByItem(MotherOutput));
                    MotherChildren = MotherOutput.RelationListe.Boern;
                }
                child.Mom = Mothers;
            }
            // Søskende (Har samme far og mor)
            if (FatherChildren != null && MotherChildren != null)
            {
                foreach (var FatherChild in FatherChildren)
                {
                    foreach (var MotherChild in MotherChildren)
                    {
                        if (MotherChild.ReferenceID.Item == FatherChild.ReferenceID.Item && MotherChild.ReferenceID.Item != ChildUUID)
                        {
                            var SiblingOutput = wrapper.GetItem(MotherChild.ReferenceID.Item);
                            siblings.Add(new SimplePerson()
                            {
                                FirstName = SiblingOutput.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonGivenName,
                                MiddleName = SiblingOutput.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonMiddleName,
                                Surname = SiblingOutput.AttributListe.Egenskab[0].NavnStruktur.PersonNameStructure.PersonSurnameName,
                                CPR = ((CPRBroker.CprBorgerType)SiblingOutput.AttributListe.RegisterOplysning[0].Item).PersonCivilRegistrationIdentifier
                            });
                        }
                    }
                }
                child.Siblings = siblings;
            }
            // child.Custody
            if (ChildOutput.RelationListe.Foraeldremyndighedsindehaver != null)
            {
                List<string> CustodyOwnersNames = new List<string>();
                foreach (var custodyOwner in ChildOutput.RelationListe.Foraeldremyndighedsindehaver)
                {
                    if (custodyOwner == ChildOutput.RelationListe.Fader[0])
                    {
                        CustodyOwnersNames.Add(child.Dad.First().FullName);
                    }
                    else if (custodyOwner == ChildOutput.RelationListe.Moder[0])
                    {
                        CustodyOwnersNames.Add(child.Mom.First().FullName);
                    }
                    else
                    {
                        CustodyOwnersNames.Add(GetSimplePersonByItem(wrapper.GetItem(custodyOwner.ReferenceID.Item)).FullName);
                    }
                }
                child.CustodyOwnersNames = CustodyOwnersNames;
            }

            //List<CPRBroker.PersonRelationType>


            return child;
        }
        // returns true if string cpr is a 10 number long int with the first 4 digits being a valid day and month
        public static bool IsValidCPR(string cpr)
        {
            if (cpr.Length != 10) return false;
            uint theNum = 0;
            if (!uint.TryParse(cpr, out theNum)) return false;
            if (int.Parse(cpr.Substring(0, 2)) > 31 || int.Parse(cpr.Substring(0, 2)) == 0) return false;
            if (int.Parse(cpr.Substring(2, 2)) > 12 || int.Parse(cpr.Substring(2, 2)) == 0) return false;
            return true;
        }
    }
}
