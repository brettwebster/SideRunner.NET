using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sider
{
    /// <summary>
    /// ExpectedCondition class is missing some conditions utilized by SIDE files.
    /// This class provides a home for those extra conditions to live - 
    /// 
    /// TODO: Push new functions to SeleniumExtras project
    /// </summary>
    public sealed class ExpectedRunnerConditions
    {
        private ExpectedRunnerConditions() { }


        /// <summary>
        /// Used to wait for an element to no longer be present.
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public static Func<IWebDriver,bool> ElementNotExists(By locator)
        {
            return (driver) => { return driver.FindElement(locator) == null; };
        }
    }

}
