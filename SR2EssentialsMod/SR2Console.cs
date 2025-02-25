﻿using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using SR2E.Commands;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SR2E
{
    public static class SR2Console
    {
        public static List<string> commandHistory;
        public static int commandHistoryIdx = -1;
        /// <summary>
        /// Display a message in the console
        /// </summary>
        public static void SendMessage(string message)
        {
            SendMessage(message, SR2EEntryPoint.syncConsole);
        }
        /// <summary>
        /// Display a message in the console
        /// </summary>
        public static void SendMessage(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
        {
            if(String.IsNullOrEmpty(message))
                return;
            try
            {
                if (message.Contains("[SR2E]:"))
                {
                    return;
                }

                if (message.StartsWith("[UnityExplorer]"))
                {
                    return;
                }
                if (message.StartsWith("[]:"))
                {
                    return;
                }

                if (!SR2EEntryPoint.consoleFinishedCreating)
                    return;
                if (consoleContent.childCount >= maxMessages)
                    GameObject.Destroy(consoleContent.GetChild(0).gameObject);
                if (message.Contains("\n"))
                {
                    if (doMLLog) MelonLogger.Msg($"[SR2E]: {message}");
                    foreach (string singularLine in message.Split('\n'))
                        SendMessage(singularLine, doMLLog, false);
                    return;
                }
                else
                {
                    if (doMLLog && internal_logMLForSingleLine) MelonLogger.Msg($"[SR2E]: {message}");
                    GameObject instance = GameObject.Instantiate(specialMessagePrefab, consoleContent);
                    instance.gameObject.SetActive(true);
                    instance.transform.GetChild(0).gameObject.SetActive(true);
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 1f);
                    _scrollbar.value = 0f;
                    scrollCompletlyDown = true;
                    return;
                }
            }
            catch { }
        }
        /// <summary>
        /// Display an error in the console
        /// </summary>
        public static void SendError(string message)
        {
            SendError(message, SR2EEntryPoint.syncConsole);
        }
        /// <summary>
        /// Display an error in the console
        /// </summary>
        public static void SendError(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
        {
            if(String.IsNullOrEmpty(message))
                return;
            try
            {
                if (message.Contains("[SR2E]:"))
                {
                    return;
                }

                if (message.StartsWith("[UnityExplorer]"))
                {
                    return;
                }
                
                if (message.StartsWith("[]:"))
                {
                    return;
                }

                if (!SR2EEntryPoint.consoleFinishedCreating)
                    return;
                if (consoleContent.childCount >= maxMessages)
                    GameObject.Destroy(consoleContent.GetChild(0).gameObject);
                if (message.Contains("\n"))
                {
                    if (doMLLog) MelonLogger.Error($"[SR2E]: {message}");
                    foreach (string singularLine in message.Split('\n'))
                        SendError(singularLine, doMLLog, false);
                    return;
                }
                else
                {
                    if (doMLLog && internal_logMLForSingleLine) MelonLogger.Error($"[SR2E]: {message}");

                    GameObject instance = GameObject.Instantiate(specialMessagePrefab, consoleContent);
                    instance.gameObject.SetActive(true);
                    instance.transform.GetChild(0).gameObject.SetActive(true);
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(1f, 0f, 0, 1);
                    _scrollbar.value = 0f;
                    scrollCompletlyDown = true;
                    return;
                }
            }
            catch { }
        }
        /// <summary>
        /// Display an error in the console
        /// </summary>
        public static void SendWarning(string message)
        {
            SendWarning(message, SR2EEntryPoint.syncConsole);
        }
        /// <summary>
        /// Display an error in the console
        /// </summary>
        public static void SendWarning(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
        {
            if(String.IsNullOrEmpty(message))
                return;
            try
            {
                if (message.Contains("[SR2E]:"))
                {
                    return;
                }

                if (message.StartsWith("[UnityExplorer]"))
                {
                    return;
                }

                if (message.StartsWith("[]:"))
                {
                    return;
                }

                if (!SR2EEntryPoint.consoleFinishedCreating)
                    return;
                if (consoleContent.childCount >= maxMessages)
                    GameObject.Destroy(consoleContent.GetChild(0).gameObject);
                if (message.Contains("\n"))
                {
                    if (doMLLog) MelonLogger.Warning($"[SR2E]: {message}");
                    foreach (string singularLine in message.Split('\n'))
                        SendWarning(singularLine, doMLLog, false);
                    return;
                }
                else
                {
                    if (doMLLog && internal_logMLForSingleLine) MelonLogger.Warning($"[SR2E]: {message}");
                    GameObject instance = GameObject.Instantiate(specialMessagePrefab, consoleContent);
                    instance.gameObject.SetActive(true);
                    instance.transform.GetChild(0).gameObject.SetActive(true);
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
                    instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 0, 1);
                    _scrollbar.value = 0f;
                    scrollCompletlyDown = true;
                    return;
                }
            }
            catch { }
        }
        /// <summary>
        /// Check if console is open
        /// </summary>
        public static bool isOpen
        { get { return transform.GetChild(0).gameObject.activeSelf; } }

        /// <summary>
        /// Closes the console
        /// </summary>
        public static void Close()
        {
            for (int i = 0; i < autoCompleteContent.childCount; i++)
            { Object.Destroy(autoCompleteContent.GetChild(i).gameObject); }

            consoleBlock.SetActive(false);
            consoleMenu.SetActive(false);
            try { SystemContext.Instance.SceneLoader.UnpauseGame(); } catch (Exception e) { }
            Object.FindObjectOfType<InputSystemUIInputModule>().actionsAsset.Enable();

        }

        /// <summary>
        /// Opens the console
        /// </summary>
        public static void Open()
        {
            if (SR2ModMenu.isOpen)
                return;
            if (SR2Warps.warpTo != null)
                return;

            consoleBlock.SetActive(true);
            consoleMenu.SetActive(true);
            try { SystemContext.Instance.SceneLoader.TryPauseGame(); } catch (Exception e) { }
            Object.FindObjectOfType<InputSystemUIInputModule>().actionsAsset.Disable();
            RefreshAutoComplete(commandInput.text);
        }
        /// <summary>
        /// Toggles the console
        /// </summary>
        public static void Toggle()
        {
            if (SystemContext.Instance.SceneLoader.CurrentSceneGroup.name != "StandaloneStart" &&
                SystemContext.Instance.SceneLoader.CurrentSceneGroup.name != "CompanyLogo" &&
                SystemContext.Instance.SceneLoader.CurrentSceneGroup.name != "LoadScene")
            {
                if (isOpen)
                    Close();
                else
                    Open();
            }
        }
        /// <summary>
        /// Registers a command to be used in the console
        /// </summary>
        public static bool RegisterCommand(SR2CCommand cmd)
        {
            if (commands.ContainsKey(cmd.ID.ToLowerInvariant()))
            {
                SendMessage($"Trying to register command with id '<color=white>{cmd.ID.ToLowerInvariant()}</color>' but the ID is already registered!");
                return false;
            }
            commands.Add(cmd.ID.ToLowerInvariant(), cmd);
            List<KeyValuePair<string, SR2CCommand>> myList = commands.ToList();

            myList.Sort(delegate (KeyValuePair<string, SR2CCommand> pair1, KeyValuePair<string, SR2CCommand> pair2) { return pair1.Key.CompareTo(pair2.Key); });
            commands = myList.ToDictionary(x => x.Key, x => x.Value);
            return true;
        }

        /// <summary>
        /// Unregisters a command
        /// </summary>
        public static bool UnRegisterCommand(SR2CCommand cmd)
        {
            return UnRegisterCommand(cmd.ID);
        }
        /// <summary>
        /// Unregisters a command
        /// </summary>
        public static bool UnRegisterCommand(string cmd)
        {
            if (commands.ContainsKey(cmd.ToLowerInvariant()))
            {
                commands.Remove(cmd.ToLowerInvariant());
                return true;
            }
            SendMessage($"Trying to unregister command with id '<color=white>{cmd.ToLowerInvariant()}</color>' but the ID is not registered!");
            return false;
        }
        /// <summary>
        /// Execute a string as if it was a commandId with args
        /// </summary>
        public static void ExecuteByString(string input, bool silent = false)
        {
            string[] cmds = input.Split(';');
            foreach (string c in cmds)
                if (!String.IsNullOrEmpty(c))
                {
                    bool spaces = c.Contains(" ");
                    string cmd = spaces ? c.Substring(0, c.IndexOf(' ')) : c;

                    if (commands.ContainsKey(cmd))
                    {
                        bool successful;
                        if (spaces)
                        {
                            var argString = c.TrimEnd(' ') + " ";
                            List<string> split = argString.Split(' ').ToList();
                            split.RemoveAt(0);
                            split.RemoveAt(split.Count - 1);
                            bool shouldRunNormalExecute = true;
                            if (split.Count != 0)
                            {
                                string[] stringArray = split.ToArray();
                                if (silent)
                                { shouldRunNormalExecute = !commands[cmd].SilentExecute(stringArray); }
                                if(shouldRunNormalExecute)
                                    successful = commands[cmd].Execute(stringArray);
                            }
                            else
                            {
                                if (silent)
                                { shouldRunNormalExecute = !commands[cmd].SilentExecute(null); }
                                if(shouldRunNormalExecute)
                                    successful = commands[cmd].Execute(null);
                            }
                        }
                        else
                        {
                            bool shouldRunNormalExecute = true;
                            if (silent)
                            { shouldRunNormalExecute = !commands[cmd].SilentExecute(null); }
                            if(shouldRunNormalExecute)
                                successful = commands[cmd].Execute(null);
                        }
                    }
                    else
                        SendError("Unknown command. Please use '<color=white>help</color>' for available commands");
                }
        }



        internal static Transform transform;
        internal static GameObject gameObject;
        internal static Dictionary<string, SR2CCommand> commands = new Dictionary<string, SR2CCommand>();

        static Scrollbar _scrollbar;
        static bool shouldResetTime = false;
        const int maxMessages = 100;
        private static bool scrollCompletlyDown = false;

        static void RefreshAutoComplete(string text)
        {
            autoCompleteContent.position = new Vector3(autoCompleteContent.position.x, 744, autoCompleteContent.position.z);
            if (selectedAutoComplete > autoCompleteContent.childCount - 1)
            {
                selectedAutoComplete = 0;
            }
            for (int i = 0; i < autoCompleteContent.childCount; i++)
                Object.Destroy(autoCompleteContent.GetChild(i).gameObject);
            if (String.IsNullOrEmpty(text))
            { autoCompleteScrollView.SetActive(false); return; }
            if (text.Contains(" "))
            {
                string cmd = text.Substring(0, text.IndexOf(' '));
                if (commands.ContainsKey(cmd))
                {
                    var argString = text;
                    List<string> split = argString.Split(' ').ToList();
                    split.RemoveAt(0);
                    int argIndex = split.Count - 1;
                    string[] args = null;
                    if (split.Count != 0)
                        args = split.ToArray();
                    List<string> possibleAutoCompletes = (commands[cmd].GetAutoComplete(argIndex, args));
                    if (possibleAutoCompletes != null)
                    {
                        int maxPredictions = 50; //This is to reduce lag
                        int predicted = 0;
                        foreach (string argument in possibleAutoCompletes)
                        {
                            if (predicted > maxPredictions)
                                return;
                            if (args != null)
                                if (!argument.ToLower().StartsWith(split[split.Count - 1].ToLower()))
                                    continue;
                            predicted++;
                            GameObject instance = Object.Instantiate(autoCompleteEntryPrefab, autoCompleteContent);
                            instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = argument;
                            instance.SetActive(true);
                            instance.GetComponent<Button>().onClick.AddListener((Action)(() =>
                            {
                                commandInput.text = cmd;

                                if (args != null)
                                {
                                    for (int i = 0; i < args.Length - 1; i++)
                                    {
                                        commandInput.text += " " + args[i];
                                    }

                                    commandInput.text += " " + argument;
                                }

                                commandInput.MoveToEndOfLine(false, false);
                            }));
                        }
                    }
                }
            }
            else
                foreach (KeyValuePair<string, SR2CCommand> valuePair in commands)
                    if (valuePair.Key.StartsWith(text))
                    {
                        GameObject instance = Object.Instantiate(autoCompleteEntryPrefab, autoCompleteContent);
                        instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = valuePair.Key;
                        instance.SetActive(true);
                        instance.GetComponent<Button>().onClick.AddListener((Action)(() =>
                        {
                            commandInput.text = valuePair.Key;
                            commandInput.MoveToEndOfLine(false, false);
                        }));
                    }
            autoCompleteScrollView.SetActive(autoCompleteContent.childCount != 0);

        }

        //Warps & Keybinding loading
        private static void SetupData()
        {
            SR2CommandBindingManager.Start();
            SR2Warps.Start();
        }
        //Setup ModMenu
        private static void SetupModMenu()
        {
            SR2ModMenu.parent = transform;
            SR2ModMenu.gameObject = SR2EUtils.getObjRec<GameObject>(transform, "modMenu");
            SR2ModMenu.transform = SR2EUtils.getObjRec<Transform>(transform, "modMenu");
            SR2ModMenu.Start();
        }
        private static void SetupCommands()
        {
            consoleBlock = SR2EUtils.getObjRec<GameObject>(transform, "consoleBlock");
            consoleMenu = SR2EUtils.getObjRec<GameObject>(transform, "consoleMenu");
            consoleContent = SR2EUtils.getObjRec<Transform>(transform, "ConsoleContent");
            messagePrefab = SR2EUtils.getObjRec<GameObject>(transform, "messagePrefab");
            specialMessagePrefab = SR2EUtils.getObjRec<GameObject>(transform, "specialMessagePrefab");
            commandInput = SR2EUtils.getObjRec<TMP_InputField>(transform, "commandInput");
            _scrollbar = SR2EUtils.getObjRec<Scrollbar>(transform, "ConsoleScroll");
            autoCompleteContent = SR2EUtils.getObjRec<Transform>(transform, "AutoCompleteContent");
            autoCompleteEntryPrefab = SR2EUtils.getObjRec<GameObject>(transform, "AutoCompleteEntry");
            autoCompleteScrollView = SR2EUtils.getObjRec<GameObject>(transform, "AutoCompleteScroll");
            autoCompleteScrollView.GetComponent<ScrollRect>().enabled = false;
            autoCompleteScrollView.SetActive(false);
            consoleBlock.SetActive(false);
            consoleMenu.SetActive(false);
            commandInput.onValueChanged.AddListener((Action<string>)((text) => { RefreshAutoComplete(text); }));
            RegisterCommand(new GiveCommand());
            RegisterCommand(new UtilCommand());
            ClassInjector.RegisterTypeInIl2Cpp(typeof(UtilCommand.ObjectBlocker));
            RegisterCommand(new BindCommand());
            RegisterCommand(new UnbindCommand());
            RegisterCommand(new SpawnCommand());
            RegisterCommand(new FastForwardCommand());
            RegisterCommand(new ClearCommand());
            RegisterCommand(new ClearInventoryCommand());
            RegisterCommand(new ModsCommand());
            RegisterCommand(new HelpCommand());
            RegisterCommand(new RefillSlotsCommand());
            RegisterCommand(new NewBucksCommand());
            RegisterCommand(new KillCommand());
            RegisterCommand(new GiveGadgetCommand());
            RegisterCommand(new GiveBlueprintCommand());
            RegisterCommand(new GiveUpgradeCommand());
            RegisterCommand(new ReplaceCommand());
            RegisterCommand(new WarpCommand());
            RegisterCommand(new SaveWarpCommand());
            RegisterCommand(new DeleteWarpCommand());
            RegisterCommand(new WarpListCommand());
            RegisterCommand(new SpeedCommand());
            RegisterCommand(new GravityCommand());
            RegisterCommand(new RotateCommand());
            RegisterCommand(new MoveCommand());
            RegisterCommand(new WeatherCommand());
            RegisterCommand(new FlingCommand());

            ConsoleVisibilityCommands.RegisterAllConsoleVisibilityCommands();
          
            if (!SR2EEntryPoint.infHealthInstalled)
                RegisterCommand(new InvincibleCommand());
            if (!SR2EEntryPoint.infEnergyInstalled)
                RegisterCommand(new InfiniteEnergyCommand());
            RegisterCommand(new NoClipCommand());
            ClassInjector.RegisterTypeInIl2Cpp(typeof(NoClipCommand.NoclipComponent));
        }
        private static void SetupConsoleSync()
        {
            MelonLogger.MsgCallbackHandler += (c1, c2, s1, s2) => SendMessage($"[{s1}]: {s2}", false);
            MelonLogger.ErrorCallbackHandler += (s, s1) => SendError($"[{s}]: {s1}", false);
            MelonLogger.WarningCallbackHandler += (s, s1) => SendWarning($"[{s}]: {s}", false);
        }

        internal static void Start()
        {
            commandHistory = new List<string>();
            if (SR2EEntryPoint.syncConsole)
            {
                SetupConsoleSync();
            }


            consoleBlock = SR2EUtils.getObjRec<GameObject>(transform, "consoleBlock");
            consoleMenu = SR2EUtils.getObjRec<GameObject>(transform, "consoleMenu");
            consoleContent = SR2EUtils.getObjRec<Transform>(transform, "ConsoleContent");
            messagePrefab = SR2EUtils.getObjRec<GameObject>(transform, "messagePrefab");
            specialMessagePrefab = SR2EUtils.getObjRec<GameObject>(transform, "specialMessagePrefab");
            commandInput = SR2EUtils.getObjRec<TMP_InputField>(transform, "commandInput");
            _scrollbar = SR2EUtils.getObjRec<Scrollbar>(transform, "ConsoleScroll");
            autoCompleteContent = SR2EUtils.getObjRec<Transform>(transform, "AutoCompleteContent");
            autoCompleteEntryPrefab = SR2EUtils.getObjRec<GameObject>(transform, "AutoCompleteEntry");
            autoCompleteScrollView = SR2EUtils.getObjRec<GameObject>(transform, "AutoCompleteScroll");
            autoCompleteScrollView.GetComponent<ScrollRect>().enabled = false;
            autoCompleteScrollView.SetActive(false);
            consoleBlock.SetActive(false);
            consoleMenu.SetActive(false);
            commandInput.onValueChanged.AddListener((Action<string>)((text) => { RefreshAutoComplete(text); }));

            SetupCommands();
            SetupData();
            SetupModMenu();
        }

        static TMP_InputField commandInput;
        static GameObject autoCompleteEntryPrefab;
        static GameObject consoleBlock;
        static GameObject consoleMenu;
        static Transform consoleContent;
        static Transform autoCompleteContent;
        static GameObject autoCompleteScrollView;
        static GameObject messagePrefab;
        static GameObject specialMessagePrefab;
        private static int selectedAutoComplete = 0;
        const int maxEntryOnScreen = 6;

        internal static void Update()
        {
            if (SR2EEntryPoint.consoleFinishedCreating != true)
                return;
            commandInput.ActivateInputField();
            if (isOpen)
            {
                if (scrollCompletlyDown)
                    if (_scrollbar.value != 0)
                    {
                        _scrollbar.value = 0f;
                        scrollCompletlyDown = false;
                    }

                if (Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    if (autoCompleteContent.childCount != 0)
                        try
                        {
                            autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Button>().onClick.Invoke();
                            selectedAutoComplete = 0;
                        }
                        catch { }

                }
            }
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                if (commandInput.text != "") Execute();
            
            if (commandHistoryIdx != -1 && !autoCompleteScrollView.active)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    commandInput.text = commandHistory[commandHistoryIdx];
                    commandHistoryIdx -= 1;
                }
            }

            if (Keyboard.current.ctrlKey.wasPressedThisFrame)
                if (Keyboard.current.tabKey.isPressed)
                    Toggle();
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                if (Keyboard.current.ctrlKey.isPressed)
                    Toggle();
            if (autoCompleteContent.childCount != 0 && autoCompleteScrollView.active)
            {
                if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                    NextAutoComplete();

                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                    PrevAutoComplete();
            }

            if (selectedAutoComplete == autoCompleteContent.childCount)
            {
                selectedAutoComplete = 0;
            }
            if (_scrollbar != null)
            {
                float value = Mouse.current.scroll.ReadValue().y;
                if (Mouse.current.scroll.ReadValue().y != 0)
                    _scrollbar.value = Mathf.Clamp(_scrollbar.value + ((value > 0.01 ? 1.25f : value < -0.01 ? -1.25f : 0) * _scrollbar.size), 0, 1f);

            }
            try
            {
                if (autoCompleteContent.childCount != 0)
                {
                    autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Image>().color = new Color32(255, 211, 0, 120);
                    if (selectedAutoComplete > maxEntryOnScreen)
                        autoCompleteContent.position = new Vector3(autoCompleteContent.position.x, 744 - (27 * maxEntryOnScreen) + (27 * selectedAutoComplete), autoCompleteContent.position.z);

                    else
                        autoCompleteContent.position = new Vector3(autoCompleteContent.position.x, 744, autoCompleteContent.position.z);
                }
            }
            catch { }
            SR2CommandBindingManager.Update();
            //Modmenu
            SR2ModMenu.Update();
        }

        public static void NextAutoComplete()
        {
            selectedAutoComplete += 1;
            if (selectedAutoComplete > autoCompleteContent.childCount - 1)
            {
                selectedAutoComplete = 0;
                autoCompleteContent.GetChild(autoCompleteContent.childCount - 1).GetComponent<Image>().color = new Color32(0, 0, 0, 25);
                autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Image>().color = new Color32(255, 211, 0, 120);
            }
            else
            {
                autoCompleteContent.GetChild(selectedAutoComplete - 1).GetComponent<Image>().color = new Color32(0, 0, 0, 25);
                autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Image>().color = new Color32(255, 211, 0, 120);
            }
        }
        public static void PrevAutoComplete()
        {
            selectedAutoComplete -= 1;

            if (selectedAutoComplete < 0)
            {
                selectedAutoComplete = autoCompleteContent.childCount - 1;
                autoCompleteContent.GetChild(0).GetComponent<Image>().color = new Color32(0, 0, 0, 25);
                autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Image>().color = new Color32(255, 211, 0, 120);
            }
            else
            {
                autoCompleteContent.GetChild(selectedAutoComplete + 1).GetComponent<Image>().color = new Color32(0, 0, 0, 25);
                autoCompleteContent.GetChild(selectedAutoComplete).GetComponent<Image>().color = new Color32(255, 211, 0, 120);
            }
        }

        static void Execute()
        {
            string cmds = commandInput.text;
            commandHistory.Add(cmds);
            commandHistoryIdx = commandHistory.Count - 1;
            commandInput.text = "";
            for (int i = 0; i < autoCompleteContent.childCount; i++)
                Object.Destroy(autoCompleteContent.GetChild(i).gameObject);
            ExecuteByString(cmds);

        }



    }
}