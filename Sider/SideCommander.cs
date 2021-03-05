using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Sider.Models;
using System.Threading;
using System.Diagnostics;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Sider
{
    public class SideRunner
    {
        IWebDriver driver;
        Project project;

        public TimeSpan ImplicitWait { get; set; } = TimeSpan.FromSeconds(10);

        public Project Project { get => this.project; set => this.project = value; }

        public SideRunner(IWebDriver driver, Project project)
        {
            this.driver = driver ?? throw new ArgumentNullException();
            this.Project = project ?? throw new ArgumentNullException();
        }

        public SideRunner(IWebDriver driver, string json)
        {
            this.driver = driver ?? throw new ArgumentNullException();
            this.Project = JsonConvert.DeserializeObject<Project>(json);

            var cmd = new OpenQA.Selenium.Remote.Command("","");
        }

        public void ExecuteTest(string testName)
        {
            var test = this.Project?.Tests?.First(t => t.Name == testName) ?? throw new InvalidOperationException();
            this.ExecuteTest(test);
        }

        private void ExecuteTest(Test test)
        {
            driver.Manage().Timeouts().ImplicitWait = this.ImplicitWait;

            foreach (var command in test.Commands)
            {
                this.ExecuteCommand(command);
            }
        }

        // https://github.com/SeleniumHQ/selenium/blob/master/java/server/src/org/openqa/selenium/server/htmlrunner/NonReflectiveSteps.java
        // https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs

        // ^\s*value\s*=\s*value\.replace\((".+")\s*,\s*(Keys\..)(.*)\)\s*;
        // {$1, $2\L($3)},

        // _(?!.+")(.)
        // \u$1

        // \.Numpad
        // \.NumberPad

        // etc
        // [Keys] enum does not support all keys ?
        protected Dictionary<string, string> keyMap = new Dictionary<string, string>
        {
            {"${KEY_ALT}", Keys.Alt},
            {"${KEY_CONTROL}", Keys.Control},
            {"${KEY_CTRL}", Keys.Control},
            {"${KEY_META}", Keys.Meta},
            {"${KEY_COMMAND}", Keys.Command},
            {"${KEY_SHIFT}", Keys.Shift},
            {"${KEY_BACKSPACE}", Keys.Backspace},
            {"${KEY_BKSP}", Keys.Backspace},
            {"${KEY_DELETE}", Keys.Delete},
            {"${KEY_DEL}", Keys.Delete},
            {"${KEY_ENTER}", Keys.Enter},
            {"${KEY_EQUALS}", Keys.Equal},
            {"${KEY_ESCAPE}", Keys.Escape},
            {"${KEY_ESC}", Keys.Escape},
            {"${KEY_INSERT}", Keys.Insert},
            {"${KEY_INS}", Keys.Insert},
            {"${KEY_PAUSE}", Keys.Pause},
            {"${KEY_SEMICOLON}", Keys.Semicolon},
            {"${KEY_SPACE}", Keys.Space},
            {"${KEY_TAB}", Keys.Tab},
            {"${KEY_LEFT}", Keys.Left},
            {"${KEY_UP}", Keys.Up},
            {"${KEY_RIGHT}", Keys.Right},
            {"${KEY_DOWN}", Keys.Down},
            {"${KEY_PAGE_UP}", Keys.PageUp},
            {"${KEY_PGUP}", Keys.PageUp},
            {"${KEY_PAGE_DOWN}", Keys.PageDown},
            {"${KEY_PGDN}", Keys.PageDown},
            {"${KEY_END}", Keys.End},
            {"${KEY_HOME}", Keys.Home},
            {"${KEY_NUMPAD0}", Keys.NumberPad0},
            {"${KEY_N0}", Keys.NumberPad0},
            {"${KEY_NUMPAD1}", Keys.NumberPad1},
            {"${KEY_N1}", Keys.NumberPad1},
            {"${KEY_NUMPAD2}", Keys.NumberPad2},
            {"${KEY_N2}", Keys.NumberPad2},
            {"${KEY_NUMPAD3}", Keys.NumberPad3},
            {"${KEY_N3}", Keys.NumberPad3},
            {"${KEY_NUMPAD4}", Keys.NumberPad4},
            {"${KEY_N4}", Keys.NumberPad4},
            {"${KEY_NUMPAD5}", Keys.NumberPad5},
            {"${KEY_N5}", Keys.NumberPad5},
            {"${KEY_NUMPAD6}", Keys.NumberPad6},
            {"${KEY_N6}", Keys.NumberPad6},
            {"${KEY_NUMPAD7}", Keys.NumberPad7},
            {"${KEY_N7}", Keys.NumberPad7},
            {"${KEY_NUMPAD8}", Keys.NumberPad8},
            {"${KEY_N8}", Keys.NumberPad8},
            {"${KEY_NUMPAD9}", Keys.NumberPad9},
            {"${KEY_N9}", Keys.NumberPad9},
            {"${KEY_ADD}", Keys.Add},
            {"${KEY_NUM_PLUS}", Keys.Add},
            {"${KEY_DECIMAL}", Keys.Decimal},
            {"${KEY_NUM_PERIOD}", Keys.Decimal},
            {"${KEY_DIVIDE}", Keys.Divide},
            {"${KEY_NUM_DIVISION}", Keys.Divide},
            {"${KEY_MULTIPLY}", Keys.Multiply},
            {"${KEY_NUM_MULTIPLY}", Keys.Multiply},
            {"${KEY_SEPARATOR}", Keys.Separator},
            {"${KEY_SEP}", Keys.Separator},
            {"${KEY_SUBTRACT}", Keys.Subtract},
            {"${KEY_NUM_MINUS}", Keys.Subtract},
            {"${KEY_F1}", Keys.F1},
            {"${KEY_F2}", Keys.F2},
            {"${KEY_F3}", Keys.F3},
            {"${KEY_F4}", Keys.F4},
            {"${KEY_F5}", Keys.F5},
            {"${KEY_F6}", Keys.F6},
            {"${KEY_F7}", Keys.F7},
            {"${KEY_F8}", Keys.F8},
            {"${KEY_F9}", Keys.F9},
            {"${KEY_F10}", Keys.F10},
            {"${KEY_F11}", Keys.F11},
            {"${KEY_F12}", Keys.F12},
        };

        public void ExecuteCommand(Command command)
        {
            // https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js
            // ^\s*([^:]+):\s*([^,]+),
            // case "$1": $2\(command\); break;

            switch (command.CommandName)
            {
                case "open": emitOpen(command); break;
                case "click": emitClick(command); break;
                case "clickAt": emitClick(command); break;
                case "check": emitCheck(command); break;
                case "uncheck": emitUncheck(command); break;
                case "debugger": emitDebugger(command); break;
                case "doubleClick": emitDoubleClick(command); break;
                case "doubleClickAt": emitDoubleClick(command); break;
                case "dragAndDropToObject": emitDragAndDrop(command); break;
                case "type": emitType(command); break;
                case "sendKeys": emitSendKeys(command); break;
                case "echo": emitEcho(command); break;
                case "run": emitRun(command); break;
                case "runScript": emitRunScript(command); break;
                case "executeScript": emitExecuteScript(command); break;
                case "executeAsyncScript": emitExecuteAsyncScript(command); break;
                case "pause": emitPause(command); break;
                case "verifyChecked": emitVerifyChecked(command); break;
                case "verifyNotChecked": emitVerifyNotChecked(command); break;
                case "verifyEditable": emitVerifyEditable(command); break;
                case "verifyNotEditable": emitVerifyNotEditable(command); break;
                case "verifyElementPresent": emitVerifyElementPresent(command); break;
                case "verifyElementNotPresent": emitVerifyElementNotPresent(command); break;
                case "verifySelectedValue": emitVerifySelectedValue(command); break;
                case "verifyNotSelectedValue": emitVerifyNotSelectedValue(command); break;
                case "verifyValue": emitVerifyValue(command); break;
                case "verifyText": emitVerifyText(command); break;
                case "verifyTitle": emitVerifyTitle(command); break;
                case "verifyNotText": emitVerifyNotText(command); break;
                case "verifySelectedLabel": emitVerifySelectedLabel(command); break;
                case "assertChecked": emitVerifyChecked(command); break;
                case "assertNotChecked": emitVerifyNotChecked(command); break;
                case "assertEditable": emitVerifyEditable(command); break;
                case "assertNotEditable": emitVerifyNotEditable(command); break;
                case "assertElementPresent": emitVerifyElementPresent(command); break;
                case "assertElementNotPresent": emitVerifyElementNotPresent(command); break;
                case "assertSelectedValue": emitVerifySelectedValue(command); break;
                case "assertNotSelectedValue": emitVerifyNotSelectedValue(command); break;
                case "assertValue": emitVerifyValue(command); break;
                case "assertText": emitVerifyText(command); break;
                case "assertTitle": emitVerifyTitle(command); break;
                case "assertSelectedLabel": emitVerifySelectedLabel(command); break;
                case "store": emitStore(command); break;
                case "storeText": emitStoreText(command); break;
                case "storeValue": emitStoreValue(command); break;
                case "storeTitle": emitStoreTitle(command); break;
                case "storeWindowHandle": emitStoreWindowHandle(command); break;
                case "storeXpathCount": emitStoreXpathCount(command); break;
                case "storeAttribute": emitStoreAttribute(command); break;
                case "select": emitSelect(command); break;
                case "addSelection": emitSelect(command); break;
                case "removeSelection": emitSelect(command); break;
                case "selectFrame": emitSelectFrame(command); break;
                case "selectWindow": emitSelectWindow(command); break;
                case "close": emitClose(command); break;
                case "mouseDown": emitMouseDown(command); break;
                case "mouseDownAt": emitMouseDown(command); break;
                case "mouseUp": emitMouseUp(command); break;
                case "mouseUpAt": emitMouseUp(command); break;
                case "mouseMove": emitMouseMove(command); break;
                case "mouseMoveAt": emitMouseMove(command); break;
                case "mouseOver": emitMouseMove(command); break;
                case "mouseOut": emitMouseOut(command); break;
                case "assertAlert": emitAssertAlertAndAccept(command); break;
                case "assertNotText": emitVerifyNotText(command); break;
                case "assertPrompt": emitAssertAlert(command); break;
                case "assertConfirmation": emitAssertAlert(command); break;
                case "webdriverAnswerOnVisiblePrompt": emitAnswerOnNextPrompt(command); break;
                case "webdriverChooseOkOnVisibleConfirmation": emitChooseOkOnNextConfirmation(command); break;
                case "webdriverChooseCancelOnVisibleConfirmation": emitChooseCancelOnNextConfirmation(command); break;
                case "webdriverChooseCancelOnVisiblePrompt": emitChooseCancelOnNextConfirmation(command); break;
                case "editContent": emitEditContent(command); break;
                case "submit": emitSubmit(command); break;
                case "answerOnNextPrompt": skip(command); break;
                case "chooseCancelOnNextConfirmation": skip(command); break;
                case "chooseCancelOnNextPrompt": skip(command); break;
                case "chooseOkOnNextConfirmation": skip(command); break;
                case "setSpeed": emitSetSpeed(command); break;
                case "setWindowSize": emitSetWindowSize(command); break;
                case "do": emitControlFlowDo(command); break;
                case "else": emitControlFlowElse(command); break;
                case "elseIf": emitControlFlowElseIf(command); break;
                case "end": emitControlFlowEnd(command); break;
                case "if": emitControlFlowIf(command); break;
                case "repeatIf": emitControlFlowRepeatIf(command); break;
                case "times": emitControlFlowTimes(command); break;
                case "while": emitControlFlowWhile(command); break;
                case "assert": emitAssert(command); break;
                case "verify": emitAssert(command); break;
                case "waitForElementPresent": emitWaitForElementPresent(command); break;
                case "waitForElementNotPresent": emitWaitForElementNotPresent(command); break;
                case "waitForElementVisible": emitWaitForElementVisible(command); break;
                case "waitForElementNotVisible": emitWaitForElementNotVisible(command); break;
                case "waitForElementEditable": emitWaitForElementEditable(command); break;
                case "waitForElementNotEditable": emitWaitForElementNotEditable(command); break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void emitWaitForElementNotEditable(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitWaitForElementEditable(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitWaitForElementNotVisible(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitWaitForElementVisible(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitWaitForElementNotPresent(Command command)
        {
            var w = new WebDriverWait(this.driver, ImplicitWait);
            w.Until(ExpectedRunnerConditions.ElementNotExists(command.TargetToLocator()));
        }

        private void emitWaitForElementPresent(Command command)
        {
            var w = new WebDriverWait(this.driver, ImplicitWait);
            w.Until(ExpectedConditions.ElementExists(command.TargetToLocator()));
        }

        private void emitAssert(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowWhile(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowTimes(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowRepeatIf(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowIf(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowEnd(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowElseIf(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowElse(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitControlFlowDo(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitSetWindowSize(Command command)
        {
            var sizeStrings = command.Target.Split('x');
            this.driver.Manage().Window.Size = new Size(int.Parse(sizeStrings[0]), int.Parse(sizeStrings[1]));
        }

        private void emitSetSpeed(Command command)
        {
            throw new NotImplementedException();
        }

        private void skip(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitSubmit(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitEditContent(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitChooseCancelOnNextConfirmation(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitChooseOkOnNextConfirmation(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitAnswerOnNextPrompt(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitAssertAlert(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitAssertAlertAndAccept(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitMouseOut(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitMouseMove(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitMouseUp(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitMouseDown(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitClose(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitSelectWindow(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitSelectFrame(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitSelect(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreAttribute(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreXpathCount(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreWindowHandle(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreTitle(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreValue(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStoreText(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitStore(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifySelectedLabel(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyNotText(Command command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L514
        /// </summary>
        /// <param name="command"></param>
        private void emitVerifyTitle(Command command)
        {
            var title = driver.Title;
            SeleniumAssert.AreSame(command.Target, title);
        }

        private void emitVerifyText(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyValue(Command command)
        {
            var el = this.driver.FindElement(command.TargetToLocator());
            SeleniumAssert.AreSame(command.Value, el.GetAttribute("value"));
        }

        private void emitVerifyNotSelectedValue(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifySelectedValue(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyElementNotPresent(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyElementPresent(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyNotEditable(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyEditable(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyNotChecked(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitVerifyChecked(Command command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L392
        /// </summary>
        /// <param name="command"></param>
        private void emitPause(Command command)
        {
            Thread.Sleep(int.Parse(command.Target) * 1000);
        }

        private void emitExecuteAsyncScript(Command command)
        {
            throw new NotImplementedException();
        }

        private void emitExecuteScript(Command command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L365
        /// </summary>
        /// <param name="command"></param>
        private void emitRunScript(Command command)
        {
            driver.ExecuteJavaScript(command.Target);
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L359
        /// </summary>
        /// <param name="command"></param>
        private void emitRun(Command command)
        {
            this.ExecuteTest(command.Target);
        }

        private void emitEcho(Command command)
        {
            // https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L335
            
            driver.ExecuteJavaScript($"console.log('${command.Target}');");
            // ?
        }

        private void emitSendKeys(Command command)
        {
            // https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L321

            var elem = this.selectElement(command.Target);
            var key = command.Value;
            if (this.keyMap.Keys.Contains(key))
                key = this.keyMap[key];
            elem.SendKeys(key);
        }

        private void emitType(Command command)
        {
            // https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L311

            var elem = this.selectElement(command.Target);
            elem.Clear();
            elem.SendKeys(command.Value);
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L297
        /// https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriverbackedselenium/Internal/SeleniumEmulation/DragAndDrop.cs#L49
        /// </summary>
        /// <param name="command"></param>
        private void emitDragAndDrop(Command command)
        {
            var dropSource = this.selectElement(command.Target);
            var dropTarget = this.selectElement(command.Value);

            var action = new Actions(driver);
            action.DragAndDrop(dropSource, dropTarget)?.Perform();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L287
        /// </summary>
        /// <param name="command"></param>
        private void emitDoubleClick(Command command)
        {
            var elem = this.selectElement(command.Target);
 
            var action = new Actions(driver);
            action.DoubleClick(elem)?.Perform();
        }

        private void emitDebugger(Command command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L349
        /// </summary>
        /// <param name="command"></param>
        private void emitUncheck(Command command)
        {
            this.emitCheckTest(command, elem => elem.GetAttribute("checked")?.ToLower() == "true");
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L339
        /// </summary>
        /// <param name="command"></param>
        private void emitCheck(Command command)
        {
            this.emitCheckTest(command, elem => elem.GetAttribute("checked")?.ToLower() != "true");
        }

        private void emitCheckTest(Command command, Func<IWebElement,bool> test)
        {
            var elem = this.selectElement(command.Target);

            if (elem.TagName.ToLower() == "input" &&
                elem.GetAttribute("type").ToLower() == "checkbox")
            {
                var c = elem.GetAttribute("checked");
                if (test(elem))
                {
                    elem.Click();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L273
        /// </summary>
        /// <param name="command"></param>
        private void emitClick(Command command)
        {
            var elem = this.selectElement(command.Target);
            elem.Click();
        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L266
        /// </summary>
        /// <param name="command"></param>
        private void emitOpen(Command command)
        {
            var url = command.Target;
            if (!Regex.IsMatch(url, @"^(file|http|https):\/\/"))
                url = Project.Url + command.Target;
            driver.Navigate().GoToUrl(url);

        }

        /// <summary>
        /// https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/location.js
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected IWebElement selectElement(string target)
        {
            var targets = target.Split('=');
            switch (targets[0])
            {
                case "css":
                    return driver.FindElement(By.CssSelector(targets[1]));
                case "link":
                case "linkText":
                    return this.driver.FindElement(By.LinkText(targets[1]));
                case "id":
                    return this.driver.FindElement(By.Id(targets[1]));
                case "name":
                    return this.driver.FindElement(By.Name(targets[1]));
                case "partialLinkText":
                    return this.driver.FindElement(By.PartialLinkText(targets[1]));
                case "xpath":
                    return this.driver.FindElement(By.XPath(targets[1]));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
