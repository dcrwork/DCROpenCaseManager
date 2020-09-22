using System;
using System.Web.Services.Protocols;
using System.IO;

namespace AcadreLib.Acadre
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://traen.com/Services/AcadreService")]
    [System.Xml.Serialization.XmlRootAttribute("SoapHeader", Namespace = "http://traen.com/Services/AcadreService", IsNullable = false)]
    public partial class SoapHeaderType : System.Web.Services.Protocols.SoapHeader
    {
        private string userField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string User
        {
            get
            {
                return this.userField;
            }
            set
            {
                this.userField = value;
            }
        }
    }

    public class PWSHeaderExtension : SoapExtension
    {
        Stream oldStream;
        Stream newStream;

        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }

        public override object GetInitializer(Type serviceType)
        {
            return "C:\\" + serviceType + ".log";
        }

        public override void Initialize(object initializer)
        {

        }

        //Save the Stream representing the SOAP request or SOAP response into
        //a local memory buffer.
        public override Stream ChainStream(Stream stream)
        {
            oldStream = stream;
            newStream = new MemoryStream();
            return newStream;
        }

        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    //Add the CustomSoapHeader to outgoing client requests
                    if (message is SoapClientMessage)
                        AddHeader(message);
                    break;
                case SoapMessageStage.AfterSerialize:
                    newStream.Position = 0;
                    Copy(newStream, oldStream);
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    Copy(oldStream, newStream);
                    newStream.Position = 0;
                    break;
                case SoapMessageStage.AfterDeserialize:
                    break;
            }
        }

        public static String User = String.Empty;

        private void AddHeader(SoapMessage message)
        {
            if (String.IsNullOrEmpty(User))
                throw new Exception("No user specified. Use the 'User' property to set the user initials to be parsed with the soap header.");
            SoapHeaderType header = new SoapHeaderType();
            header.User = User;
            message.Headers.Add(header);
        }

        private void Copy(Stream from, Stream to)
        {
            TextReader reader = new StreamReader(from);
            TextWriter writer = new StreamWriter(to);
            writer.WriteLine(reader.ReadToEnd());
            writer.Flush();
        }

    }

    //Create a SoapExtensionAttribute for the SOAP Extension that can be 
    //applied to an XML Web Service method
    [AttributeUsage(AttributeTargets.Method)]
    public class PWSHeaderExtensionAttribute : SoapExtensionAttribute
    {
        public override Type ExtensionType
        {
            get { return typeof(PWSHeaderExtension); }
        }

        public override int Priority
        {
            get
            {
                return 100;
            }
            set
            {

            }
        }
    }


}