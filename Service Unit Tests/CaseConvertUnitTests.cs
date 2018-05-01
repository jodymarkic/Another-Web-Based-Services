using System;
using System.Web.Services.Protocols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
/*
 *  FILENAME        : CaseConvertUnitTests.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains methods used to test the Text Service
 */
namespace Service_Unit_Tests.TextService
{
    [TestClass]
    public class CaseConvertUnitTest
    {
        //
        //  METHOD      : Case_TestToUpper
        //  DESCRIPTION : Tests services toUpper functionality
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        [TestMethod]
        public void Case_TestToUpper()
        {
            Case test = new Case();
            // 1 = toUpper
            string actualResult = test.CaseConvert("hello", 1);
            string expectedResult = "HELLO";
            Assert.AreEqual(expectedResult, actualResult);
        }
        //
        //  METHOD      : Case_TestToLower
        //  DESCRIPTION : Tests services toLower functionality
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        public void Case_TestToLower()
        {
            Case test = new Case();
            // 2 = toLower
            string actualResult = test.CaseConvert("HELLO", 2);
            string expectedResult = "hello";
            Assert.AreEqual(expectedResult, actualResult);
        }
        //
        //  METHOD      : Case_TestBadCharacter
        //  DESCRIPTION : Provides bad input and looks for a soap exception to be thrown.
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        [TestMethod]
        [ExpectedException(typeof(SoapException), "A integer was provided for case conversion.")]
        public void Case_TestBadCharacter()
        {
            Case test = new Case();
            string actualResult = test.CaseConvert("25", 1);
        }
    }
}
