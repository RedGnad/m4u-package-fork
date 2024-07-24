using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//------------------ ||||||||||||||||||||||||| ----------------------------------
public partial class MultisynqBuildAssistantEW : EditorWindow {

  public static HandyColors colz;

  //=============================================================================
  double lastTime = 0;
  double deltaTime = 0;
  double countdown_ToConvertSuccesses = -1;

  //==== UI Refs ================================================================

  Button CheckIfReady_Btn; // CHECK IF READY

  VisualElement Checkmark_Img; // CHECK and MULTIPLY
  VisualElement Multiply_Img;

  VisualElement Ready_Status_Img; // ALL READY
  Label Ready_Message_Lbl;
  Button Awesome_Btn;        
  Button Top_Ready_Docs_Btn;

  VisualElement Settings_Status_Img; // SETTINGS
  Label Settings_Message_Lbl;
  Button GotoSettings_Btn;
  Button SettingsCreate_Btn;

  VisualElement Node_Status_Img; // NODE
  Label Node_Message_Lbl;
  Button TryAuto_Btn;
  Button GotoNodePath_Btn;
  DropdownField Node_Dropdown;

  VisualElement ApiKey_Status_Img; // API KEY
  Label ApiKey_Message_Lbl;
  Button SignUpApi_Btn;      
  Button GotoApiKey_Btn;
  Button ApiKey_Docs_Btn;

  VisualElement HaveBridge_Status_Img; // BRIDGE
  Label HaveBridge_Message_Lbl;
  Button GotoBridgeGob_Btn;
  Button CreateBridgeGob_Btn;

  VisualElement HasCqSys_Img; // HAS CQ SYSTEMS 
  Label HasCqSys_Message_Lbl;
  Button AddCqSys_Btn;
  Button ListMissingCqSys_Btn;

  VisualElement BridgeHasSettings_Img; // BRIDGE HAS SETTINGS
  Label BridgeHasSettings_Message_Lbl;
  Button BridgeHasSettings_AutoConnect_Btn;
  Button BridgeHasSettings_Goto_Btn;

  VisualElement JSBuildTools_Img; // JS BUILD TOOLS
  Label JSBuildTools_Message_Lbl;
  Button CopyJSBuildTools_Btn;
  Button GotoJSBuildToolsFolder_Btn;

  VisualElement HasAppJs_Status_Img; // HAS APP JS
  Label HasAppJs_Message_Lbl;
  Button SetAppName_Btn;
  Button MakeAppJsFile_Btn;
  Button HasAppJs_Docs_Btn;
  Button GotoAppJsFile_Btn;
  Button GotoAppJsFolder_Btn;

  VisualElement JbtVersionMatch_Img; // VERSION MATCH - JS BUILD TOOLS
  Label JbtVersionMatch_Message_Lbl;
  Button ReinstallTools_Btn;
  // Button GotoJSBuildToolsFolder_Btn;
  Button OpenBuildPanel_Btn;

  VisualElement JSBuild_Status_Img; // JS BUILD
  Label JSBuild_Message_Lbl;
  Button Build_JsNow_Btn;
  Button ToggleJSBuild_Btn;

  List<Button> allButtons = new();



  //====== Singleton ============================================================
  static private MultisynqBuildAssistantEW _Instance;
  static public MultisynqBuildAssistantEW Instance { 
    get {
      if (_Instance == null)_Instance = GetWindow<MultisynqBuildAssistantEW>();
      return _Instance;
    }
    private set{}
  }

  //=============================================================================

  private const string ewFolder = "Packages/io.croquet.multiplayer/Editor/MultisynqEditorWindow/";
  private const string cqSettingsAssetOutputPath = "Assets/Settings/CroquetSettings_XXXXXXXX.asset";

  //====== Menu ============================================================
  [MenuItem("Multisynq/Open Multisynq Build Assistant Window...",priority=10)]
  [MenuItem("Window/Multisynq/Open Build Assistant...",priority=1000)]
  public static void ShowMultisynqWelcome_MenuMethod() {
    var icon = AssetDatabase.LoadAssetAtPath<Texture>(ewFolder + "Images/MultiSynq_Icon.png");
    // referencing the Instance property will create the window if it doesn't exist 
    Instance.titleContent = new GUIContent("Multisynq Build Assistant", icon);
  }
  void OnDestroy() {
    _Instance = null;
  }

  //====== EditowWindow Init (auto-called when Shown) ==================================
  public void CreateGUI() {
    // Import UXML
    var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ewFolder + "MultisynqBuildAssistant_UI.uxml");
    var labelFromUXML = visualTree.Instantiate();
    rootVisualElement.Add(labelFromUXML);

    // A stylesheet can be added to a VisualElement.
    // The style will be applied to the VisualElement and all of its children.
    // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/CustomEditor.uss");
    // rootVisualElement.styleSheets.Add(styleSheet);

    // Custom init for our elements
    colz = new();
    SetupUI();
    SetupStatuses();
  }

  //=============================================================================
  
  private void SetupUI() {
    // CHECKMARK and MULTIPLY
    SetupVisElem("Checkmark_Img", ref Checkmark_Img);
    SetupVisElem("Multiply_Img",  ref Multiply_Img);
    // CHECK READINESS
    SetupButton("CheckIfReady_Btn", ref CheckIfReady_Btn, Clk_CheckIfReady);
    // READY
    SetupVisElem("Ready_Status_Img",   ref Ready_Status_Img);
    SetupLabel(  "Ready_Message_Lbl",  ref Ready_Message_Lbl); 
    SetupButton( "Awesome_Btn",        ref Awesome_Btn,        Clk_BeAwesome);
    SetupButton( "Top_Ready_Docs_Btn", ref Top_Ready_Docs_Btn, Clk_Top_Ready_Docs);
    // SETTINGS
    SetupVisElem("Settings_Status_Img",  ref Settings_Status_Img);
    SetupLabel(  "Settings_Message_Lbl", ref Settings_Message_Lbl);
    SetupButton( "GotoSettings_Btn",     ref GotoSettings_Btn,   Clk_GotoSettings);
    SetupButton( "SettingsCreate_Btn",   ref SettingsCreate_Btn, Clk_SettingsCreate);
    // NODE
    SetupVisElem("Node_Status_Img",  ref Node_Status_Img);
    SetupLabel(  "Node_Message_Lbl", ref Node_Message_Lbl);
    SetupButton( "GotoNodePath_Btn", ref GotoNodePath_Btn, Clk_GotoNodePath);
    SetupButton( "TryAuto_Btn",      ref TryAuto_Btn,      Clk_AutoSetupNode);
    Node_Dropdown = rootVisualElement.Query<DropdownField>("Node_Dropdown").First();
    Node_Dropdown.RegisterValueChangedCallback( (evt) => {
      string nodePath = evt.newValue.Replace(" ∕ ", "/");
      string nodeVer = TryNodePath(nodePath);
      if (nodeVer == null) {
        MqWelcome_StatusSets.node.error.Set();
      } else {
        MqWelcome_StatusSets.node.success.Set();
        // set the CroquetSetting.nodePath
        var cqStgs = FindProjectCqSettings();
        cqStgs.pathToNode = nodePath;
      }
      CheckAllStatusForReady();
    });

    // API KEY
    SetupVisElem("ApiKey_Status_Img",  ref ApiKey_Status_Img);
    SetupLabel(  "ApiKey_Message_Lbl", ref ApiKey_Message_Lbl);
    SetupButton( "SignUpApi_Btn",      ref SignUpApi_Btn,   Clk_SignUpApi);
    SetupButton( "GotoApiKey_Btn",     ref GotoApiKey_Btn,  Clk_EnterApiKey);
    SetupButton( "ApiKey_Docs_Btn",    ref ApiKey_Docs_Btn, Clk_ApiKey_Docs);
    // BRIDGE
    SetupVisElem("HaveBridge_Status_Img",  ref HaveBridge_Status_Img);
    SetupLabel(  "HaveBridge_Message_Lbl", ref HaveBridge_Message_Lbl);
    SetupButton( "GotoBridgeGob_Btn",      ref GotoBridgeGob_Btn,   Clk_GotoBridgeGob);
    SetupButton( "CreateBridgeGob_Btn",    ref CreateBridgeGob_Btn, Clk_CreateBridgeGob);
    // HAS CQ SYSTEMS 
    SetupVisElem("HasCqSys_Img",         ref HasCqSys_Img);
    SetupLabel(  "HasCqSys_Message_Lbl", ref HasCqSys_Message_Lbl);
    SetupButton( "AddCqSys_Btn",         ref AddCqSys_Btn, Clk_AddCqSys);
    SetupButton( "ListMissingCqSys_Btn", ref ListMissingCqSys_Btn, Clk_ListMissingCqSys);
    // BRIDGE HAS SETTINGS
    SetupVisElem("BridgeHasSettings_Img",             ref BridgeHasSettings_Img);
    SetupLabel(  "BridgeHasSettings_Message_Lbl",     ref BridgeHasSettings_Message_Lbl);
    SetupButton( "BridgeHasSettings_AutoConnect_Btn", ref BridgeHasSettings_AutoConnect_Btn, Clk_BridgeHasSettings_AutoConnect);
    SetupButton( "BridgeHasSettings_Goto_Btn",        ref BridgeHasSettings_Goto_Btn,        Clk_BridgeHasSettings_Goto);
    // JS BUILD TOOLS
    SetupVisElem("JSBuildTools_Img",           ref JSBuildTools_Img);
    SetupLabel(  "JSBuildTools_Message_Lbl",   ref JSBuildTools_Message_Lbl);
    SetupButton( "CopyJSBuildTools_Btn",       ref CopyJSBuildTools_Btn,       Clk_CopyJSBuildTools);
    SetupButton( "GotoJSBuildToolsFolder_Btn", ref GotoJSBuildToolsFolder_Btn, Clk_GotoJSBuildToolsFolder);

    // HAS APP JS
    SetupVisElem("HasAppJs_Status_Img",  ref HasAppJs_Status_Img);
    SetupLabel(  "HasAppJs_Message_Lbl", ref HasAppJs_Message_Lbl);
    SetupButton( "SetAppName_Btn",       ref SetAppName_Btn,      Clk_SetAppName);
    SetupButton( "MakeAppJsFile_Btn",    ref MakeAppJsFile_Btn,   Clk_MakeAppJsFile);
    SetupButton( "HasAppJs_Docs_Btn",    ref HasAppJs_Docs_Btn,   Clk_HasAppJs_Docs);
    SetupButton( "GotoAppJsFile_Btn",    ref GotoAppJsFile_Btn,   Clk_GotoAppJsFile);
    SetupButton( "GotoAppJsFolder_Btn",  ref GotoAppJsFolder_Btn, Clk_GotoAppJsFolder);

    // JS BUILD
    SetupVisElem("JSBuild_Status_Img", ref JSBuild_Status_Img);
    SetupLabel(  "JSBuild_Message_Lbl",  ref JSBuild_Message_Lbl);
    SetupButton( "ToggleJSBuild_Btn",   ref ToggleJSBuild_Btn, Clk_ToggleJSBuild); // Start JS Build Watcher
    SetupButton( "Build_JsNow_Btn",     ref Build_JsNow_Btn,   Clk_Build_JsNow);
    // VERSION MATCH - JS BUILD TOOLS
    SetupVisElem("JbtVersionMatch_Img",         ref JbtVersionMatch_Img);
    SetupLabel(  "JbtVersionMatch_Message_Lbl", ref JbtVersionMatch_Message_Lbl);
    SetupButton( "ReinstallTools_Btn",          ref ReinstallTools_Btn,     Clk_ReinstallTools);
    SetupButton( "OpenBuildPanel_Btn",          ref OpenBuildPanel_Btn,     Clk_OpenEditorBuildPanel);
    //-----
    // Hide most buttons
    HideMostButtons();
  }

  private void HideMostButtons() {
    foreach (Button button in allButtons) {
      string[] whitelisted = {
        "CheckIfReady_Btn",
        "_Docs_",
        // "GotoSettings_Btn",
      };
      if (whitelisted.Any(button.name.Contains)) {
        button.style.visibility = Visibility.Visible;
      } else {
        button.style.visibility = Visibility.Hidden;
      }
    }
    Node_Dropdown.style.visibility = Visibility.Hidden;
  }

  private void SetupStatuses() {
    string t_synq = "<b><color=#006AFF>Synq</color></b>";
    string t_key  = "<b><color=#006AFF>API Key</color></b>";
    string t_node = "<b><color=#417E37>Node</color></b>";
    string t_jsb  = "<b><color=#E5DB1C>JS Build</color></b>";
    // string t_js  = "<b><color=#E5DB1C>JS</color></b>";

    StatusSet.readyImg   = rootVisualElement.Query<VisualElement>("Checkmark_Img").First();
    StatusSet.warningImg = rootVisualElement.Query<VisualElement>("Warn_Img").First();
    StatusSet.errorImg   = rootVisualElement.Query<VisualElement>("Multiply_Img").First(); // Red X
    StatusSet.successImg = rootVisualElement.Query<VisualElement>("Checkmark_Img").First();
    StatusSet.blankImg   = rootVisualElement.Query<VisualElement>("Blank_Img").First();

    var hideTheseGrp = rootVisualElement.Query<VisualElement>("HideThese_Grp").First();
    // hideTheseGrp.style.display = DisplayStyle.None;

    MqWelcome_StatusSets.ready = new StatusSet( Ready_Message_Lbl, Ready_Status_Img,
      // (info, warning, error, success)
      $"You are <b><size=+1><color=#77ff77>Ready to </color>{t_synq}</b></size><color=#888>      All green lights below.",
      $"Warn 00000",
      $"Look below to fix what's not ready...",
      $"W00t!!! You are ready to {t_synq}!", // displays for 5 seconds, then switches to the .ready message
      "Press   Check If Ready   above"
    );
    MqWelcome_StatusSets.settings = new StatusSet( Settings_Message_Lbl, Settings_Status_Img,
      // (info, warning, error, success)
      $"Settings are ready to go!",
      $"Settings are set to defaults! Look for other red items below to fix this.",
      $"Settings asset is missing! Click <b>Create Settings</b> to make some.",
      $"Settings are configured!!! Well done!",
      "Settings status"
    );
    GotoSettings_Btn.SetEnabled(false);

    MqWelcome_StatusSets.node = new StatusSet( Node_Message_Lbl, Node_Status_Img,
      // (info, warning, error, success)
      $"{t_node} is ready to go!",
      $"{t_node} is not running",
      $"{t_node} needs your help getting set up.",
      $"{t_node} path configured!!! Well done!",
      "Node status"
    );
    MqWelcome_StatusSets.apiKey = new StatusSet( ApiKey_Message_Lbl, ApiKey_Status_Img,
      // (info, warning, error, success)
      $"The {t_key} is ready to go!",
      $"The {t_key} is not set",
      $"Let's get you a free {t_key}. It's easy.",
      $"The {t_key} is configured!!! Well done!",
      "API Key status"
    );
    MqWelcome_StatusSets.bridge = new StatusSet( HaveBridge_Message_Lbl, HaveBridge_Status_Img,
      // (info, warning, error, success)
      "Bridge GameObject is ready to go!",
      "Bridge GameObject is missing!",
      "Bridge GameObject is missing in scene! Click <b>Create Bridge</b> to make one.",
      "Bridge Gob <color=#888888>(GameObject)</color> found!! Well done!",
      "Bridge GameObject status"
    );
    MqWelcome_StatusSets.hasCqSys = new StatusSet( HasCqSys_Message_Lbl, HasCqSys_Img,
      // (info, warning, error, success, blank)
      "Croquet Systems are ready to go!",
      "Croquet Systems are missing",
      "Croquet Systems are missing! Click <b>Add Croquet Systems</b> to get them.",
      "Croquet Systems installed!!! Well done!",
      "Croquet Systems status"
    );
    MqWelcome_StatusSets.bridgeHasSettings = new StatusSet( BridgeHasSettings_Message_Lbl, BridgeHasSettings_Img,
      // ... info, warning, error, success)
      "Bridge has settings!",
      "Bridge is missing settings!",
      "Bridge is missing settings! Click <b>Auto Connect</b> to connect it.",
      "Bridge connected to settings!!! Well done!",
      "Bridge's Settings status"
    );
    MqWelcome_StatusSets.jsBuildTools = new StatusSet( JSBuildTools_Message_Lbl, JSBuildTools_Img,
      // (info, warning, error, success)
      "JS Build Tools are ready to go!",
      "JS Build Tools are missing",
      "JS Build Tools are missing! Click <b>Copy JS Build Tools</b> to get them.",
      "JS Build Tools installed!!! Well done!",
      "JS Build Tools status"
    );
    MqWelcome_StatusSets.hasAppJs = new StatusSet( HasAppJs_Message_Lbl, HasAppJs_Status_Img,
      // (info, warning, error, success)
      "JS file for AppName is ready to go!",
      "JS file for AppName is missing",
      "JS file for AppName is missing! Click <b>Make App JS File</b> to create one.",
      "JS file for AppName found! Well done!",
      "JS file for AppName status"
    );
    MqWelcome_StatusSets.jsBuild = new StatusSet( JSBuild_Message_Lbl, JSBuild_Status_Img,
      // (info, warning, error, success, blank)
      $"{t_jsb} output folder is present!",
      $"{t_jsb} is not ready",
      $"{t_jsb} needs your help getting set up.",
      $"{t_jsb} Build output folder found! Well done!",
      "JS Build status"
    );
    MqWelcome_StatusSets.versionMatch = new StatusSet( JbtVersionMatch_Message_Lbl, JbtVersionMatch_Img,
      // (info, warning, error, success, blank)
      $"Versions of {t_jsb} Tools and Built output match!",
      $"Versions of {t_jsb} Tools and Built output do not match",
      $"Versions of {t_jsb} Tools and Built output do not match!\n<b>Make a new or first build!</b>",
      $"Versions of {t_jsb} Tools and Built output match!!! Well done!",
      "Version Match status"
    );
    MqWelcome_StatusSets.AllStatusSetsToBlank();
  }
  //=============================================================================
  //=============================================================================

  //-- Clicks - CHECK READINESS --------------------------------
  private void Clk_CheckIfReady() { // CHECK READINESS  ------------- Click
    bool allRdy = true;
    allRdy &= Check_Settings(); 
    allRdy &= Check_Node();
    allRdy &= Check_ApiKey();
    allRdy &= Check_BridgeComponent();
    allRdy &= Check_HasCqSystems();
    allRdy &= Check_BridgeHasSettings();
    allRdy &= Check_JS_BuildTools();
    allRdy &= Check_HasAppJs();
    allRdy &= Check_ToolsVersionMatch();
    allRdy &= Check_JS_Build();
    //-----
    if (allRdy) AllAreReady();
    else        AllAreReady(false);
    NodePathsToDropdownAndCheck();
  }
  void NodePathsToDropdownAndCheck() {
    var nps = FindAllNodeIntances().Select( f => (f+"/node").Replace("/"," ∕ ") ).ToList();
    Node_Dropdown.choices = nps;
    Node_Dropdown.style.visibility = Visibility.Visible;
    // compare to CroquetSettings
    var cqStgs = FindProjectCqSettings();
    if (cqStgs != null) {
      string nodePath = cqStgs.pathToNode.Replace("/"," ∕ ");
      if (nps.Contains(nodePath)) {
        Node_Dropdown.SetValueWithoutNotify(nodePath);
        MqWelcome_StatusSets.node.success.Set();
      } else {
        MqWelcome_StatusSets.node.error.Set();
      }
    }
  }

  //-- Clicks - READY --------------------------------

  private void Clk_BeAwesome() { // READY  ------------- Click
    Debug.Log("Be Awesome!!!!");
    // Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ"); // Copilot thinks you should go to this url. You know you want to. =]
    Application.OpenURL("https://giphy.com/search/everything-is-awesome");
  }

  private void Clk_Top_Ready_Docs() { // READY  ------------- Click
    Application.OpenURL("https://multisynq.io/docs/unity/");
  }


  //-- Click - SETTINGS --------------------------------

  private void Clk_GotoSettings() { // SETTINGS  ------------- Click
    GotoSettings();
    Notify("Selected in Project.\nSee Inspector.");
  }

  private void Clk_SettingsCreate() { // SETTINGS  ------------- Click
    // CroquetSettings in scene
    var cqStgs = EnsureSettingsFile();
    if (cqStgs == null) {
      Debug.LogError("Could not find or create CroquetSettings file");
      MqWelcome_StatusSets.ready.error.Set();
    } else {
      MqWelcome_StatusSets.ready.success.Set();
    }
    GotoSettings();
    // make buttons for goto Node path and API key visible
    GotoNodePath_Btn.style.visibility = Visibility.Visible;
    GotoApiKey_Btn.style.visibility   = Visibility.Visible;
    // TryAuto_Btn.style.visibility = Visibility.Visible;
    Check_Settings();
    CheckAllStatusForReady();
  }

  private void GotoSettings() { // SETTINGS  ------------- Click
    // Select the file in Project pane of the Editor so it shows up in the Inspector
    var cqStgs = FindProjectCqSettings();
    if (cqStgs == null) {
      Debug.LogError("Could not find or create CroquetSettings file");
      return;
    } else {
      Notify("Selected in Project.\nSettings in Inspector.");
      Selection.activeObject = cqStgs;            // Select the settings you are using
      ProjectWindowUtil.ShowCreatedAsset(cqStgs); // Also show selection in Project pane
      EditorGUIUtility.PingObject(cqStgs);        // highlight in yellow
    }
  }

  //-- Clicks - NODE --------------------------------

  private void Clk_GotoNodePath() { // NODE  ------------- Click
    GotoSettings();
    // notify message
    var msg = "See Inspector.\n\nCroquet Settings\nwith node path\nselected in Project.";
    ShowNotification(new GUIContent(msg), 4);
  }

  List<string> FindAllNodeIntances() {

    List<string> nodePaths = new List<string>();
    // loop through possible node folders (by platform) and collect found ones in a List
    var nodeFolders = new List<string>();
    // fetch the home folder and expand any ~
    
    if (Application.platform == RuntimePlatform.OSXEditor) {
      string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      nodeFolders = new List<string>{ 
        "/usr/local/bin", 
        "/opt/homebrew/bin", 
        "/usr/bin", 
        $"{home}/.nvm/versions/node/*/bin" 
      };
    } else if (Application.platform == RuntimePlatform.WindowsEditor) {
      nodeFolders = new List<string>{ 
        "C:/Program Files/nodejs/node.exe" 
      };
    }
    // loop through the subfolders and expanding any * wildcards
    // make sure to split any folders with * into parent and wildcard

    var foldersWithNode = nodeFolders
      .SelectMany(folder => {
        if (folder.Contains("*")) {
          var parts = folder.Split('*');
          // remove any trailing slashes
          var parent = parts[0].TrimEnd('/');
          var child = parts[1].TrimStart('/');
          var expanded   = Directory.GetDirectories(parent, "*");
          var candidates = expanded.Select( d => d + "/" + child );
          // Debug.Log(  "Parent: " + parent + " Child: " + child +  " Found: " + y.Aggregate("", (acc, f) => acc + f + "\n") );
          return candidates; // i.e ["/usr/local/bin", "/opt/homebrew/bin"]
        } else {
          // Debug.Log("Folder: " + folder);
          return new string[]{ folder };
        }
      }).Where( folder => File.Exists(folder + "/node") && File.Exists(folder + "/npm") ).ToList();

    Debug.Log($"FindAllNodeIntances().foldersWithNode[{foldersWithNode.Count}] = [\n{foldersWithNode.Aggregate("", (acc, f) => $"{acc}  {f}/node,\n")}]");
    return foldersWithNode;
  }
  
  private void Clk_AutoSetupNode() { // NODE  ------------- Click
    Debug.Log("Auto Setup Node!");
    switch (Application.platform) {
      case RuntimePlatform.OSXEditor:
        Debug.Log("OSX Editor Detected");
        var cqStgs = FindProjectCqSettings();
        var nodePaths = FindAllNodeIntances();
        if (nodePaths==null || nodePaths.Count == 0) {
          NotifyAndLogError("Node not found on your system. To get it: https://nodejs.org/en/download/prebuilt-installer");
          MqWelcome_StatusSets.node.error.Set();
          return;
        } else {
          cqStgs.pathToNode = nodePaths[0] + "/node";
        }
        Check_Node();
        break;
      case RuntimePlatform.WindowsEditor:
        Debug.Log("Windows Editor Detected");
        string nodeVer = GetNodeVersion("cmd.exe", $"/c runwebpack.bat ");
        break;
      // case RuntimePlatform.LinuxEditor:
      //   Debug.Log("Linux Editor Detected");
      // break;
      default:
        Debug.LogError("Unsupported platform: " + Application.platform);
        break;
    }
    CheckAllStatusForReady();
  }

  //-- Clicks - API KEY --------------------------------

  private void Clk_SignUpApi() { // API KEY  ------------- Click
    GotoSettings();
    Application.OpenURL("https://croquet.io/account/");
  }

  private void Clk_EnterApiKey() {  // API KEY  ------------- Click
    GotoSettings();
    Notify("Selected in Project.\nSee Inspector.");
  }

  private void Clk_ApiKey_Docs() {
    Application.OpenURL("https://croquet.io/account/");
    // Application.OpenURL("https://multisynq.io/docs/unity/");
  }

  //-- Clicks - BRIDGE --------------------------------

  private void Clk_GotoBridgeGob() { // BRIDGE  ------------- Click
    var bridge = FindObjectOfType<CroquetBridge>();  // find ComponentType CroquetBridge in scene
    if (bridge == null) {
      NotifyAndLogError("Could not find\nCroquetBridge in scene!");
    } else {
      Selection.activeGameObject = bridge.gameObject; // select in Hierachy
      EditorGUIUtility.PingObject(bridge.gameObject); // highlight in yellow for a sec
      NotifyAndLog("Selected in Hierarchy.\nSee CroquetBridge in Inspector.");
    }
  }
  
  private void Clk_CreateBridgeGob() { // BRIDGE  ------------- Click
    var bridge = FindObjectOfType<CroquetBridge>();
    if (bridge != null) {
      string msg = "CroquetBridge already exists in scene";
      Notify(msg); Debug.LogError(msg);
    } else {
      var cbGob = new GameObject("CroquetBridge");
      var cb = cbGob.AddComponent<CroquetBridge>();
      cbGob.AddComponent<CroquetRunner>();
      cbGob.AddComponent<CroquetEntitySystem>();
      cbGob.AddComponent<CroquetSpatialSystem>();
      cbGob.AddComponent<CroquetMaterialSystem>();
      cbGob.AddComponent<CroquetFileReader>();
      cb.appName = "DefaultAppName";

      Selection.activeGameObject = cbGob;
      string msg = "Created CroquetBridge\nGameObject in scene.\nSelected it.";
      Notify(msg); Debug.Log(msg); 
      Check_BridgeComponent();
      CheckAllStatusForReady();
    }
    Check_BridgeComponent();
    Check_BridgeHasSettings();
    CheckAllStatusForReady();
  }
  string EnsureComp<T>(GameObject gob) where T : Component {
    var comp = FindObjectOfType<T>();
    string name = typeof(T).Name;
    if (comp == null) {
      var go = (gob==null) ? new GameObject(name) : gob;
      go.AddComponent<T>();
      Debug.Log($"Created {name} Component in scene.", gob);
      return name+"\n";
    } else {
      Debug.Log($"{name} already exists in scene.");
      return "";
    }
  }
  //-- Clicks - HAS CROQUET SYSTEMS --------------------------------
  void Clk_AddCqSys() { // HAS CQ SYSTEMS  ------------- Click
    var cqBridge = FindObjectOfType<CroquetBridge>();
    if (cqBridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      var cqGob = cqBridge.gameObject;
      string rpt = "";
      rpt += EnsureComp<CroquetRunner>(cqGob);
      rpt += EnsureComp<CroquetEntitySystem>(cqGob);
      rpt += EnsureComp<CroquetSpatialSystem>(cqGob);
      rpt += EnsureComp<CroquetMaterialSystem>(cqGob);
      rpt += EnsureComp<CroquetFileReader>(cqGob);
      if (rpt == "") {
        NotifyAndLog("All Croquet Systems are present in CroquetBridge GameObject.");
      } else {
        NotifyAndLog("Added:\n"+rpt);
      }
    }
    Check_HasCqSystems();
    CheckAllStatusForReady();
  }
  void Clk_ListMissingCqSys() { // HAS CQ SYSTEMS  ------------- Click
    var cqBridge = FindObjectOfType<CroquetBridge>();
    if (cqBridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      string rpt = "";
      rpt += (FindObjectOfType<CroquetRunner>()         == null) ? "CroquetRunner\n" : "";
      rpt += (FindObjectOfType<CroquetEntitySystem>()   == null) ? "CroquetEntitySystem\n" : "";
      rpt += (FindObjectOfType<CroquetSpatialSystem>()  == null) ? "CroquetSpatialSystem\n" : "";
      rpt += (FindObjectOfType<CroquetMaterialSystem>() == null) ? "CroquetMaterialSystem\n" : "";
      rpt += (FindObjectOfType<CroquetFileReader>()     == null) ? "CroquetFileReader\n" : "";
      if (rpt == "") {
        NotifyAndLog("All Croquet Systems present.");
      } else {
        NotifyAndLog("Missing Croquet Systems:\n"+rpt);
      }
    }
  }
  //-- Clicks - BRIDGE HAS SETTINGS --------------------------------

  void Clk_BridgeHasSettings_AutoConnect() { // BRIDGE HAS SETTINGS  ------------- Click
    var bridge = FindObjectOfType<CroquetBridge>();
    if (bridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      var cqSettings = FindProjectCqSettings();
      if (cqSettings == null) {
        NotifyAndLogError("Could not find CroquetSettings in project!");
        return;
      } else {
        bridge.appProperties = cqSettings;
        NotifyAndLog("Connected CroquetBridge to CroquetSettings!");
        Check_BridgeHasSettings();
        CheckAllStatusForReady();
      }
    }
  }

  void Clk_BridgeHasSettings_Goto() { // BRIDGE HAS SETTINGS  ------------- Click
    GotoSettings();
  }

  //-- Clicks - JS BUILD --------------------------------

  async void Clk_ToggleJSBuild() { // JS BUILD  ------------- Click
    if (ToggleJSBuild_Btn.text == "Start JS Build Watcher") {
      bool success = await CroquetBuilder.EnsureJSToolsAvailable();
      if (!success) return;
      CroquetBuilder.StartBuild(true); // true => start watcher
      Debug.Log("Started JS Build Watcher");
      ToggleJSBuild_Btn.text = "Stop JS Build Watcher";
    } else {
      ToggleJSBuild_Btn.text = "Start JS Build Watcher";
      CroquetBuilder.StopWatcher();
    }
  }

  async void Clk_Build_JsNow() { // JS BUILD  ------------- Click
    Debug.Log("Building JS...");
    bool success = await CroquetBuilder.EnsureJSToolsAvailable();
    if (!success) {
      var msg = "JS Build Tools are missing!!!\nCannot build.";
      Debug.LogError(msg);
      EditorUtility.DisplayDialog("Missing JS Tools", msg, "OK");
      return;
    }
    CroquetBuilder.StartBuild(false); // false => no watcher
    Check_JS_Build();
  }

  //-- JS BUILD TOOLS --------------------------------

  private async void Clk_CopyJSBuildTools() { // JS BUILD TOOLS  ------------- Click
    await CroquetBuilder.InstallJSTools();
    Check_JS_BuildTools();
    CheckAllStatusForReady();
  }

  private void Clk_GotoJSBuildToolsFolder() { // JS BUILD TOOLS  ------------- Click
    string croquetJSFolder = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "..", "CroquetJS"));
    string jsBuildFolder   = Path.GetFullPath(Path.Combine(croquetJSFolder, ".js-build"));
    if (!Directory.Exists(jsBuildFolder)) {
      NotifyAndLogError("Could not find\nJS Build Tools folder");
      return;
    }
    NotifyAndLog("CroquetJS/.js-build \nfolder opened\nin Finder/Explorer.");
    EditorUtility.RevealInFinder(jsBuildFolder);
  }
    // SetupButton( "SetAppName_Btn",       ref SetAppName_Btn,      Clk_SetAppName);
    // SetupButton( "MakeAppJsFile_Btn",    ref MakeAppJsFile_Btn,   Clk_MakeAppJsFile);
    // SetupButton( "HasAppJs_Docs_Btn",    ref HasAppJs_Docs_Btn,   Clk_HasAppJs_Docs);
    // SetupButton( "GotoAppJsFile_Btn",    ref GotoAppJsFile_Btn,   Clk_GotoAppJsFile);
    // SetupButton( "GotoAppJsFolder_Btn",  ref GotoAppJsFolder_Btn, Clk_GotoAppJsFolder);
  // VisualElement HasAppJs_Status_Img; // HAS APP JS
  // Label HasAppJs_Message_Lbl;
  // Button SetAppName_Btn;
  // Button MakeAppJsFile_Btn  ;
  // Button HasAppJs_Docs_Btn  ;
  // Button GotoAppJsFile_Btn  ;
  // Button GotoAppJsFolder_Btn;
    // string newAppName = EditorUtility.DisplayDialog("Set App Name", "Enter the name of your app", appName);

  //-- HAS APP JS --------------------------------

  private void Clk_SetAppName() { // HAS APP JS  ------------- Click
    var cqBridge = FindObjectOfType<CroquetBridge>();
    if (cqBridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      // direct user to enter an appName into the CroquetBridge field for appName
      // select the CroquetBridge in the scene
      Selection.activeGameObject = cqBridge.gameObject;
      EditorGUIUtility.PingObject(cqBridge.gameObject);
      EditorUtility.DisplayDialog("Set App Name", "Enter a name for your app into the CroquetBridge's field \n\nappName\n\n \n\nWe will select it for you, so check the Inspector.", "Ok");
    }

  }
  private void Clk_MakeAppJsFile() {}
  private void Clk_HasAppJs_Docs() {}
  private void Clk_GotoAppJsFile() {}
  private void Clk_GotoAppJsFolder() {}
  

  //-- VERSION MATCH - JS BUILD TOOLS --------------------------------

  void Clk_OpenEditorBuildPanel() { // Open Build - JS BUILD TOOLS  ------------- Click
    EditorWindow.GetWindow<BuildPlayerWindow>().Show();
    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes.Where( s => s.enabled ).ToArray();
    if (scenes.Length == 0) {
      NotifyAndLogError("No scenes in Build Settings.\nAdd some scenes to build.");
      return;
    }
  }
  void Clk_ReinstallTools() { // VERSION MATCH - JS BUILD TOOLS  ------------- Click
    CroquetMenu.InstallJSTools();
    Check_ToolsVersionMatch();
    CheckAllStatusForReady();
  }
  //=============================================================================
  //=============================================================================

  private void SetupButton(string buttonName, ref Button button, Action buttonAction) {
    button = rootVisualElement.Query<Button>(buttonName).First();
    if (button == null) {
      Debug.LogError("Could not find button: " + buttonName);
      return;
    }
    button.clicked += buttonAction;
    allButtons.Add(button);
  }

  private void SetupLabel(string labelName, ref Label label) {
    label = rootVisualElement.Query<Label>(labelName).First();
    if (label == null) {
      Debug.LogError("Could not find label: " + labelName);
    }
  }

  private void SetupVisElem(string visElemName, ref VisualElement visElem) {
    visElem = rootVisualElement.Query<VisualElement>(visElemName).First();
    if (visElem == null) {
      Debug.LogError("Could not find VisualElement: " + visElemName);
    }
  }

  //=============================================================================

  static public void NotifyAndLog(string msg, float seconds = 4) {
    Instance.ShowNotification(new GUIContent(msg), seconds);
    Debug.Log(msg);
  }
  static public void NotifyAndLogError(string msg, float seconds = 4) {
    Instance.ShowNotification(new GUIContent(msg), seconds);
    Debug.LogError(msg);
  }

  static public void Notify(string msg, float seconds = 4) {
    Instance.ShowNotification(new GUIContent(msg), seconds);
  }

  //=============================================================================

  private CroquetSettings CopyDefaultSettingsFile() {
    string path = ewFolder + "resources/CroquetSettings_Template.asset";
    AssetDatabase.CopyAsset(path, cqSettingsAssetOutputPath);
    return AssetDatabase.LoadAssetAtPath<CroquetSettings>(cqSettingsAssetOutputPath);
  }

  private CroquetSettings FindProjectCqSettings() {
    CroquetSettings cqSettings = null;
      // Check if the file is there
    string[] guids = AssetDatabase.FindAssets("t:CroquetSettings");
    guids = Array.FindAll(guids, guid => !AssetDatabase.GUIDToAssetPath(guid).Contains("Packages/"));

    if (guids.Length > 0) {
      cqSettings = AssetDatabase.LoadAssetAtPath<CroquetSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
      if (guids.Length > 1) {
        Debug.LogWarning("Found more than one CroquetSettings file. You should only have one.");
        // Print out path of all files found
        int i = 1;
        foreach (string guid in guids) {
          string path = AssetDatabase.GUIDToAssetPath(guid);
          // Make them select when click each log
          Debug.LogWarning( i++ + ". " + path, AssetDatabase.LoadAssetAtPath<CroquetSettings>(path));
        }
      }
    }
    if (cqSettings == null) {
      Debug.LogWarning("Could not find CroquetSettings.asset in your Assets folders.");
      MqWelcome_StatusSets.settings.error.Set();
      MqWelcome_StatusSets.node.error.Set();
      MqWelcome_StatusSets.apiKey.error.Set();
      MqWelcome_StatusSets.ready.error.Set();
    }
    return cqSettings;
  }

  private CroquetSettings EnsureSettingsFile() {
    CroquetSettings cqSettings = FindProjectCqSettings();
    // If not, copy file from ./resources/CroquetSettings_Template.asset
    // into Assets/Settings/CroquetSettings.asset
    if (cqSettings == null) {
      cqSettings = CopyDefaultSettingsFile();
    }
    return cqSettings;
  }
  
  //=============================================================================

  private void AllAreReady(bool really = true) {
    if (really) {
      MqWelcome_StatusSets.ready.success.Set();
      Awesome_Btn.style.visibility = Visibility.Visible;
      countdown_ToConvertSuccesses = 3f;
    } else {
      MqWelcome_StatusSets.ready.error.Set();
      Awesome_Btn.style.visibility = Visibility.Hidden;
    }
  }

  //=============================================================================

  private void CheckAllStatusForReady() {
    bool allRdy = true;
    // NEVER: allRdy &= Statuses.ready.IsOk() // NEVER want this
    allRdy &= MqWelcome_StatusSets.settings.IsOk();
    allRdy &= MqWelcome_StatusSets.node.IsOk();
    allRdy &= MqWelcome_StatusSets.apiKey.IsOk();
    allRdy &= MqWelcome_StatusSets.bridge.IsOk();
    allRdy &= MqWelcome_StatusSets.jsBuildTools.IsOk();
    allRdy &= MqWelcome_StatusSets.versionMatch.IsOk();
    allRdy &= MqWelcome_StatusSets.jsBuild.IsOk();
    if (allRdy) AllAreReady();      
    else        AllAreReady(false);
  }
  //=============================================================================

  private bool Check_Settings() {
    var cqStgs = FindProjectCqSettings();
    if (cqStgs == null) {
      GotoSettings_Btn.SetEnabled(false);
      SettingsCreate_Btn.style.visibility = Visibility.Visible;
      MqWelcome_StatusSets.settings.error.Set();
      return false;
    } else {
      GotoSettings_Btn.SetEnabled(true);
      MqWelcome_StatusSets.settings.success.Set();
      SettingsCreate_Btn.style.visibility = Visibility.Hidden;
      GotoSettings_Btn.style.visibility   = Visibility.Visible;
      return true;
    }
  }

  private bool Check_Node() {
    var cqStgs = FindProjectCqSettings();
    if (cqStgs == null) {
      MqWelcome_StatusSets.node.error.Set();
      // hide AutoSetup button
      TryAuto_Btn.style.visibility = Visibility.Hidden;
      return false;
    }
    TryAuto_Btn.style.visibility = Visibility.Visible;
    string nodePath = cqStgs.pathToNode;
    string nodeVer = TryNodePath(nodePath);
    if (nodeVer == null) {
      MqWelcome_StatusSets.node.error.Set();
      return false;
    } else {
      MqWelcome_StatusSets.node.success.Set();
      TryAuto_Btn.style.visibility      = Visibility.Hidden;
      GotoNodePath_Btn.style.visibility = Visibility.Visible;
      return true;
    }
  }

  private bool Check_ApiKey() {
    bool ok = false;
    var cqStgs = FindProjectCqSettings();

    if (cqStgs != null) {
      GotoApiKey_Btn.style.visibility = Visibility.Visible;
      SignUpApi_Btn.style.visibility  = Visibility.Visible;
      var apiKey = cqStgs.apiKey;
      if (apiKey == null || apiKey == "<go get one at multisynq.io>" || apiKey.Length < 1) {
        // curl -s -X GET -H "X-Croquet-Auth: 1_s77e6tyzkx5m3yryb9305sqxhkdmz65y69oy5s8e" -H "X-Croquet-App: io.croquet.vdom.ploma" -H "X-Croquet-Id: persistentId" -H "X-Croquet-Version: 1.1.0" -H "X-Croquet-Path: https://croquet.io" 'https://api.croquet.io/sign/join?meta=login'
        MqWelcome_StatusSets.apiKey.error.Set();
      } else {
        MqWelcome_StatusSets.apiKey.success.Set();
        ok = true;
      }
    }
    return ok;
  }

  private bool Check_BridgeComponent() {
    var bridge = FindObjectOfType<CroquetBridge>();
    bool fountIt = (bridge != null);
    if (!fountIt) {
      MqWelcome_StatusSets.bridge.error.Set();
      CreateBridgeGob_Btn.style.visibility = Visibility.Visible;
    } else {
      MqWelcome_StatusSets.bridge.success.Set();
      GotoBridgeGob_Btn.style.visibility   = Visibility.Visible;
      CreateBridgeGob_Btn.style.visibility = Visibility.Hidden;
    }
    return fountIt;
  }

  private bool Check_HasCqSystems() {
    // look for all the types
    var cb = FindObjectOfType<CroquetBridge>();
    var cr = FindObjectOfType<CroquetRunner>();
    var ce = FindObjectOfType<CroquetEntitySystem>();
    var cs = FindObjectOfType<CroquetSpatialSystem>();
    var cm = FindObjectOfType<CroquetMaterialSystem>();
    var cf = FindObjectOfType<CroquetFileReader>();
    // make a text report
    string rpt = "";
    rpt += (cb == null) ? "CroquetBridge\n" : "";
    rpt += (cr == null) ? "CroquetRunner\n" : "";
    rpt += (ce == null) ? "CroquetEntitySystem\n" : "";
    rpt += (cs == null) ? "CroquetSpatialSystem\n" : "";
    rpt += (cm == null) ? "CroquetMaterialSystem\n" : "";
    rpt += (cf == null) ? "CroquetFileReader\n" : "";
    // if any are missing, show the error
    if (rpt.Length > 0) {
      MqWelcome_StatusSets.hasCqSys.error.Set();
      AddCqSys_Btn.style.visibility = Visibility.Visible;
      Debug.LogWarning("Missing Croquet Systems:\n" + rpt);
      ListMissingCqSys_Btn.style.visibility = Visibility.Visible;
      return false;
    } else {
      MqWelcome_StatusSets.hasCqSys.success.Set();
      AddCqSys_Btn.style.visibility = Visibility.Hidden;
      ListMissingCqSys_Btn.style.visibility = Visibility.Hidden;
      return true;
    }
  }

  private bool Check_BridgeHasSettings() {
    var bridge = FindObjectOfType<CroquetBridge>();
    bool foundIt = (bridge != null);
    if (!foundIt) {
      MqWelcome_StatusSets.bridgeHasSettings.error.Set();
      BridgeHasSettings_AutoConnect_Btn.style.visibility = Visibility.Hidden;
      BridgeHasSettings_Goto_Btn.style.visibility = Visibility.Hidden;
    } else {
      if (bridge.appProperties == null) {
        MqWelcome_StatusSets.bridgeHasSettings.error.Set();
        BridgeHasSettings_AutoConnect_Btn.style.visibility = Visibility.Visible;
        foundIt = false;
      } else {
        MqWelcome_StatusSets.bridgeHasSettings.success.Set();
        BridgeHasSettings_AutoConnect_Btn.style.visibility = Visibility.Hidden;
        BridgeHasSettings_Goto_Btn.style.visibility = Visibility.Visible;
      }
    }
    return foundIt;
  }  

  private bool Check_JS_BuildTools() {
    string croquetJSFolder = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "..", "CroquetJS"));
    string jsBuildFolder   = Path.GetFullPath(Path.Combine(croquetJSFolder, ".js-build"));
    bool havejsBuildFolder = Directory.Exists(jsBuildFolder);
    if (!havejsBuildFolder) {
      MqWelcome_StatusSets.jsBuildTools.error.Set();
      CopyJSBuildTools_Btn.style.visibility = Visibility.Visible;
      Build_JsNow_Btn.style.visibility = Visibility.Hidden;
    } else {
      MqWelcome_StatusSets.jsBuildTools.success.Set();
      CopyJSBuildTools_Btn.style.visibility = Visibility.Hidden;
      GotoJSBuildToolsFolder_Btn.style.visibility = Visibility.Visible;
      Build_JsNow_Btn.style.visibility = Visibility.Visible;
    }
    return havejsBuildFolder;
  }

  private bool Check_HasAppJs() {
    var cqBridge = FindObjectOfType<CroquetBridge>();
    string appName = cqBridge.appName;
    // file should be in Assets/CroquetJS/<appName>/index.js
    string appNameInCqJsFolder = Path.Combine(Application.streamingAssetsPath, "..", "CroquetJS", appName);
    string appJsFile = Path.GetFullPath(Path.Combine(appNameInCqJsFolder, "index.js"));
    bool haveAppJsFile = File.Exists(appJsFile);
    if (!haveAppJsFile) {
      MqWelcome_StatusSets.hasAppJs.error.Set();
      SetAppName_Btn.style.visibility = Visibility.Visible;
      MakeAppJsFile_Btn.style.visibility = Visibility.Hidden;
      HasAppJs_Docs_Btn.style.visibility = Visibility.Hidden;
      GotoAppJsFile_Btn.style.visibility = Visibility.Hidden;
      GotoAppJsFolder_Btn.style.visibility = Visibility.Hidden;
    } else {
      MqWelcome_StatusSets.hasAppJs.success.Set();
      SetAppName_Btn.style.visibility = Visibility.Hidden;
      MakeAppJsFile_Btn.style.visibility = Visibility.Hidden;
      HasAppJs_Docs_Btn.style.visibility = Visibility.Hidden;
      GotoAppJsFile_Btn.style.visibility = Visibility.Visible;
      GotoAppJsFolder_Btn.style.visibility = Visibility.Visible;
    }


    SetAppName_Btn.style.visibility = Visibility.Visible;
    MakeAppJsFile_Btn.style.visibility = Visibility.Visible;
    HasAppJs_Docs_Btn.style.visibility = Visibility.Visible;
    GotoAppJsFile_Btn.style.visibility = Visibility.Visible;
    GotoAppJsFolder_Btn.style.visibility = Visibility.Visible;

    return haveAppJsFile;
  }

  private bool Check_ToolsVersionMatch() {
    // load the two ".last-installed-tools" files to compare versions and Tools levels
    // of (1) the tools in DotJsBuild and (2) the tools in CroquetBridge
    // var installedToolsForDotJsBuild    = LastInstalled.LoadPath(CroquetBuilder.installedToolsForDotJsBuild_Path);
    // var installedToolsForCroquetBridge = LastInstalled.LoadPath(CroquetBuilder.installedToolsForCroquetBridge_Path);
    var installedToolsForDotJsBuild    = LastInstalled.LoadPath(CroquetBuilder.JSToolsRecordInBuild);
    var installedToolsForCroquetBridge = LastInstalled.LoadPath(CroquetBuilder.JSToolsRecordInEditor);
    bool allMatch = installedToolsForDotJsBuild.IsSameAs(installedToolsForCroquetBridge);
    
    if (allMatch) {
      MqWelcome_StatusSets.versionMatch.success.Set();
      Debug.Log("JSTools for Editor & Build match!!!");
    } else {
      MqWelcome_StatusSets.versionMatch.error.Set();
      Debug.LogError( installedToolsForDotJsBuild.ReportDiffs(installedToolsForCroquetBridge) );
      ReinstallTools_Btn.style.visibility = Visibility.Visible;
    }
    ReinstallTools_Btn.style.visibility = Visibility.Visible;
    OpenBuildPanel_Btn.style.visibility = Visibility.Visible;

    return allMatch;
  }
  private bool Check_JS_Build() {
    var cqBridge = FindObjectOfType<CroquetBridge>();
    string appName = cqBridge.appName;
    string croquetJSFolder = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, appName));
    bool havejsBuildFolder = Directory.Exists(croquetJSFolder);
    if (havejsBuildFolder) {
      MqWelcome_StatusSets.jsBuild.success.Set();
    } else {
      MqWelcome_StatusSets.jsBuild.error.Set();
      Build_JsNow_Btn.style.visibility = Visibility.Visible;
    }
    return havejsBuildFolder;
  }

  //=============================================================================  

  private string TryNodePath(string nodePath) {
    if (!File.Exists(nodePath)) {
      Notify("Could not find node path file:\n" + nodePath);
      return null;
    } else {
      return GetNodeVersion(nodePath, "-v");
    }
  }
  private string GetNodeVersion(string executable = "", string arguments = "") {
    string output = RunShell(executable, arguments);
    string[] stdoutLines = output.Split('\n');

    // If output is a string that starts with v and then has a number,
    // then it's the version number, otherwise its an error
    foreach (string line in stdoutLines) {
      if (line.StartsWith("v") && float.TryParse(line.Substring(1,3), out float version)) {
        Debug.Log("Node version: " + line);
        return line;
      }
    }
    Debug.LogError("Node not found");
    return null;
  }

  private string RunShell(string executable = "", string arguments = "", int logLevel = 2, bool shellExec = false) {
    System.Diagnostics.Process pcs = new();
    pcs.StartInfo.UseShellExecute = shellExec;
    pcs.StartInfo.RedirectStandardOutput = true;
    pcs.StartInfo.RedirectStandardError = true;
    pcs.StartInfo.CreateNoWindow = true;
    pcs.StartInfo.WorkingDirectory = Path.GetFullPath(ewFolder);
    pcs.StartInfo.FileName = executable;
    pcs.StartInfo.Arguments = arguments;
    pcs.StartInfo.UserName = "root";
    pcs.Start();

    string output = pcs.StandardOutput.ReadToEnd();
    string errors = pcs.StandardError.ReadToEnd();
    pcs.WaitForExit();

    if (output.Length > 0 && logLevel > 1) Debug.Log("RunShell().output = '" + output + "'");
    if (errors.Length > 0 && logLevel > 0) Debug.LogError("RunShell().errors = '" + errors + "'");

    return output;
  }

  //=============================================================================  
  void Update() {
    Update_DeltaTime();
    Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, Ready_Message_Lbl, MqWelcome_StatusSets.ready, true);
    // Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, Node_Message_Lbl, Statuses.node);
    // Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, Key_Message_Lbl, Statuses.apiKey);
    // Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, JSBuild_Message_Lbl, Statuses.jsBuild);
    // Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, Settings_Message_Lbl, Statuses.settings);
  }
  void Update_DeltaTime()  {
    if (lastTime == 0) lastTime = EditorApplication.timeSinceStartup;
    deltaTime = EditorApplication.timeSinceStartup - lastTime;
    lastTime = EditorApplication.timeSinceStartup;
  }
  void Update_CountdownAndMessage(ref double countdownSeconds, Label messageField, StatusSet status, bool showTimer = false) {

    if (countdownSeconds > 0f) {
      countdownSeconds -= deltaTime;
      if (showTimer) messageField.text = status.success.message + "   <b>" + countdownSeconds.ToString("0.0") + "</b>";
      if (countdownSeconds <= 0f) {
        countdownSeconds = -1;
        messageField.text = status.success.message;
        MqWelcome_StatusSets.SuccessesToReady(); // <=======
      }
    }
  }
}