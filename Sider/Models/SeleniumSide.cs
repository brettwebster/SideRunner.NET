using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenQA.Selenium;

/// <summary>
/// base on 
/// http://json2csharp.com/
/// https://quicktype.io/
/// </summary>
namespace Sider.Models
{
    public partial class Project
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }

        [JsonProperty("suites")]
        public List<Suite> Suites { get; set; }

        [JsonProperty("urls")]
        public List<Uri> Urls { get; set; }

        [JsonProperty("plugins")]
        public List<Plugin> Plugins { get; set; }
    }

    public partial class Plugin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Suite
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("persistSession")]
        public bool PersistSession { get; set; }

        [JsonProperty("parallel")]
        public bool Parallel { get; set; }

        [JsonProperty("timeout")]
        public long Timeout { get; set; }

        [JsonProperty("tests")]
        public List<Guid> Tests { get; set; }
    }

    public partial class Test
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("commands")]
        public List<Command> Commands { get; set; }
    }

    public partial class Command
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("command")]
        public string CommandName { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("targets")]
        public List<List<string>> Targets { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("opensWindow", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OpensWindow { get; set; }

        [JsonProperty("windowHandleName", NullValueHandling = NullValueHandling.Ignore)]
        public string WindowHandleName { get; set; }

        [JsonProperty("windowTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public long? WindowTimeout { get; set; }

        public By TargetToLocator()
        {
            if (string.IsNullOrEmpty(Target)) { return null; }

            var targetType = Target.Substring(0, Target.IndexOf('=', 0));
            var targetValue = Target.Substring(targetType.Length+1);

            By selector = null;

            switch (targetType)
            {
                case "linkText":
                    selector = By.LinkText(targetValue);
                    break;
                case "id":
                    selector = By.Id(targetValue);
                    break;
                case "css":
                    selector = By.CssSelector(targetValue);
                    break;
            }

            return selector;
        }
    }
}
