using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using Digit.Client.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Digit.Client.Tooltip;
using Optimus.STFC.UniversalTranslator;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using UnhollowerBaseLib;
using static Optimus.STFC.UniversalTranslator.Utils;
using Digit.PrimeServer.Core;
using Digit.PrimePlatform.Services;
using Digit.Networking.Core;
using Utils = Optimus.STFC.UniversalTranslator.Utils;

namespace STFCUniversalTranslator
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class UniversalTranslatorPlugin : BasePlugin
    {
        #region[Declarations]

        public const string
            PROJECT = "STFC",
            MODNAME = "UniversalTranslator",
            AUTHOR = "Optimus",
            GUID = AUTHOR + "." + PROJECT + "." + MODNAME,
            VERSION = MyPluginInfo.PLUGIN_VERSION;   

        #endregion


        internal static new ManualLogSource Log;

        private static ConfigEntry<string> configChatTranslateToLanguage;

        private static ConfigEntry<bool> configTranslateOutgoing; 
        private static ConfigEntry<string> configTranslateOutgoingToLanguage;

        private static ConfigEntry<string> configDeeplApiKey;
        
        private static ConfigEntry<bool> configVerbose;    

        public override void Load()
        {
            // Plugin startup logic
            UniversalTranslatorPlugin.Log = base.Log;



            //configChatTranslateToLanguage = Config.Bind("General",
            //        "What language chat messages will be translated to",
            //        TranslatorDeeplKeys.RU,
            //        "На какой язык переводить сообщения");

            configChatTranslateToLanguage = Config.Bind(new ConfigDefinition("General", "What language chat messages will be translated to"),
            TranslatorDeeplKeys.RU.ToDescription(),
            new ConfigDescription("На какой язык переводить сообщения",
                new AcceptableValueList<string>(
                        TranslatorDeeplKeys.BG.ToDescription(),
                        TranslatorDeeplKeys.CS.ToDescription(),
                        TranslatorDeeplKeys.DA.ToDescription(),
                        TranslatorDeeplKeys.DE.ToDescription(),
                        TranslatorDeeplKeys.EL.ToDescription(),
                        TranslatorDeeplKeys.EN.ToDescription(),
                        //TranslatorDeeplKeys.EN_GB.ToDescription(),
                        //TranslatorDeeplKeys.EN_US.ToDescription(),
                        TranslatorDeeplKeys.ES.ToDescription(),
                        TranslatorDeeplKeys.ET.ToDescription(),
                        TranslatorDeeplKeys.FI.ToDescription(),
                        TranslatorDeeplKeys.FR.ToDescription(),
                        TranslatorDeeplKeys.HU.ToDescription(),
                        TranslatorDeeplKeys.ID.ToDescription(),
                        TranslatorDeeplKeys.IT.ToDescription(),
                        TranslatorDeeplKeys.JA.ToDescription(),
                        TranslatorDeeplKeys.KO.ToDescription(),
                        TranslatorDeeplKeys.LT.ToDescription(),
                        TranslatorDeeplKeys.LV.ToDescription(),
                        TranslatorDeeplKeys.NB.ToDescription(),
                        TranslatorDeeplKeys.NL.ToDescription(),
                        TranslatorDeeplKeys.PL.ToDescription(),
                        TranslatorDeeplKeys.PT.ToDescription(),
                        //TranslatorDeeplKeys.PT_BR.ToDescription(),
                        //TranslatorDeeplKeys.PT_PT.ToDescription(),
                        TranslatorDeeplKeys.RO.ToDescription(),
                        TranslatorDeeplKeys.RU.ToDescription(),
                        TranslatorDeeplKeys.SK.ToDescription(),
                        TranslatorDeeplKeys.SL.ToDescription(),
                        TranslatorDeeplKeys.SV.ToDescription(),
                        TranslatorDeeplKeys.TR.ToDescription(),
                        TranslatorDeeplKeys.UK.ToDescription(),
                        TranslatorDeeplKeys.ZH.ToDescription()
                                                )
                                   )
                                                    );

            configTranslateOutgoing = Config.Bind("General",
                    "Translate all outgoing massages",  // The key of the configuration option in the configuration file
                    false, // The default value
                    "Переводить исходящие сообщения");

            configTranslateOutgoingToLanguage = Config.Bind(new ConfigDefinition("General", "What language outgoing messages will be translated to"),
                    TranslatorDeeplKeys.RU.ToDescription(),
                    new ConfigDescription("На какой язык переводить отправляемые сообщения",
                        new AcceptableValueList<string>(
                                TranslatorDeeplKeys.BG.ToDescription(),
                                TranslatorDeeplKeys.CS.ToDescription(),
                                TranslatorDeeplKeys.DA.ToDescription(),
                                TranslatorDeeplKeys.DE.ToDescription(),
                                TranslatorDeeplKeys.EL.ToDescription(),
                                TranslatorDeeplKeys.EN.ToDescription(),
                                //TranslatorDeeplKeys.EN_GB.ToDescription(),
                                //TranslatorDeeplKeys.EN_US.ToDescription(),
                                TranslatorDeeplKeys.ES.ToDescription(),
                                TranslatorDeeplKeys.ET.ToDescription(),
                                TranslatorDeeplKeys.FI.ToDescription(),
                                TranslatorDeeplKeys.FR.ToDescription(),
                                TranslatorDeeplKeys.HU.ToDescription(),
                                TranslatorDeeplKeys.ID.ToDescription(),
                                TranslatorDeeplKeys.IT.ToDescription(),
                                TranslatorDeeplKeys.JA.ToDescription(),
                                TranslatorDeeplKeys.KO.ToDescription(),
                                TranslatorDeeplKeys.LT.ToDescription(),
                                TranslatorDeeplKeys.LV.ToDescription(),
                                TranslatorDeeplKeys.NB.ToDescription(),
                                TranslatorDeeplKeys.NL.ToDescription(),
                                TranslatorDeeplKeys.PL.ToDescription(),
                                TranslatorDeeplKeys.PT.ToDescription(),
                                //TranslatorDeeplKeys.PT_BR.ToDescription(),
                                //TranslatorDeeplKeys.PT_PT.ToDescription(),
                                TranslatorDeeplKeys.RO.ToDescription(),
                                TranslatorDeeplKeys.RU.ToDescription(),
                                TranslatorDeeplKeys.SK.ToDescription(),
                                TranslatorDeeplKeys.SL.ToDescription(),
                                TranslatorDeeplKeys.SV.ToDescription(),
                                TranslatorDeeplKeys.TR.ToDescription(),
                                TranslatorDeeplKeys.UK.ToDescription(),
                                TranslatorDeeplKeys.ZH.ToDescription()
                                                        )
                                            )
                                                            );


            configDeeplApiKey = Config.Bind("General",
                    "Deepl.com API key (if you want your own stable translator). Leave it empty for default.",
                    "",
                    "Ключ API для сервиса Deepl.com (если хотите собственный стабильный переводчик). Оставить поле пустым если не нужно");
            configVerbose = Config.Bind("General",      // The section under which the option is shown
                    "Verbose",  // The key of the configuration option in the configuration file
                    false, // The default value
                    "write more logs");

            Harmony.CreateAndPatchAll(typeof(UniversalTranslatorPlugin));
            Log.LogInfo($"Plugin {GUID} is loaded!");

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Translate Button Rendering

        internal static bool translatorButtonRendered = false;

        [HarmonyPatch(typeof(CanvasController), "LateUpdate")]
        [HarmonyPostfix]
        public static void CanvasController_Update(CanvasController __instance)
        {
            if (__instance != null)
            {
                if (__instance.name == "ToolTip_Canvas")                    
                {
                    Transform translator = __instance.transform.Find("Tooltip/InfoContainer/Buttons/GenericButton_Action(Translator)");
                    //if (configVerbose.Value) Log.LogInfo($"Try find \"Tooltip/InfoContainer/Buttons/GenericButton_Action(Translator)\" {translator}");

                    Transform firstButton = __instance.transform.Find("Tooltip/InfoContainer/Buttons/GenericButton_Action(Clone)");
                    //if (configVerbose.Value) Log.LogInfo($"Try find \"Tooltip/InfoContainer/Buttons/GenericButton_Action(Clone)\" {firstButton}");

                    if (__instance.GetComponentInChildren<TooltipWidget>()?.Context?.TryCast<ChatLinkAndToolTipTrigger>()?.GetType() != null)
                    {
                        if (!translatorButtonRendered)
                        {
                            if (translator == null)
                            {
                                if (configVerbose.Value) Log.LogInfo($"translator button object is null");

                                try
                                {
                                    var buttons = __instance.transform.GetComponentsInChildren<Button>(true);
                                    if (configVerbose.Value) Log.LogInfo($"CanvasController.GetComponentsInChildren<Button>() count ={buttons?.Count}");

                                    if (buttons?.Count > 3)
                                    {
                                        if (configVerbose.Value) Log.LogInfo($"Instantiate clone of {firstButton}");

                                        translator = UnityEngine.Object.Instantiate(firstButton, firstButton.parent);
                                        translator.gameObject.name = "GenericButton_Action(Translator)";
                                        if (configVerbose.Value) Log.LogInfo($"Set name of {translator}");
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    if (configVerbose.Value) Log.LogInfo($"ERROR: {e.Message}");
                                    return;
                                }
                            }
                            translatorButtonRendered = true;
                        }
                        else
                        {
                            string textLabel = firstButton.Find("MainLabel").GetComponent<TextMeshProUGUI>()?.text;
                            string textLabelToSet = "TRANSLATE";
                            switch (textLabel)
                            {
                                case "PROFIL":
                                    textLabelToSet = "ÜBERSETZEN";
                                    break;
                                case "ПРОФИЛЬ":
                                    textLabelToSet = "ПЕРЕВЕСТИ";
                                    break;
                                default:
                                    break;
                            }
                            translator.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = textLabelToSet;
                        }
                    }
                    if (firstButton!=null)
                    {
                        //if (configVerbose.Value) Log.LogInfo($"CanvasController.GetComponentsInChildren<TooltipWidget>().Context ={__instance.transform.GetComponentInChildren<TooltipWidget>()?.Context?.TryCast<ChatLinkAndToolTipTrigger>()?.GetType()}");
                        translator?.gameObject.SetActive(__instance.transform.GetComponentInChildren<TooltipWidget>()?.Context?.TryCast<ChatLinkAndToolTipTrigger>()?.GetType() != null);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameServer), "Initialise")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void GetSessionID(string sessionId, string gameVersion, bool encryptRequests)
        {
            if (configVerbose.Value) Log.LogInfo($"GameServer:\n" +
                                                    $"\t\t\t\t sessionId =\t{sessionId}\n" +
                                                    $"\t\t\t\t gameVersion =\t{gameVersion}\n" +
                                                    $"\t\t\t\t encryptRequests =\t{encryptRequests}\n");

            translatorButtonRendered = false; // reset rendering of Translator Button
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [HarmonyPatch(typeof(Button), "Press")]
        [HarmonyPostfix]
        public static void Button_Press(Button __instance)
        {
            //Log.LogInfo($"\t\t\t Button.Press()");
            if (__instance != null)
            {
                if (__instance.name == "GenericButton_Action(Translator)")
                {
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Start translation");

                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t GetContext");
                    ChatLinkAndToolTipTrigger context = __instance.transform.GetComponentInParent<CanvasController>().Context.TryCast<ChatLinkAndToolTipTrigger>();
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t context.gameObject.name == {context.gameObject.name}");
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t context.gameObject.transform.FindChild(\"PlayerName\") == {context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName")}");
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t context.gameObject.transform.FindChild(\"PlayerName\").GetComponent<TextMeshProUGUI>().text == {context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}");
                    string textToTranslate = context._tmpText.text;
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t textToTranslate == {textToTranslate}");
                    Regex regex = new Regex("""(.*)(<link="BM">.*<\/link>)(.*)""");
                    MatchCollection matches = regex.Matches(textToTranslate);
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t regex matches == {matches.Count}");

                    string beforeLink, link, afterLink, lang = string.Empty;
                    var playerNameHeader = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t player name header == {playerNameHeader}");
                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t playerNameHeader.Contains(\"Universal Translator\") == {playerNameHeader.Contains("Universal Translator")}");
                    if (matches.Count == 1)
                    {
                        beforeLink = matches[0].Groups[1].Value;
                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t beforeLink text:  {beforeLink}");

                        link = matches[0].Groups[2].Value;
                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t link text:  {link}");

                        afterLink = matches[0].Groups[3].Value;
                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t afterLink text:  {afterLink}");

                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                            if (beforeLink != string.Empty)
                            {
                                if (configVerbose.Value) Log.LogInfo($"\t\t\t\t beforeLink task starting");



                                    try
                                    {
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Try translate through [https://deepl.com/] input: {beforeLink}");
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Invoking translate method whit params: \r\n" +
                                                                                $"\t\t\t\t\t\t text       = {beforeLink} \r\n" +
                                                                                $"\t\t\t\t\t\t toLanguage = {configChatTranslateToLanguage.Value.ToString()} \r\n" +
                                                                                $"\t\t\t\t\t\t apiKey     = {configDeeplApiKey.Value}" +
                                                                                $"");
                                        TranslatorDeeplKeys toLang = Optimus.STFC.UniversalTranslator.EnumExtensions.GetValueFromDescription<TranslatorDeeplKeys>(configChatTranslateToLanguage.Value);
                                        KeyValuePair<string, string> translate = Utils.TranslateDeepl(beforeLink, toLang, Log, configVerbose.Value, configDeeplApiKey.Value);
                                        beforeLink = translate.Value;
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t from language: {translate.Key}. result: {beforeLink}");
                                        lang = translate.Key;
                                        context._tmpText.text = beforeLink + " " + link + " " + afterLink;
                                        playerNameHeader = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                        if (!playerNameHeader.Contains("Universal Translator")) // v{VERSION} from {lang.ToUpper()}:
                                        {
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t add Universal Translator tag to player name header");
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                            context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text += $"<color=#ffcc00><size=15>   Universal Translator v{VERSION} detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");
                                        }
                                        else
                                        {
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t change Universal Translator tag to player name header");
                                            var headerString = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{headerString}]");

                                            context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text = headerString.Substring(0, headerString.IndexOf("detected language ")) + $"detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        if (configVerbose.Value) Log.LogInfo($"ERROR in beforeLink translation: {e.Message}");
                                    }

                            }
                            if (afterLink != string.Empty)
                            {
                                if (configVerbose.Value) Log.LogInfo($"\t\t\t\t afterLink task starting");


                                    try
                                    {
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Try translate through [https://deepl.com/] input: {afterLink}");
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Invoking translate method whit params: \r\n" +
                                            $"\t\t\t\t\t\t text       = {afterLink} \r\n" +
                                            $"\t\t\t\t\t\t toLanguage = {configChatTranslateToLanguage.Value.ToString()} \r\n" +
                                            $"\t\t\t\t\t\t apiKey     = {configDeeplApiKey.Value}" +
                                            $"");
                                        TranslatorDeeplKeys toLang = Optimus.STFC.UniversalTranslator.EnumExtensions.GetValueFromDescription<TranslatorDeeplKeys>(configChatTranslateToLanguage.Value);
                                        KeyValuePair<string, string> translate = Utils.TranslateDeepl(afterLink, toLang, Log, configVerbose.Value, configDeeplApiKey.Value);
                                        afterLink = translate.Value;
                                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t from language: {translate.Key}. result: {afterLink}");
                                        lang = translate.Key;
                                        context._tmpText.text = beforeLink + " " + link + " " + afterLink;
                                        playerNameHeader = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                        if (!playerNameHeader.Contains("Universal Translator")) // v{VERSION} from {lang.ToUpper()}:
                                        {
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t add Universal Translator tag to player name header");
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                            context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text += $"<color=#ffcc00><size=15>   Universal Translator v{VERSION} detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");
                                        }
                                        else
                                        {
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t change Universal Translator tag to player name header");
                                            var headerString = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{headerString}]");

                                            context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text = headerString.Substring(0, headerString.IndexOf("detected language ")) + $"detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        if (configVerbose.Value) Log.LogInfo($"ERROR in afterLink translation: {e.Message}");
                                    }


                            }
                        }, CancellationToken.None);
                    }
                    else
                    {
                        if (configVerbose.Value) Log.LogInfo($"\t\t\t\t no link task starting");

                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                            try
                            {
                                if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Try translate through [https://deepl.com/] input: {textToTranslate}");
                                
                                if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Invoking translate method whit params: \r\n" +
                                    $"\t\t\t\t\t\t text       = {textToTranslate} \r\n" +
                                    $"\t\t\t\t\t\t toLanguage = {configChatTranslateToLanguage.Value.ToString()} \r\n" +
                                    $"\t\t\t\t\t\t apiKey     = {configDeeplApiKey.Value}" +
                                    $"");
                                TranslatorDeeplKeys toLang = Optimus.STFC.UniversalTranslator.EnumExtensions.GetValueFromDescription<TranslatorDeeplKeys>(configChatTranslateToLanguage.Value);
                                KeyValuePair<string, string> translate = Utils.TranslateDeepl(textToTranslate, toLang, Log, configVerbose.Value, configDeeplApiKey.Value);
                                if (configVerbose.Value) Log.LogInfo($"\t\t\t\t from language: {translate.Key}. result: {translate.Value}");
                                lang = translate.Key;
                                //////////////////////////////////////////////////////////////////////////////////////////////
                                playerNameHeader = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                if (!playerNameHeader.Contains("Universal Translator")) // v{VERSION} from {lang.ToUpper()}:
                                {
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t add Universal Translator tag to player name header");
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                    context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text += $"<color=#ffcc00><size=15>   Universal Translator v{VERSION} detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");
                                }
                                else
                                {
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t change Universal Translator tag to player name header");
                                    var headerString = context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text;
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag before: [{headerString}]");

                                    context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text = headerString.Substring(0, headerString.IndexOf("detected language ")) + $"detected language {lang.ToUpper()}:</size></color>";// v{VERSION}:
                                    if (configVerbose.Value) Log.LogInfo($"\t\t\t\t tag after:  [{context.gameObject.transform.Find("Bubble/Text/NameTime/PlayerName").GetComponent<TextMeshProUGUI>().text}]");

                                }
                                /////////////////////////////////////////////////////////////////////////////////////////////
                                context._tmpText.text = translate.Value;
                            }
                            catch (Exception e)
                            {
                                if (configVerbose.Value) Log.LogInfo($"ERROR in no match translation: {e.Message}");
                            }

                        }, CancellationToken.None);
                    }

                    __instance.transform.parent.GetComponentsInChildren<Button>(true)[4].Press();

                }
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Outgoing translate logic

        [HarmonyPatch(typeof(ChatService), "SendMessage", new System.Type[] { typeof(string), typeof(string), typeof(CallbackContainer<string>) })] //"Show", new System.Type[] { typeof(int), typeof(bool) })] //"Start")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void SendMessage(string channelID, ref string message, CallbackContainer<string> callback)
        {
            if (configVerbose.Value) Log.LogInfo($"ChatService.SendMessage(string channelID, string message, Digit.Networking.Core.CallbackContainer<string> callback):\n" +
                                                    $"\t\t\t\t channelID =\t{channelID}\n" +
                                                    $"\t\t\t\t message   =\t{message}\n" +
                                                    $"\t\t\t\t callback  =\t{callback}\n");

            string textToTranslate = message;

            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Try translate through [https://deepl.com/] input: {textToTranslate}");

            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t Invoking translate method whit params: \r\n" +
                $"\t\t\t\t\t\t text       = {textToTranslate} \r\n" +
                $"\t\t\t\t\t\t toLanguage = {configChatTranslateToLanguage.Value.ToString()} \r\n" +
                $"\t\t\t\t\t\t apiKey     = {configDeeplApiKey.Value}" +
                $"");
            TranslatorDeeplKeys toLang = Optimus.STFC.UniversalTranslator.EnumExtensions.GetValueFromDescription<TranslatorDeeplKeys>(configTranslateOutgoingToLanguage.Value);
            KeyValuePair<string, string> translate = Utils.TranslateDeepl(textToTranslate, toLang, Log, configVerbose.Value, configDeeplApiKey.Value);
            if (configVerbose.Value) Log.LogInfo($"\t\t\t\t from language: {translate.Key}. result: {translate.Value}");
            message = translate.Value;
            if (configVerbose.Value) Log.LogInfo($"ChatService.SendMessage(string channelID, string message, Digit.Networking.Core.CallbackContainer<string> callback):\n" +
                                        $"\t\t\t\t channelID =\t{channelID}\n" +
                                        $"\t\t\t\t message   =\t{message}\n" +
                                        $"\t\t\t\t callback  =\t{callback}\n");
        }

        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
