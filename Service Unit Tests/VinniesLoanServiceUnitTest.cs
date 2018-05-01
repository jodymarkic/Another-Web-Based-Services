using System;
using System.Web.Services.Protocols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
/*
 *  FILENAME        : VinniesLoanServiceUnitTest.cs
 *  PROJECT         : SOA_A03
 *  PROGRAMMER      : Jody Markic, Sean Moulton
 *  FIRST VERSION   : 10/16/2017
 *  DESCRIPTION     : This File contains test methods for Vinnies Loan Service
 */
namespace Service_Unit_Tests.VinniesLoanService
{
    [TestClass]
    public class VinniesLoansUnitTest
    {
        //
        //  METHOD      : Loan_ServiceTest
        //  DESCRIPTION : Provides good parameters and looks for a match between expected and actual results.
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        [TestMethod]
        public void Loan_ServiceTest()
        {
            Loans test = new Loans();
            float actualResults = test.LoanPayment(15000, 8, 12);
            float expectedResults = (float)1304.83;
            Assert.AreEqual(expectedResults, actualResults);
        }

        //
        //  METHOD      : Loan_CheckException
        //  DESCRIPTION : Gives bad input and looks for soap exception to be thrown.
        //  PARAMETERS  : na
        //  RETURNS     : na
        //
        [TestMethod]
        [ExpectedException(typeof(SoapException), "A negative float is provided for principle.")]
        public void Loan_CheckException()
        {
            Loans test = new Loans();
            float actualResults = test.LoanPayment(-1, 8, 12);
        }
    }
}
