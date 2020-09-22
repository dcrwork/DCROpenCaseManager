using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AcadrePWS;

namespace AcadreLib
{
    public class AcadreService
    {
        private AcadreServiceV7.CaseService7 caseService;
        private AcadreServiceV7.ContactService7 contactService;
        private AcadreServiceV7.ConfigurationService7 configurationService;
        private AcadreServiceV7.MainDocumentService7 documentService;
        private AcadreServiceV7.MemoService7 memoService;
        private AcadreServiceV4.ContactService4 contactService4;
        private AcadreServiceV4.DocumentService4 documentService4;
        private AcadreServiceV4.MemoService4 memoService4;
        private AcadreServiceV4.CaseService4 caseService4;
        private CPRBrokerService CPRBrokerService;
        string CaseFileTypeCode = "BUSAG";
        public AcadreService(string currentUser)
        {
            Acadre.PWSHeaderExtension.User = currentUser;
            System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential(Config.AcadreServiceUserName, Config.AcadreServiceUserPassword, Config.AcadreServiceUserDomain);
            caseService = new AcadreServiceV7.CaseService7
            {
                Credentials = networkCredential,
                Url = Config.AcadreService
            };
            contactService = new AcadreServiceV7.ContactService7
            {
                Credentials = networkCredential,
                Url = Config.AcadreService
            };
            configurationService = new AcadreServiceV7.ConfigurationService7
            {
                Credentials = networkCredential,
                Url = Config.AcadreService
            };
            documentService = new AcadreServiceV7.MainDocumentService7
            {
                Credentials = networkCredential,
                Url = Config.AcadreService
            };
            memoService = new AcadreServiceV7.MemoService7
            {
                Credentials = networkCredential,
                Url = Config.AcadreService
            };
            documentService4 = new AcadreServiceV4.DocumentService4
            {
                Credentials = networkCredential,
                Url = Config.AcadreService.Replace("7.asmx", "4.asmx")
            };
            memoService4 = new AcadreServiceV4.MemoService4
            {
                Credentials = networkCredential,
                Url = Config.AcadreService.Replace("7.asmx", "4.asmx")
            };
            CPRBrokerService = new CPRBrokerService(Config.CPRBrokerEndpointURL, Config.CPRBrokerUserToken, Config.CPRBrokerApplicationToken);
        }

        public IEnumerable<ChildCase> SearchChildren(SearchCriterion searchCriterion)
        {
            List<ChildCase> childCases = new List<ChildCase>();
            var userList = configurationService.GetUserList(new AcadreServiceV7.EmptyRequestType()).ToList(); // Herfra kan CaseManager aflæses
            AcadreServiceV7.AdvancedSearchCaseCriterionType2 searchCaseCriterion = new AcadreServiceV7.AdvancedSearchCaseCriterionType2();
            AcadreServiceV7.AdvancedCaseSearchRequestType3 advancedCaseSearchRequestType = new AcadreServiceV7.AdvancedCaseSearchRequestType3();
            string[] StatusCodes;
            if (!searchCriterion.IsClosed.HasValue)
            {
                StatusCodes = new string[] { "A", "B", "P", "S" };
            }
            else
            {
                if (searchCriterion.IsClosed.Value)
                    StatusCodes = new string[] { "A" };
                else
                    StatusCodes = new string[] { "B" };
            }
            // Sagstype er det samme for alle børnesager
            advancedCaseSearchRequestType.TypeCode = CaseFileTypeCode;
            //searchCaseCriterion.CaseFileTypeCode = CaseFileTypeCode;
            // Afdeling er obligatorisk søgekriterie
            if (searchCriterion.AcadreOrgID != 0)
            {
                advancedCaseSearchRequestType.AdministrativeUnitId = searchCriterion.AcadreOrgID.ToString();
                //searchCaseCriterion.AdministrativeUnit = new AcadreServiceV7.AdministrativeUnitType()
                //{
                //    AdministrativeUnitReference = searchCriterion.AcadreOrgID.ToString()
                //};
            }
            // Sagsansvarlig er valgfrit søgekriterie           
            if (searchCriterion.CaseManagerInitials != null)
            {
                var user = userList.SingleOrDefault(ut => ut.Initials == searchCriterion.CaseManagerInitials);
                if (user != null)
                {
                    advancedCaseSearchRequestType.ResponsibleUserId = user.Id;
                    // searchCaseCriterion.CaseFileManagerReference = user.Id;
                }
                else
                    return childCases;
            }
            // KLE er valgfrit søgekriterie  
            //if (searchCriterion.KLE != null)
            //{
            //    searchCaseCriterion.ClassificationCriterion = new AcadreServiceV7.ClassificationCriterionType[]
            //    {
            //            new AcadreServiceV7.ClassificationCriterionType()
            //            {
            //                ClassificationLiteral = searchCriterion.KLE,
            //                PrincipleLiteral = "KL Koder"
            //            }
            //    };
            //}

            // Der kan desværre ikke søges efter kontakter med navn fordi APWS ikke kan returnere mere end 100 personer.
            AcadreServiceV7.ContactSearchResponseType[] foundContacts = new AcadreServiceV7.ContactSearchResponseType[] { null };
            //if (searchCriterion.PrimaryContactsName != "" && searchCriterion.ChildCPR == "*")
            //{
            //    var searchContactCriterion = new AcadreServiceV7.SearchContactCriterionType2();
            //    searchContactCriterion.ContactTypeName = "Person";
            //    searchContactCriterion.SearchTerm = searchCriterion.PrimaryContactsName;
            //    foundContacts = contactService.SearchContacts(searchContactCriterion);
            //    if (foundContacts.Length == 0)
            //    {
            //        return childCases;
            //    }
            //}

            // CPR er valgfrit søgekriterie
            advancedCaseSearchRequestType.Title = searchCriterion.ChildCPR;
            //searchCaseCriterion.CaseFileTitleText = searchCriterion.ChildCPR;
            advancedCaseSearchRequestType.CustomFields = new AcadreServiceV7.CustomField[] { new AcadreServiceV7.CustomField { Value = searchCriterion.CaseContent, Name = "df1" } };
            //searchCaseCriterion.CustomFields = new AcadreServiceV7.CustomField[] { new AcadreServiceV7.CustomField { Value = searchCriterion.CaseContent, Name = "df1" }};
            foreach (var Contact in foundContacts)
            {
                if (Contact != null)
                    advancedCaseSearchRequestType.Title = ((AcadreServiceV7.PersonType3)contactService.GetContact(Contact.GUID)).PersonCivilRegistrationIdentifier;

                foreach (var StatusCode in StatusCodes)
                {
                    advancedCaseSearchRequestType.StatusCode = StatusCode;
                    var foundCases = caseService.AdvancedCaseSearch(advancedCaseSearchRequestType);

                    //var foundCases = caseService.SearchCases(searchCaseCriterion);
                    foreach (AcadreServiceV7.BUCaseFileType foundCase in foundCases)
                    {
                        if (!(searchCriterion.KLE == ""))
                        {
                            foreach (var category in foundCase.Classification.Category)
                            {
                                if (category.Principle == "KL Koder" && category.Literal != searchCriterion.KLE)
                                    continue;
                            }
                        }
                        //string caseIDtemp = foundCase.CaseFileReference;

                        //foundCase = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(caseIDtemp);
                        // Sagsindhold er valgfrit søgekriterie  
                        // Barnets navn og KLE er valgfrit søgekriterie
                        if (searchCriterion.PrimaryContactsName != "" && !foundCase.TitleAlternativeText.Contains(searchCriterion.PrimaryContactsName))
                        {
                            continue;
                        }
                        var user = userList.SingleOrDefault(ut => ut.Id == foundCase.CaseFileManagerReference);
                        if (user == null)
                            return childCases;
                        childCases.Add(new ChildCase()
                        {
                            CaseID = int.Parse(foundCase.CaseFileIdentifier),
                            ChildName = foundCase.TitleAlternativeText,
                            ChildCPR = foundCase.CaseFileTitleText,
                            CaseManagerInitials = user.Initials,
                            CaseManagerName = user.Name,
                            CaseContent = foundCase.CustomFields.df1,
                            IsClosed = foundCase.CaseFileStatusCode == "A",
                            Note = foundCase.Note,
                            CaseNumberIdentifier = foundCase.CaseFileNumberIdentifier
                        });


                        //}
                    }
                }
            }
            return childCases;
        }
        public Child GetChildInfo(string CPR)
        {
            Child child = new Child();

            child = CPRBrokerService.GetChild(CPR);
            SearchCriterion searchCriterion = new SearchCriterion()
            {
                ChildCPR = CPR,
                CaseContent = "Løbende journal"
            };
            IEnumerable<ChildCase> childCases = SearchChildren(searchCriterion);
            foreach (var childCase in childCases)
            {
                child.CaseID = childCase.CaseID;
                child.Note = childCase.Note;
                child.CaseNumberIdentifier = childCase.CaseNumberIdentifier;
                child.CaseManagerInitials = childCase.CaseManagerInitials;
                child.CaseManagerName = childCase.CaseManagerName;
                if (!childCase.IsClosed) // Hvis sagen er afsluttet så bør vi lede videre
                    return child;
            }
            return child;
        }
        public Child GetChildInfo(int CaseID)
        {
            Child child = new Child();
            AcadreServiceV7.BUCaseFileType Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(CaseID.ToString());
            var user = GetUser(Case.CaseFileManagerReference);
            child = CPRBrokerService.GetChild(Case.CaseFileTitleText);
            child.Note = Case.Note;
            child.CaseID = CaseID;
            child.CaseNumberIdentifier = Case.CaseFileNumberIdentifier;
            child.CaseManagerInitials = user.Initials;
            child.CaseManagerName = user.Name;
            return child;
        }
        public int CreateChildJournal(string CPR, int AcadreOrgID, string CaseManagerInitials)
        {
            CPR = CPR.Replace("-", "").Trim();
            if (!CPRBrokerService.IsValidCPR(CPR)) throw new Exception("CPR-nummeret (" + CPR + ") er ikke gyldigt.");

            string KLE = "27.24.00";
            string CaseContent = "Løbende journal";
            string PublicationRestriction = "2"; // Aktindsigt, 2 = Delvis
            string CaseStatus = "B"; // Sagsstatus, B = Under Behandling
            string CaseDisposalCode = "B"; // Kassationskode, B = Bevares
            string DeletionCode = "P1825D"; // Slettekode
            string AccessCode = "BN"; // Adgangskode, BN = Børnesager
            string Classification = "G01"; // Facet
            string SubType = "Person"; // Undertype
            int SpecialistID = 8;
            int RecommendationID = 1;
            int CategoryID = 4;

            // AcadreOrgID = "58"; // Sagsansvarlig Enhed, OBS! skal ændres til at bruge brugerens organisationsplacering
            AcadreServiceV7.CaseFileType3 Case;

            // Undersøger om sagen allerede eksisterer
            AcadreServiceV7.AdvancedSearchCaseCriterionType2 searchCriterion = new AcadreServiceV7.AdvancedSearchCaseCriterionType2();
            searchCriterion.CaseFileTitleText = CPR;
            searchCriterion.CaseFileTypeCode = CaseFileTypeCode;
            searchCriterion.CustomFields = new AcadreServiceV7.CustomField[] { new AcadreServiceV7.CustomField { Value = CaseContent, Name = "df1" } };
            searchCriterion.ClassificationCriterion = new AcadreServiceV7.ClassificationCriterionType[]
            {
                new AcadreServiceV7.ClassificationCriterionType()
                {
                    ClassificationLiteral = KLE,
                    PrincipleLiteral = "KL Koder"
                }
            };
            string CurrentCaseFileStatusCode = "";
            string CurrentCaseID = "";

            // Kigger alle fundne sager igennem og tjekker om de indeholder "Løbende journal" i sagsindhold (CaseContent)
            foreach (AcadreServiceV7.CaseSearchResponseType foundCase in caseService.SearchCases(searchCriterion))
            {

                Case = caseService.GetCase(foundCase.CaseFileReference);

                //if (Case.CustomFields.df1.Contains(CaseContent))
                //{
                if (Case.CaseFileStatusCode == "B") // Er sagen åben?
                {
                    CurrentCaseID = foundCase.CaseFileReference;
                    CurrentCaseFileStatusCode = "B";
                    break; // Der blev fundet en åben sag. Så behøver vi ikke at lede mere
                }
                else if (Case.CaseFileStatusCode == "A" && CurrentCaseFileStatusCode != "B")
                {
                    CurrentCaseID = foundCase.CaseFileReference;
                    CurrentCaseFileStatusCode = "A";
                    // Der blev fundet en lukket sag, men vi leder videre for at se om der er en åben sag
                }
                //}
            }
            // Hvis der blev fundet en åben sag så returneres denne sags CaseID. Hvis der kun blev fundet en lukket sag så returneres -1.
            if (CurrentCaseID != "")
            {
                if (CurrentCaseFileStatusCode == "B")
                { }//return int.Parse(CurrentCaseID);
                else
                    return -1;
            }

            // look up contact by cprnumber
            string contactGUID;
            string contactName;
            var searchContactCriterion = new AcadreServiceV7.SearchContactCriterionType2();
            searchContactCriterion.ContactTypeName = "Person";
            searchContactCriterion.SearchTerm = CPR;
            var foundContacts = contactService.SearchContacts(searchContactCriterion);
            if (foundContacts.Length > 0)
            {
                // contact already exists, read GUID and name
                contactGUID = foundContacts.First().GUID;
                contactName = foundContacts.First().ContactTitle;
            }
            else
            {
                // forsøger at finde CPR i CPR Broker
                SimplePerson simplePerson;
                try
                {
                    simplePerson = CPRBrokerService.GetSimplePersonByCPR(CPR);
                }
                catch (Exception e)
                {
                    throw new Exception("CPR-nummeret (" + CPR + ") kunne ikke findes i CPR-registret", e);
                }
                // contact doesn't exist - create it and assign GUID
                var contact = new AcadreServiceV7.PersonType2();
                contact.PersonCivilRegistrationIdentifierStatusCode = "0";
                contact.PersonCivilRegistrationIdentifier = CPR;
                contact.PersonNameForAddressingName = contactName = simplePerson.FullName;
                contactGUID = contactService.CreateContact(contact);
            }

            var createCaseRequest = new AcadreServiceV7.CreateCaseRequestType();
            AcadreServiceV7.CaseFileType3 caseFile;
            //AcadreServiceV7.BUCaseFileType caseFile = new AcadreServiceV7.BUCaseFileType();
            if (CaseFileTypeCode == "BUSAG")
            {
                AcadreServiceV7.BUCaseFileType BUcaseFile = new AcadreServiceV7.BUCaseFileType();
                BUcaseFile.SpecialistId = SpecialistID; // Faggruppe
                BUcaseFile.SpecialistIdSpecified = true;
                BUcaseFile.RecommendationId = RecommendationID; // Henvendelse
                BUcaseFile.RecommendationIdSpecified = true;
                BUcaseFile.CategoryId = CategoryID; // Kategori
                BUcaseFile.CategoryIdSpecified = true;
                caseFile = BUcaseFile;
            }
            else
            {
                caseFile = new AcadreServiceV7.CaseFileType3();
            }
            caseFile.CaseFileTypeCode = CaseFileTypeCode;
            caseFile.Year = DateTime.Now.Year.ToString();
            caseFile.CreationDate = DateTime.Now;
            caseFile.CaseFileTitleText = CPR;
            caseFile.TitleAlternativeText = contactName;
            caseFile.RestrictedFromPublicText = PublicationRestriction;
            caseFile.CaseFileStatusCode = CaseStatus;
            caseFile.CaseFileDisposalCode = CaseDisposalCode;
            caseFile.DeletionCode = DeletionCode;
            caseFile.AccessCode = AccessCode;
            caseFile.SubType = SubType;
            caseFile.AdministrativeUnit = new AcadreServiceV7.AdministrativeUnitType[]
            {
                            new AcadreServiceV7.AdministrativeUnitType() { AdministrativeUnitReference = AcadreOrgID.ToString() }
            };

            caseFile.CustomFieldCollection = new AcadreServiceV7.CustomField[]
            {
                            new AcadreServiceV7.CustomField(){Name = "df1",Value = CaseContent}
                            ,new AcadreServiceV7.CustomField(){Name = "df25",Value = contactGUID} //contactGUID
            };

            caseFile.Classification = new AcadreServiceV7.ClassificationType
            {
                Category = new AcadreServiceV7.CategoryType[] {
                            new AcadreServiceV7.CategoryType(){ Principle="KL Koder", Literal = KLE }
                            ,new AcadreServiceV7.CategoryType(){ Principle="Facetter", Literal = Classification}
                        }
            };

            caseFile.Party = new AcadreServiceV7.PartyType[] { new AcadreServiceV7.PartyType() {
                            CreationDate = DateTime.Now
                            ,ContactReference = contactGUID
                            ,PublicAccessLevelReference = "3"
                            ,IsPrimary = true
                        } };

            createCaseRequest.CaseFile = caseFile;

            var userList = configurationService.GetUserList(new AcadreServiceV7.EmptyRequestType()).ToList();
            var user = userList.SingleOrDefault(ut => ut.Initials == CaseManagerInitials);
            if (user != null)
            {
                createCaseRequest.CaseFile.CaseFileManagerReference = user.Id;
            }
            //createCaseRequest.CaseFile.
            var createCaseResponse = caseService.CreateCase(createCaseRequest);
            // check for multicase (samlesag) response.
            if (createCaseResponse.CreateCaseAndAMCResult == AcadreServiceV7.CreateCaseAndAMCResultType.CaseNotCreatedAndListAMCReceived)
            {
                // create the case in all the multicases
                createCaseRequest.MultiCaseIdentifiers = createCaseResponse.MultiCaseIdentifiers;
                createCaseResponse = createCaseResponse = caseService.CreateCase(createCaseRequest);
            }
            return int.Parse(createCaseResponse.CaseFileIdentifier);
        }
        public IEnumerable<ChildCase> GetChildCases(int CaseID)
        {
            List<ChildCase> childCases = new List<ChildCase>();
            AcadreServiceV7.BUCaseFileType Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(CaseID.ToString());
            string CPR = Case.CaseFileTitleText;

            AcadreServiceV7.AdvancedSearchCaseCriterionType2 searchCriterion = new AcadreServiceV7.AdvancedSearchCaseCriterionType2();
            searchCriterion.CaseFileTitleText = CPR;
            searchCriterion.CaseFileTypeCode = CaseFileTypeCode;
            var userList = configurationService.GetUserList(new AcadreServiceV7.EmptyRequestType()).ToList();
            foreach (AcadreServiceV7.CaseSearchResponseType foundCase in caseService.SearchCases(searchCriterion))
            {
                Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(foundCase.CaseFileReference);
                var user = userList.SingleOrDefault(ut => ut.Id == Case.CaseFileManagerReference);
                if (user == null)
                {
                    user = new AcadreServiceV7.UserType() { Initials = "", Name = "" };
                }
                childCases.Add(new ChildCase
                {
                    CaseID = int.Parse(foundCase.CaseFileReference),
                    CaseContent = Case.CustomFields.df1,
                    ChildCPR = CPR,
                    CaseNumberIdentifier = Case.CaseFileNumberIdentifier,
                    CaseManagerInitials = user.Initials,
                    CaseManagerName = user.Name,
                    Note = Case.Note,
                    ChildName = Case.TitleAlternativeText,
                    IsClosed = Case.CaseFileStatusCode == "A"
                });
            }
            return childCases;
        }
        public IEnumerable<JournalDocument> GetChildJournalDocuments(int CaseID)
        {
            List<JournalDocument> journalDocuments = new List<JournalDocument>();
            var childCases = GetChildCases(CaseID);
            // Henter dokumenter
            foreach (ChildCase childCase in childCases)
            {
                journalDocuments.AddRange(GetChildCaseDocuments(childCase.CaseID));
            }

            // Henter notater
            var Case = caseService.GetCase(CaseID.ToString());
            foreach (var memo in memoService4.GetAllMemo(CaseID.ToString()))
            {
                var outputbinary = documentService4.GetPhysicalDocument(new AcadreServiceV4.FileVersionReferenceType()
                {
                    FileReference = memo.MemoFileReference,
                    Version = "1"
                });
                FlowDocument document = new FlowDocument();
                TextRange txtRange = null;
                using (MemoryStream stream = new MemoryStream(outputbinary))
                {
                    // create a TextRange around the entire document
                    txtRange = new TextRange(document.ContentStart, document.ContentEnd);
                    txtRange.Load(stream, DataFormats.Rtf);
                }
                var memo7 = memoService.GetMemo(memo.MemoIdentifier);
                var memo4 = memoService4.GetMemo(memo.MemoIdentifier);
                journalDocuments.Add(new JournalDocument
                {
                    Type = "Memo",
                    DocumentID = int.Parse(memo.MemoIdentifier),
                    Title = memo.MemoTitleText,
                    LastChangedDate = memo.MemoEventDate,
                    CaseID = CaseID,
                    CaseNumberIdentifier = Case.CaseFileNumberIdentifier,
                    DocumentMemoDescription = txtRange.Text // Her skal der bruges indholdet af notatet
                });
            }

            return journalDocuments;
        }
        // Denne metode er kun midlertidig ind til Formpipe får løst problemet med GetAllMemo
        public IEnumerable<JournalDocument> GetChildJournalDocuments(int CaseID, int[] MemoIDs)
        {
            List<JournalDocument> journalDocuments = new List<JournalDocument>();
            AcadreServiceV4.MemoType memo;
            byte[] outputbinary;
            var childCases = GetChildCases(CaseID);
            // Henter dokumenter
            foreach (ChildCase childCase in childCases)
            {
                journalDocuments.AddRange(GetChildCaseDocuments(childCase.CaseID));
            }
            var Case = caseService.GetCase(CaseID.ToString());
            if (MemoIDs == null)
                return journalDocuments;

            foreach (var MemoID in MemoIDs)
            {
                try
                {
                    memo = memoService4.GetMemo(MemoID.ToString());
                    outputbinary = documentService4.GetPhysicalDocument(new AcadreServiceV4.FileVersionReferenceType()
                    {
                        FileReference = memo.MemoFileReference,
                        Version = "1"
                    });
                }
                catch (Exception e)
                {
                    //throw new Exception("Kunne ikke hente journalnotat " + MemoID + " på sagen",e);
                    continue;
                }
                FlowDocument document = new FlowDocument();
                TextRange txtRange = null;
                using (MemoryStream stream = new MemoryStream(outputbinary))
                {
                    // create a TextRange around the entire document
                    txtRange = new TextRange(document.ContentStart, document.ContentEnd);
                    txtRange.Load(stream, DataFormats.Rtf);
                }
                journalDocuments.Add(new JournalDocument
                {
                    Type = "Memo",
                    DocumentID = MemoID,
                    Title = memo.MemoTitleText,
                    LastChangedDate = memo.MemoEventDate,
                    CaseID = CaseID,
                    CaseNumberIdentifier = Case.CaseFileNumberIdentifier,
                    DocumentMemoDescription = txtRange.Text // Her skal der bruges indholdet af notatet
                });
            }
            return journalDocuments;
        }
        public IEnumerable<JournalDocument> GetChildCaseDocuments(int CaseID)
        {
            List<JournalDocument> journalDocuments = new List<JournalDocument>();
            var Case = caseService.GetCase(CaseID.ToString());

            string caseNumberIdentifier = Case.CaseFileNumberIdentifier;
            foreach (var document in documentService.GetAllDocuments(CaseID.ToString()))
            {
                var document7 = documentService.GetMainDocument(document.Document.DocumentIdentifier);
                journalDocuments.Add(new JournalDocument
                {
                    Type = "Document",
                    DocumentID = int.Parse(document.Document.DocumentIdentifier),
                    Title = document.Document.DocumentTitleText,
                    LastChangedDate = document.Record.RegistrationDate,
                    CaseID = CaseID,
                    CaseNumberIdentifier = Case.CaseFileNumberIdentifier,
                    DocumentMemoDescription = document.Record.DescriptionText
                });
            }
            return journalDocuments;
        }
        public void SetBUComment(int CaseID, string NewComment)
        {
            var Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(CaseID.ToString());
            Case.Note = NewComment;
            try
            {
                caseService.UpdateCase(Case);
            }
            catch (Exception e)
            {
                throw new Exception("Kunne ikke opdatere Acadre sagen", e);
            }

        }
        public JournalDocument GetMemo(int MemoID)
        {
            JournalDocument journalDocument;
            var memo = memoService4.GetMemo(MemoID.ToString());
            var outputbinary = documentService4.GetPhysicalDocument(new AcadreServiceV4.FileVersionReferenceType()
            {
                FileReference = memo.MemoFileReference,
                Version = "1"
            });
            File.WriteAllBytes("C:/Temp/test.rtf", outputbinary);
            FlowDocument document = new FlowDocument();
            TextRange txtRange = null;
            using (MemoryStream stream = new MemoryStream(outputbinary))
            {
                // create a TextRange around the entire document
                txtRange = new TextRange(document.ContentStart, document.ContentEnd);
                txtRange.Load(stream, DataFormats.Rtf);
            }
            journalDocument = new JournalDocument
            {
                Type = "Memo",
                DocumentID = MemoID,
                Title = memo.MemoTitleText,
                LastChangedDate = memo.MemoEventDate,
                CaseID = int.Parse(memo.CaseFileReference),
                CaseNumberIdentifier = "",
                DocumentMemoDescription = txtRange.Text // Her skal der bruges indholdet af notatet
            };
            return journalDocument;
        }
        public int CreateMemo(MemoRequest memoRequest)
        {
            var now = DateTime.Now;

            var memoType = new AcadreLib.AcadreServiceV7.TimedJournalMemoType();
            memoType.AccessCode = memoRequest.accessCode;
            memoType.CaseFileReference = memoRequest.caseFileReference;
            memoType.CreationDate = now;

            var userList = configurationService.GetUserList(new AcadreLib.AcadreServiceV7.EmptyRequestType()).ToList();
            var user = userList.SingleOrDefault(ut => ut.Initials == memoRequest.creatorReference);
            if (user != null)
            {
                memoType.CreatorReference = user.Id;
            }

            memoType.MemoTypeReference = memoRequest.memoTypeReference;
            memoType.IsLocked = memoRequest.isLocked;
            memoType.MemoEventDate = memoRequest.eventDate;
            memoType.MemoTitleText = memoRequest.titleText;

            var tempMemo = new AcadreLib.AcadreServiceV7.CreateTimedJournalMemoRequestType();
            tempMemo.FileName = memoRequest.fileName;
            tempMemo.XMLBinary = memoRequest.fileBytes;
            tempMemo.TimedJournalMemo = memoType;

            var memoId = int.Parse(memoService.CreateMemo(tempMemo));
            return memoId;
        }
        // Changes the Responsible CaseManager on all the childrens cases where the CaseManagerInitials = CaseManagerInitialsFrom. If CaseManagerInitialsFrom is "" then all child cases CaseManager is changed to CaseManagerInitialsTo.
        public void ChangeChildResponsible(string oldCaseManagerInitials, string newCaseManagerInitials, int CaseID)
        {
            List<ChildCase> childCases = new List<ChildCase>();
            AcadreServiceV7.BUCaseFileType Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(CaseID.ToString());
            string CPR = Case.CaseFileTitleText;

            AcadreServiceV7.AdvancedSearchCaseCriterionType2 searchCriterion = new AcadreServiceV7.AdvancedSearchCaseCriterionType2();
            searchCriterion.CaseFileTitleText = CPR;
            searchCriterion.CaseFileTypeCode = CaseFileTypeCode;
            var userList = configurationService.GetUserList(new AcadreServiceV7.EmptyRequestType()).ToList();
            foreach (AcadreServiceV7.CaseSearchResponseType foundCase in caseService.SearchCases(searchCriterion))
            {
                Case = (AcadreServiceV7.BUCaseFileType)caseService.GetCase(foundCase.CaseFileReference);
                var user = userList.SingleOrDefault(ut => ut.Id == Case.CaseFileManagerReference);
                if (user == null)
                    user = new AcadreServiceV7.UserType { Initials = "" };
                if (user.Initials == oldCaseManagerInitials || oldCaseManagerInitials == "" || user.Initials == "")
                {
                    var newUser = userList.SingleOrDefault(ut => ut.Initials == newCaseManagerInitials);
                    if (newUser == null)
                        throw new Exception("Der fandtes ikke en bruger i Acadre med initialerne " + newCaseManagerInitials);
                    Case.CaseFileManagerReference = newUser.Id;
                    try
                    {
                        caseService.UpdateCase(Case);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Kunne ikke opdatere Acadre sagen", e);
                    }
                }
            }
        }
        private AcadreServiceV7.UserType GetUser(string UserReference)
        {
            var userList = configurationService.GetUserList(new AcadreServiceV7.EmptyRequestType()).ToList(); // Herfra kan CaseManager aflæses
            return userList.SingleOrDefault(ut => ut.Id == UserReference);
        }
    }
}
