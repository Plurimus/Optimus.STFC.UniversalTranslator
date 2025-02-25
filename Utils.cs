using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Optimus.STFC.UniversalTranslator
{
    public static class Utils
    {
        public enum TranslatorDeeplKeys : int
        {
            [Description("Bulgarian")]
            BG,
            [Description("Czech")]
            CS,
            [Description("Danish")]
            DA,
            [Description("German")]
            DE,
            [Description("Greek")]
            EL,
            [Description("English (unspecified variant for backward compatibility; please select EN-GB or EN-US instead)")]
            EN,
            [Description("English(British)")]
            EN_GB,
            [Description("English(American)")]
            EN_US,
            [Description("Spanish")]
            ES,
            [Description("Estonian")]
            ET,
            [Description("Finnish")]
            FI,
            [Description("French")]
            FR,
            [Description("Hungarian")]
            HU,
            [Description("Indonesian")]
            ID,
            [Description("Italian")]
            IT,
            [Description("Japanese")]
            JA,
            [Description("Korean")]
            KO,
            [Description("Lithuanian")]
            LT,
            [Description("Latvian")]
            LV,
            [Description("Norwegian(Bokmål)")]
            NB,
            [Description("Dutch")]
            NL,
            [Description("Polish")]
            PL,
            [Description("Portuguese(unspecified variant for backward compatibility; please select PT-BR or PT-PT instead)")]
            PT,
            [Description("Portuguese(Brazilian)")]
            PT_BR,
            [Description("Portuguese(all Portuguese varieties excluding Brazilian Portuguese)")]
            PT_PT,
            [Description("Romanian")]
            RO,
            [Description("Russian")]
            RU,
            [Description("Slovak")]
            SK,
            [Description("Slovenian")]
            SL,
            [Description("Swedish")]
            SV,
            [Description("Turkish")]
            TR,
            [Description("Ukrainian")]
            UK,
            [Description("Chinese(simplified)")]
            ZH 

    }
        public static KeyValuePair<string, string> TranslateDeepl(String text, Utils.TranslatorDeeplKeys toLanguage, ManualLogSource log, bool writelog, string apiKey)
        {
            try
            {
                if (writelog) log.LogInfo($"\t\t\t\t Starting method TranslateDeepl()");
                if (writelog) log.LogInfo($"\t\t\t\t data to translate: {text}");
                if (writelog) log.LogInfo($"\t\t\t\t language to translate: [{toLanguage.ToString()}]");


                //var url = $"https://api.deepl.com/v2/translate";      // api Pro
                var url = $"https://api-free.deepl.com/v2/translate";   // api Free
                if (writelog) log.LogInfo($"\t\t\t\t set url [{url}]");
                WebRequest request = WebRequest.Create(url);
                if (writelog) log.LogInfo($"\t\t\t\t created WebRequest [{request}]");

                request.Method = "POST";
                if (writelog) log.LogInfo($"\t\t\t\t set method [{request.Method}]");

                request.Headers.Clear();
                if (writelog) log.LogInfo($"\t\t\t\t cleared request headers, count = [{request.Headers.Count}]");


                //pls don't use it. it is from a free account with low limits, just for STFC translator
                string deeplApiKey = "2a1c9cf5-0393-46ca-98a1-fb35c7b72fd1:fx"; 
                if (apiKey is not null && apiKey != "" && apiKey.Length > 10)
                {
                    deeplApiKey = apiKey;
                }
                request.Headers["Authorization"] = $"DeepL-Auth-Key {deeplApiKey}";
                if (writelog) log.LogInfo($"\t\t\t\t set Authorization header = [{request.Headers["Authorization"]}]");

                ((HttpWebRequest)request).UserAgent = $"{MyPluginInfo.PLUGIN_NAME}|v{MyPluginInfo.PLUGIN_VERSION}";
                if (writelog) log.LogInfo($"\t\t\t\t set User-Agent = [{((HttpWebRequest)request).UserAgent}]");

                request.ContentType = "application/json";
                if (writelog) log.LogInfo($"\t\t\t\t set Content-Type header = [{request.Headers["Content-Type"]}]");

                string postData = $"{{\"text\":[\"{text.Replace("\"","\\\"")}\"],\"target_lang\":\"{toLanguage}\"}}";
                if (writelog) log.LogInfo($"\t\t\t\t set postData = [{postData}]");

                byte[] byte1 = Encoding.UTF8.GetBytes(postData);
                if (writelog) log.LogInfo($"\t\t\t\t get postData bytes length = {byte1.Length}");


                Stream newStream = request.GetRequestStream();
                if (writelog) log.LogInfo($"\t\t\t\t created GetRequestStream()");

                newStream.Write(byte1, 0, byte1.Length);
                // Read response
                WebResponse response = request.GetResponse();
                var translatedText = "";
                var detected_source_language = "";
                using (Stream stream = response.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[0x1000];
                        int bytes;
                        while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytes);
                        }
                        byte[] arr = memoryStream.ToArray();
                        if (writelog) log.LogInfo($"\t\t\t\t response length: {arr.Length}");

                        var jsonResponse = Encoding.UTF8.GetString(arr);
                        if (writelog) log.LogInfo($"\t\t\t\t response string: {jsonResponse}");
                        
                        JObject json = JObject.Parse(jsonResponse);
                        translatedText = json["translations"][0]["text"].ToString();
                        if (writelog) log.LogInfo($"\t\t\t\t translatedText: {translatedText}");
                        
                        detected_source_language = json["translations"][0]["detected_source_language"].ToString();
                        if (writelog) log.LogInfo($"\t\t\t\t detected_source_language: {detected_source_language}");
                    }
                }
                return new KeyValuePair<string, string>(detected_source_language, translatedText);
            }
            catch (System.Exception e)
            {
                if (writelog) log.LogInfo($"ERROR in TranslateDeepl(): {e.Message}\r\n {e.StackTrace}");
                throw;
            }
        }

    }


    public static class EnumExtensions
    {
        private static readonly
            ConcurrentDictionary<string, string> DisplayNameCache = new ConcurrentDictionary<string, string>();

        public static string ToDescription<T>(this T value)
        {
            var key = $"{value.GetType().FullName}.{value}";

            var displayName = DisplayNameCache.GetOrAdd(key, x =>
            {
                var name = (DescriptionAttribute[])value
                    .GetType()
                    .GetTypeInfo()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);

                return name.Length > 0 ? name[0].Description : value.ToString();
            });

            return displayName;
        }

        public static T GetValueFromDescription<T>(string description) where T : System.Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (System.Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new System.ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}
