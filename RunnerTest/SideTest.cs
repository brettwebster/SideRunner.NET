using NUnit.Framework;
using System.IO;
using OpenQA.Selenium.Chrome;
using Sider;
using System;

using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tests
{
    public class Tests
    {
        string basePath = "";
        string dir = "testset";
        HttpListener listener = null;
        bool listening = false;

        [SetUp]
        public void Setup()
        {
            this.basePath = TestContext.CurrentContext.TestDirectory;

            for (var i = 0; i < 3; i++)
                this.basePath = Path.GetDirectoryName(this.basePath);

            this.basePath = Path.Join(this.basePath, this.dir);
           
        }

        /// <summary>
        /// This method provides a common set of functions that are performed after each test method.
        /// <seealso cref="https://docs.nunit.org/articles/nunit/writing-tests/attributes/teardown.html"/>
        /// </summary>
        [TearDown]
        public void CloseListener()
        {
            // close any rogue chromedriver's remaining from a failed test
            var drivers = Process.GetProcessesByName("chromedriver");
            drivers.AsParallel().ForAll(p =>
            {
                p.Kill();
            });

            if (this.listening)
            {
                this.listening = false;
                this.listener?.Close();
                this.listener = null;
            }
        }

        public async Task StartHttpServer(string filePath)
        {
            this.listener = new HttpListener();

            this.listening = true;
            this.listener.Prefixes.Add("http://localhost:8080/");

            this.listener.Start();

            while (this.listening)
            {
                var context = await this.listener.GetContextAsync();
                var request = context.Request;

                var response = context.Response;
                response.ContentType = "text/html";
                response.StatusCode = (int)HttpStatusCode.OK;

                var buffer = File.ReadAllBytes(filePath);

                response.ContentLength64 = buffer.Length;

                using (var output = response.OutputStream)
                {
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }
                response.Close();
            }
        }

        [Test]
        public void CheckUncheck()
        {
            this.Run(this.dir, "check-uncheck");
        }

        [Test]
        public void Click()
        {
            this.Run(this.dir, "click");
        }

        [Test]
        public void DoubleClick()
        {
            this.Run(this.dir, "double-click");
        }

        [Test]
        public void Type()
        {
            this.Run(this.dir, "type");
        }

        public void Run(string basePath, string testName)
        {
            var sidePath = Path.Join(this.basePath, testName + Path.DirectorySeparatorChar + testName + ".side");
            var htmlPath = Path.Join(this.basePath, testName + Path.DirectorySeparatorChar + testName + ".html");

            var options = new ChromeOptions();
            options.AddArgument("--headless");

            using (var driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory, options))
            {
                var sider = new SideRunner(driver, File.ReadAllText(sidePath, System.Text.Encoding.UTF8));
                var task = this.StartHttpServer(htmlPath);

                sider.ExecuteTest(testName);

                driver.FindElementById("validate").Click();
                var result = driver.FindElementById("result").Text;
                Assert.AreEqual("OK", result);
            }
        }
    }
}