﻿using System.Linq;
using System.Reflection;
using Il2CppSystem.IO;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController.Abilities;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppTMPro;
using SR2E.Commands;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Injection;
using SR2E.Saving;

namespace SR2E
{
    public static class BuildInfo
    {
        public const string Name = "SR2Essentials"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Essentials for Slime Rancher 2"; // Description for the Mod.  (Set as null if none)
        public const string Author = "ThatFinn"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.4.2"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = "https://www.nexusmods.com/slimerancher2/mods/60"; // Download Link for the Mod.  (Set as null if none)
    }

    public class SR2EEntryPoint : MelonMod
    {
        internal static bool infEnergy = false;
        internal static bool infHealth = false;
        internal static bool infEnergyInstalled = false;
        internal static bool infHealthInstalled = false;
        internal static bool consoleFinishedCreating = false;
        internal static bool syncConsole = true;
        internal static bool skipEngagementPrompt = false;
        internal static bool debugLogging = false;
        bool mainMenuLoaded = false;
        private static bool _iconChanged = false;
        static Image _modsButtonIconImage;

        internal static IdentifiableType[] identifiableTypes
        { get { return GameContext.Instance.AutoSaveDirector.identifiableTypes.GetAllMembers().ToArray().Where(identifiableType => !string.IsNullOrEmpty(identifiableType.ReferenceId)).ToArray(); } }
        
        internal static IdentifiableType getIdentifiableByName(string name)
        {
            foreach (IdentifiableType type in identifiableTypes)
                if (type.name.ToUpper() == name.ToUpper())
                    return type;
            return null;
        }
        internal static IdentifiableType getIdentifiableByLocalizedName(string name)
        {
            foreach (IdentifiableType type in identifiableTypes)
                try
                {
                    if (type.LocalizedName.GetLocalizedString().ToUpper().Replace(" ","") == name.ToUpper())
                        return type;
                }
                catch (System.Exception ignored)
                {}
            
            return null;
        }
        static bool CheckIfLargo(string value) => (value.Remove(0, 1)).Any(char.IsUpper);
        internal static MelonPreferences_Category prefs;
        internal static float noclipAdjustSpeed = 235f;

        internal static void RefreshPrefs()
        {
            prefs.DeleteEntry("noclipFlySpeed");
            prefs.DeleteEntry("noclipFlySprintSpeed");

            if (!prefs.HasEntry("noclipAdjustSpeed"))
                prefs.CreateEntry("noclipAdjustSpeed", (float)235f, "NoClip scroll speed", false);
            if (!prefs.HasEntry("doesConsoleSync"))
                prefs.CreateEntry("doesConsoleSync", (bool)false, "Console sync with ML log", false);
            if (!prefs.HasEntry("skipEngagementPrompt"))
                prefs.CreateEntry("skipEngagementPrompt", (bool)false, "Skip the engagement prompt", false);
            if (!prefs.HasEntry("debugLogging"))
                prefs.CreateEntry("debugLogging", (bool)false, "Log debug info", false);
            noclipAdjustSpeed = prefs.GetEntry<float>("noclipAdjustSpeed").Value;
            syncConsole = prefs.GetEntry<bool>("doesConsoleSync").Value;
            skipEngagementPrompt = prefs.GetEntry<bool>("skipEngagementPrompt").Value;
            debugLogging = prefs.GetEntry<bool>("debugLogging").Value;

        }
        public override void OnInitializeMelon()
        {
            prefs = MelonPreferences.CreateCategory("SR2Essentials");
            ClassInjector.RegisterTypeInIl2Cpp<SR2ESlimeDataSaver>();
            ClassInjector.RegisterTypeInIl2Cpp<SR2EGordoDataSaver>();
            RefreshPrefs();
            foreach (MelonBase melonBase in MelonBase.RegisteredMelons)
                switch (melonBase.Info.Name)
                {
                    case "InfiniteEnergy":
                        infEnergyInstalled = true;
                        break;
                    case "InfiniteHealth":
                        infHealthInstalled = true;
                        break;
                }
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "SystemCore":
                    System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SR2E.srtwoessentials.assetbundle");
                    byte[] buffer = new byte[16 * 1024];
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, read);

                    AssetBundle bundle = AssetBundle.LoadFromMemory(ms.ToArray());
                    foreach (var obj in bundle.LoadAllAssets())
                        if (obj != null)
                            if(obj.name=="AllMightyMenus")
                                Object.Instantiate(obj);

                    if (skipEngagementPrompt)
                    {
                        SR2EUtils.Get<GameObject>("EngagementSkipMessage").SetActive(true);
                    }
                    break;
                case "MainMenuUI":
                    infEnergy = false;
                    //SceneContext.Instance.PlayerState._model.maxHealth = InvincibleCommand.normalHealth;
                    infHealth = false;
                    if (skipEngagementPrompt)
                    {
                        SR2Console.transform.getObjRec<GameObject>("EngagementSkipMessage").SetActive(false);
                    }
                    break;
                case "StandaloneEngagementPrompt":
                    PlatformEngagementPrompt prompt = Object.FindObjectOfType<PlatformEngagementPrompt>();
                    Object.FindObjectOfType<CompanyLogoScene>().StartLoadingIndicator();
                    prompt.EngagementPromptTextUI.SetActive(false);
                    prompt.OnInteract(new InputAction.CallbackContext());
                    break;
                case "UICore":
                    InfiniteEnergyCommand.energyMeter = SR2EUtils.Get<EnergyMeter>("Energy Meter");
                    break;
                case "PlayerCore":
                    InfiniteEnergyCommand.jetpackAbilityData = SR2EUtils.Get<JetpackAbilityData>("Jetpack");
                    break;
            }

        }
        public override void OnSceneWasInitialized(int buildindex, string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenuUI":
                    mainMenuLoaded = true;
                    CreateModMenuButton();
                    break;
                    
            }

        }
        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenuUI":
                    mainMenuLoaded = false;
                    break;
                case "LoadScene":
                    SR2Warps.OnSceneLoaded(sceneName);
                    break;
            }
        }


        static SRCharacterController Player;
        static SRCameraController camera;

        public override void OnUpdate()
        {
            if (mainMenuLoaded)
                if (!_iconChanged)
                {
                    Sprite sprite = SR2EUtils.getObjRec<Image>(SR2Console.transform, "modsButtonIconImage").sprite;
                    if (sprite != null)
                        if (_modsButtonIconImage != null)
                        {
                            _modsButtonIconImage.sprite = sprite;
                            _iconChanged = true;
                        }
                }

            if (infEnergy)
                if (SceneContext.Instance != null)
                    if (SceneContext.Instance.PlayerState != null)
                        SceneContext.Instance.PlayerState.SetEnergy(int.MaxValue);
            if(infHealth)
                if (SceneContext.Instance != null)
                    if (SceneContext.Instance.PlayerState != null)
                        SceneContext.Instance.PlayerState.SetHealth(int.MaxValue);
            if (SceneContext.Instance != null)
                if (SceneContext.Instance.Player != null)
                    if (!SR2Console.isOpen)
                        if (!SR2ModMenu.isOpen)
                            if (Time.timeScale != 0)
                            {
                                if (Player == null)
                                    Player = SR2EUtils.Get<SRCharacterController>("PlayerControllerKCC");

                                if (camera == null)
                                    camera = SR2EUtils.Get<SRCameraController>("PlayerCameraKCC");
                            } 
                    

            if (!consoleFinishedCreating)
            {
                GameObject obj = GameObject.FindGameObjectWithTag("Respawn");
                if (obj != null)
                {
                    consoleFinishedCreating = true;
                    SR2Console.gameObject = obj;
                    obj.name = "SR2Console";
                    GameObject.DontDestroyOnLoad(obj);
                    SR2Console.transform = obj.transform;
                    SR2Console.Start();
                }
            }

            SR2Console.Update();
        }


        internal static void CreateModMenuButton()
        {
            MainMenuLandingRootUI rootUI = Object.FindObjectOfType<MainMenuLandingRootUI>();
            Transform buttonHolder = rootUI.transform.GetChild(0).GetChild(3);
            for (int i = 0; i < buttonHolder.childCount; i++)
                if (buttonHolder.GetChild(i).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text == "Mods")
                    Object.Destroy(buttonHolder.GetChild(i).gameObject);

            //Create Button
            GameObject newButton = Object.Instantiate(buttonHolder.GetChild(0).gameObject, buttonHolder);
            newButton.transform.SetSiblingIndex(buttonHolder.childCount==4?2:3);
            newButton.name = "ModsButton";
            newButton.transform.GetChild(0).GetComponent<CanvasGroup>().enabled = false;
            newButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Mods";
            Object.Destroy(newButton.transform.GetChild(0).GetChild(1).GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>());
            Object.Destroy(newButton.transform.GetChild(0).GetChild(1).GetComponent<Il2CppMonomiPark.SlimeRancher.UI.Framework.Layout.GameObjectWatcher>());
            _modsButtonIconImage = newButton.transform.GetChild(0).GetChild(2).GetComponent<Image>();

            _iconChanged = false;

            Object.Destroy(newButton.transform.GetChild(0).GetChild(1).GetComponent<LocalizedVersionText>());

            //Fix Selected Button SpaceBar Icon Dupe
            for (int i = 0; i < newButton.transform.GetChild(0).GetChild(0).GetChild(0).childCount; i++)
                newButton.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false);
            newButton.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);

            Button button = newButton.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener((System.Action)(() => { SR2ModMenu.Open(); }));
        }
    }
}