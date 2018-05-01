using System;
using System.Web;
using System.Web.Services;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web.Services.Protocols;
using System.IO;

/*
 *  FILENAME        : Case.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains methods and class pertaining to Case Convert Service
 */

[WebService(Namespace = "http://localhost/TextService")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Case : System.Web.Services.WebService
{
    //
    //  CLASS: Case
    //  DESCRIPTION: This Class holds methods used in the Case Conversion Service
    //
    //
    enum convertCaseFlags { toUpper = 1, toLower = 2 };
    enum parameterType { letters = 0, numbers };
    string[] regexExpressions = { @"^[a-zA-Z]+$", @"^[0-9]+$" };

    public Case()
    {
        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }
    //
    //  METHOD      : CaseConvert
    //  DESCRIPTION : Determines what case to switch text to, calls methods to convert or logs error and throws exception.
    //  PARAMETERS  : string incoming - Client provided argument
    //                int flag        - flag used to notate if case is to be converted toUpper or toLower
    //  RETURNS     : string outgoing - convert text to which ever case was chosen.
    //
    [WebMethod]
    public string CaseConvert(string incoming, int flag)
    {
        string outgoing = "";

        if (EvaluateParameters(incoming, regexExpressions[(int)parameterType.letters]))
        {
            switch (flag)
            {
                case (int)convertCaseFlags.toUpper:
                    outgoing = incoming.ToUpper();
                    break;
                case (int)convertCaseFlags.toLower:
                    outgoing = incoming.ToLower();
                    break;
                default:
                    LogAndThrow(WhichDetails((int)parameterType.numbers));
                    break;
            }
        }
        else
        {
            LogAndThrow(WhichDetails((int)parameterType.letters));
        }

        return outgoing;
    }
    //
    //  METHOD      : EvaluateParameters
    //  DESCRIPTION : Consructor
    //  PARAMETERS  : string incoming   - text provided from client
    //                string evaluation - regex string to compare incoming to.
    //  RETURNS     : bool - success if incoming passes evaluation
    //
    public bool EvaluateParameters(string incoming, string evaluation)
    {
        bool result = false;

        if (Regex.IsMatch(incoming, evaluation))
        {
            result = true;
        }

        return result;
    }
    //
    //  METHOD      : BuildDetail
    //  DESCRIPTION : Create bulk of error log, parameters fill in the specific details like error type, variable name, and explanation
    //  PARAMETERS  : string name                 - Variable name
    //                string type                 - Error type
    //                string explanation          - Explanation of error
    //  RETURNS     : XmlNode node                - XML containing constructed log.
    //
    public XmlNode BuildDetail(string name, string type, string explanation)
    {
        XmlDocument doc = new System.Xml.XmlDocument();
        XmlNode node = doc.CreateNode(XmlNodeType.Element, SoapException.DetailElementName.Name, SoapException.DetailElementName.Namespace);
        XmlNode details;
        XmlAttribute nameAttr;
        XmlAttribute errorTypeAttr;
        XmlAttribute explanationAttr;

        details = doc.CreateNode(XmlNodeType.Element, "ParameterError", "http://localhost/TextService");
        nameAttr = doc.CreateAttribute("s", "name", "http://localhost/TextService");
        nameAttr.Value = name;
        errorTypeAttr = doc.CreateAttribute("s", "errorType", "http://localhost/TextService");
        errorTypeAttr.Value = type;
        explanationAttr = doc.CreateAttribute("s", "explanation", "http://localhost/TextService");
        explanationAttr.Value = explanation;

        details.Attributes.Append(nameAttr);
        details.Attributes.Append(errorTypeAttr);
        details.Attributes.Append(explanationAttr);

        node.AppendChild(details);

        return node;
    }
    //
    //  METHOD      : WhichDetails
    //  DESCRIPTION : Based on flag provided, generates specific details for error log.
    //  PARAMETERS  : int flag        - represents what error or exception as occurred.
    //  RETURNS     : XmlNode details - Object containing specific details selected for error log. 
    //
    public XmlNode WhichDetails(int flag)
    {
        XmlNode details;

        switch (flag)
        {
            case (int)parameterType.letters:
                details = BuildDetail("incoming", "input", "Only accepts letters in the incoming string");
                break;
            case (int)parameterType.numbers:
                details = BuildDetail("flag", "input", "Only accepts numbers 1 & 2; One: converts to Upper-case; Two: converts to Lower-case");
                break;
            default:
                details = null;
                break;
        }

        return details;
    }
    //
    //  METHOD      : ThrowSoapFault
    //  DESCRIPTION : Method used to throw soap fault.
    //  PARAMETERS  : XmlNode detail - Details of error log.
    //  RETURNS     : na
    //
    public void ThrowSoapFault(XmlNode detail)
    {
        //Throw the exception    
        SoapException se = new SoapException("Invalid Input",
        SoapException.ClientFaultCode,
        Context.Request.Url.AbsoluteUri,
        detail);

        throw se;
        //here we throw the exception.
    }
    //
    //  METHOD      : LogAndThrow
    //  DESCRIPTION : Creates error leg based on Soap fault and prints to txt after validation has occurred. Throws soap exception when called.
    //  PARAMETERS  : XmlNode detail - containing information concerning why exception was thrown.
    //  RETURNS     : na
    //
    public void LogAndThrow(XmlNode detail)
    {
        SoapException se = new SoapException("Invalid Input",
        SoapException.ClientFaultCode,
        Context.Request.Url.AbsoluteUri,
        detail);

        string logFile = "~/App_Data/ErrorLog.txt";
        logFile = HttpContext.Current.Server.MapPath(logFile);

        StreamWriter sw = new StreamWriter(logFile, true);
        sw.WriteLine("********** {0} **********", DateTime.Now);

        sw.WriteLine("Fault Code Namespace: " + se.Code.Namespace);
        sw.WriteLine("Fault Code Name: " + se.Code.Name);
        sw.WriteLine("SOAP Actor that threw Exception: " + se.Actor);
        sw.WriteLine("Fault Details: " + se.Message);

        XmlAttributeCollection errorAttributes = se.Detail.ChildNodes[0].Attributes;

        sw.WriteLine("Parameter Name: " + errorAttributes["s:name"].Value);
        sw.WriteLine("Fault Type: " + errorAttributes["s:errorType"].Value);
        sw.WriteLine("Fault Explanation: " + errorAttributes["s:explanation"].Value);

        sw.Close();

        throw se;
    }
}