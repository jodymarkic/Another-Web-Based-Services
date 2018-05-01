using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
/*
 *  FILENAME        : Stocks.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains methods and class pertaining to the Ticker Tape Service
 */
[WebService(Namespace = "http://localhost/TickerTape")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Stocks : System.Web.Services.WebService
{
    //
    //  CLASS: Stocks
    //  DESCRIPTION: This Class holds all methods used in the Ticker Tape Service.
    //
    //
    public struct QuoteData
    {
        public string Symbol;
        public double LastPrice;
        public string LastPriceDate;
        public string LastPriceTime;
    }
    string regexExpression = @"^[A-Z\.\^]{2,10}$";
    object resultBuffer;

    public Stocks()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 

    }



    //
    //  METHOD      : EvaluateParameters
    //  DESCRIPTION : Consructor
    //  PARAMETERS  : string incoming   - text provided from client
    //                string evaluation - regex string to compare incoming to.
    //  RETURNS     : bool - success if incoming passes evaluation
    //
    public bool EvaluateParameters(string tickerSymbol, string evaluation)
    {
        bool result = false;

        if (Regex.IsMatch(tickerSymbol, evaluation))
        {
            result = true;
            //all good
        }

        return result;
    }

    //
    //  METHOD      : GetQuote
    //  DESCRIPTION : Calls service, parses data, and validates return. Stores in struct for display.
    //  PARAMETERS  : string tickerSymbol - ticker symbol represents stocks ticker symbol
    //  RETURNS     : struct myData - struct containing response from service
    //
    [WebMethod]
    public QuoteData GetQuote(string tickerSymbol)
    {

        bool myException = false;

        QuoteData myData = new QuoteData();
        try
        {
            if (EvaluateParameters(tickerSymbol, regexExpression))
            {
                com.cdyne.ws.DelayedStockQuote myDelayedQuote = new com.cdyne.ws.DelayedStockQuote();
                com.cdyne.ws.QuoteData myQuoteData = myDelayedQuote.GetQuote(tickerSymbol, "0");

                myData.Symbol = myQuoteData.StockSymbol;
                decimal lastPriceBuffer = myQuoteData.LastTradeAmount;
                Double.TryParse(lastPriceBuffer.ToString(), out myData.LastPrice);
                myData.LastPriceDate = myQuoteData.LastTradeDateTime.ToString("MM/dd/yyyy");
                myData.LastPriceTime = myQuoteData.LastTradeDateTime.ToString("HH:mm");

            }
            else
            {
                myException = true;
                ThrowSoapFault(BuildDetail("tickerSymbol", "input", "Your Ticker symbol format is wrong, Valid symbols only contain upper-case, '.'s, and '^'s characters \n "));
            }
        }
        catch (SoapException error)
        {
            if (myException)
            {
                LogAndRethrow(error, myException);
            }
            else
            {
                LogAndRethrow(error, myException);
            }
        }

        return myData;
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

        details = doc.CreateNode(XmlNodeType.Element, "ParameterError", "http://localhost/TickerTape");
        nameAttr = doc.CreateAttribute("s", "name", "http://localhost/TickerTape");
        nameAttr.Value = name;
        errorTypeAttr = doc.CreateAttribute("s", "errorType", "http://localhost/TickerTape");
        errorTypeAttr.Value = type;
        explanationAttr = doc.CreateAttribute("s", "explanation", "http://localhost/TickerTape");
        explanationAttr.Value = explanation;

        details.Attributes.Append(nameAttr);
        details.Attributes.Append(errorTypeAttr);
        details.Attributes.Append(explanationAttr);

        node.AppendChild(details);

        return node;
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
    public void LogAndRethrow(SoapException error, bool myException)
    {
        string logFile = "~/App_Data/ErrorLog.txt";
        logFile = HttpContext.Current.Server.MapPath(logFile);

        StreamWriter sw = new StreamWriter(logFile, true);
        sw.WriteLine("********** {0} **********", DateTime.Now);

        sw.WriteLine("Fault Code Namespace: " + error.Code.Namespace);
        sw.WriteLine("Fault Code Name: " + error.Code.Name);
        sw.WriteLine("SOAP Actor that threw Exception: " + error.Actor);
        sw.WriteLine("Fault Details: " + error.Message);

        if (myException)
        {
            XmlAttributeCollection errorAttributes = error.Detail.ChildNodes[0].Attributes;
            sw.WriteLine("Method Name: GetQuote");
            sw.WriteLine("Parameter Name: " + errorAttributes["s:name"].Value);
            sw.WriteLine("Fault Type: " + errorAttributes["s:errorType"].Value);
            sw.WriteLine("Fault Explanation: " + errorAttributes["s:explanation"].Value);
        }


        sw.Close();

        throw error;
    }
}