using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace org.thecrossingchurch.Tests.Integrations
{
    public class AdminTests
    {
        String base_url;

        IWebDriver driver;

        [OneTimeSetUp]
        public void start_Browser()
        {
            // Local Selenium WebDriver
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            base_url = ConfigurationManager.AppSettings["baseUrl"];
        }

        [Test]
        public void test_login()
        {
            driver.Url = base_url + "/login";

            IWebElement txtUserName = driver.FindElement( By.CssSelector( "[id$='_tbUserName']" ) );
            txtUserName.SendKeys( ConfigurationManager.AppSettings["adminUsername"] );

            IWebElement txtPassword = driver.FindElement( By.CssSelector( "[id$='_tbPassword']" ) );
            txtPassword.SendKeys( ConfigurationManager.AppSettings["adminPassword"] );

            IWebElement btnLogin = driver.FindElement( By.CssSelector( "[id$='_btnLogin']" ) );
            btnLogin.Click();

            System.Threading.Thread.Sleep( 2000 );

            driver.Navigate().GoToUrl( base_url + "/admin" );

            Assert.AreEqual( "Home | Rock RMS", driver.Title );
        }

        [Test]
        public void test_viewProfile()
        {
            driver.Url = base_url + "/Person/1";
            System.Threading.Thread.Sleep( 2000 );

            Assert.AreEqual( "Admin Admin | Rock RMS", driver.Title );
        }

        [OneTimeTearDown]
        public void close_Browser()
        {
            driver.Quit();
        }
    }
}