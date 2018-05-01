using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.IO;

/*
 *  FILENAME        : Loans.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains methods and class pertaining to Vinnies Loan Service
 */

/// <summary>
/// Summary description for Loans
/// </summary>
[WebService(Namespace = "http://localhost/LoanPayment/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]


//
//  CLASS: Loans 
//  DESCRIPTION: This Class holds all methods used in Vinnies Loan Service
//
//
public class Loans : System.Web.Services.WebService
{
    public Loans()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    //
    //  METHOD      : LoanPayment
    //  DESCRIPTION : Method used as starting point. Parameters are input for service. Validates input and calls other methods and constructs monthly payments for reponse. 
    //  PARAMETERS  : float principle      - Principle Amount
    //                float rate           - Rate of Interest
    //                int payments         - Amount of payments to be made.
    //  RETURNS     : float monthlyPayment - Amount of each payment to be made.
    //
    [WebMethod]
    public float LoanPayment(float principle, float rate, int payments)
    {
        bool principleIsPos = false;
        bool rateIsPos = false;
        bool paymentsIsPos = false;

        float monthlyPayment = 0;
        float monthlyRate = 0;
        float middleOfEquation = 0;

        // Validate incoming parameters
        principleIsPos = CheckRange(principle);
        rateIsPos = CheckRange(rate);
        paymentsIsPos = CheckRange(payments);


        if (principleIsPos == true && rateIsPos == true && paymentsIsPos == true)
        {
            // Calculate our rate
            monthlyRate = CalculateRate(rate);

            // Generate the middle of the equation as per the example.
            middleOfEquation = CalculateMiddleOfFormula(monthlyRate, payments);

            // Finish Equation and Calculate our return value
            monthlyPayment = CalculateLoan(principle, monthlyRate, middleOfEquation);
        }
        else
        {
            if (principleIsPos == false)
            {
                LogAndThrow(WhichDetails(1));
            }

            if (rateIsPos == false)
            {
                LogAndThrow(WhichDetails(2));
            }

            if (paymentsIsPos == false)
            {
                LogAndThrow(WhichDetails(3));
            }
            // If Incoming bad. Return Static Error response of -1
             monthlyPayment = -1;
            
        }
        // Return monthly payment
        return monthlyPayment;
    }
    //
    //  METHOD      : CalculateLoan
    //  DESCRIPTION : Third part of monthly rate calculation. Rounds final answer to 2 decimal places to represent currency.
    //  PARAMETERS  : float principle             - Principle Amount
    //                float monthlyRate           - How much to paid in each payment
    //                float middleOfTheEquation   - Calculation that needs to be carried over to finish equation
    //  RETURNS     : float monthlyPayment        - Rounded monthly payment.
    //
    private float CalculateLoan(float principle, float monthlyRate, float middleOfTheEquation)
    {
        // Plug numbers into rest of Loan Payment Formula
        float monthlyPayment = (monthlyRate + middleOfTheEquation) * principle;

        // Convert float and round to 2 dec places.
        Decimal toRound = (decimal)monthlyPayment;
        monthlyPayment = (float)Decimal.Round(toRound, 2);

        return monthlyPayment;
    }
    //
    //  METHOD      : CalculateRate
    //  DESCRIPTION : First part of Loan Rate calculation. Determines rate for equation.
    //  PARAMETERS  : float rate           - Rate of Interest
    //  RETURNS     : float monthlyRate    - rate for equation
    //
    private float CalculateRate(float rate)
    {
        float monthlyRate = 0;

        // Calculate monthly rate for loan payment formula
        monthlyRate = rate / 1200;
        return monthlyRate;
    }
    //
    //  METHOD      : CalculateMiddleOfFormula
    //  DESCRIPTION : Second part of Loan Rate calculation. Simplifies formula.
    //  PARAMETERS  : float rate   - Rate for equation provided by CalculateLoan
    //                int payments - Amount of payments to be made.
    //  RETURNS     : asFloat - Middle of the equation simplified as a single float.
    //
    private float CalculateMiddleOfFormula(float rate, int payments)
    {
        // Step 4 of the example.
        // http://www.1728.org/loanform.htm

        // Generating numbers for formula
        float monthlyRatePlusOne = 1;
        monthlyRatePlusOne += rate;
        double middleOfFormula = 0;
        double powerResult = Math.Pow(monthlyRatePlusOne, payments);

        // Plug in numbers for formula and convert back to float
        middleOfFormula = (rate / (powerResult - 1));
        float asFloat = (float)middleOfFormula;

        return asFloat;
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
            case 1:
                details = BuildDetail("Principle", "input", "Only accepts positive numbers.");
                break;
            case 2:
                details = BuildDetail("Rate", "input", "Only accepts positive numbers.");
                break;
            case 3:
                details = BuildDetail("Payments", "input", "Only accepts positive numbers.");
                break;
            default:
                details = null;
                break;
        }

        return details;
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

        details = doc.CreateNode(XmlNodeType.Element, "ParameterError", "http://localhost/LoanService");
        nameAttr = doc.CreateAttribute("s", "name", "http://localhost/LoanService");
        nameAttr.Value = name;
        errorTypeAttr = doc.CreateAttribute("s", "errorType", "http://localhost/LoanService");
        errorTypeAttr.Value = type;
        explanationAttr = doc.CreateAttribute("s", "explanation", "http://localhost/LoanService");
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
    //  METHOD      : CheckRange
    //  DESCRIPTION : checks if parameter is a positive float.
    //  PARAMETERS  : float num - Number to check
    //  RETURNS     : bool
    //
    private bool CheckRange(float num)
    {
        if (num <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //
    //  METHOD      : CheckRange
    //  DESCRIPTION : checks if parameter is a positive int.
    //  PARAMETERS  : int num - Number to check
    //  RETURNS     : bool
    //
    private bool CheckRange(int num)
    {
        if (num <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //
    //  METHOD      : myForm
    //  DESCRIPTION : Consructor
    //  PARAMETERS  : na
    //  RETURNS     : na
    //
}
