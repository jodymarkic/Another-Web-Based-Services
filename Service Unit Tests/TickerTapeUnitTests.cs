using System;
using System.Web.Services.Protocols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
/*
 *  FILENAME        : TickerTapeUnitTests.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains test methods for the Ticker tape service
 */
namespace Service_Unit_Tests.TickerTape
{
    [TestClass]
    public class TickerTapeUnitTests
    {
        //public struct QuoteData
        //{
        //    public string actualSymbol;
        //    public double actualLastPrice;
        //    public string actualLastPriceDate;
        //    public string actualLastPriceTime;
        //}

        //[TestMethod]
        //public void TestGetQuote()
        //{
        //    // Create Struct
        //    QuoteData results = new QuoteData();
        //    // Instantiate Class
        //    Stocks test = new Stocks();
        //    // Create expectations
        //    string expectedResult = "QQQ";
        //    // Call Service
        //    test.GetQuote("QQQ");
        //    test.Equals(results);
        //    // Compare Results.
        //    Assert.AreEqual(expectedResult, results.actualSymbol);
        //}


        //
        //  METHOD      : TickerTape_CheckForSoapException
        //  DESCRIPTION : Test provides a bad input string and looks for a soap exception to be thrown.
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        [TestMethod]
        [ExpectedException(typeof(SoapException), "An incorrect Ticker Symbol was provided.")]
        public void TickerTape_CheckForSoapException()
        {
            Stocks test = new Stocks();
            test.GetQuote("QRQ");
        }
    }
}
