using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Multisynq;

//------------------ ||||||||||||||||||||||||| ----------------------------------
public partial class MultisynqBuildAssistantEW : EditorWindow {

  //=============================================================================
  public double countdown_ToConvertSuccesses = -1;
  double lastTime = 0;
  double deltaTime = 0;

  Button CheckIfReady_Btn; // CHECK IF READY
  Toggle ClearLogs_Tgl;    // CLEAR LOGS
  bool clearLogs = true;

  //==== Status Items (SI_) =====================================================
  public SI_ReadyTotal        siReadyTotal;
  public SI_Settings          siSettings;
  public SI_Node              siNode;
  public SI_ApiKey            siApiKey;
  public SI_Bridge            siBridge;
  public SI_Systems           siSystems;
  public SI_BridgeHasSettings siBridgeHasSettings;
  public SI_JsBuildTools      siJsBuildTools;
  public SI_HasAppJs          siHasAppJs;
  public SI_JsBuild           siJsBuild;
  public SI_JbtVersionMatch   siJbtVersionMatch;
  public SI_BuiltOutput       siBuiltOutput;
  public SI_JsPlugins         siJsPlugins;

  //====== EditowWindow Init (auto-called when Shown) ==================================
  public void CreateGUI() {

    var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Mq_File.ewFolder + "MultisynqBuildAssistant_UI.uxml");
    var labelFromUXML = visualTree.Instantiate();
    rootVisualElement.Add(labelFromUXML);

    SetupUI();
    StatusSet.InitTextures();
    StatusItem.AllStatusSetsToBlank();
  }

  //=============================================================================

  private void SetupUI() {
    StatusItem.ClearStaticLists();
    siReadyTotal        = new SI_ReadyTotal(this);
    siReadyTotal.SetupButton("CheckIfReady_Btn", ref CheckIfReady_Btn, Clk_CheckIfReady);
    siReadyTotal.SetupToggle("ClearLogs_Tgl",    ref ClearLogs_Tgl,    Tgl_ClearLogs   );
    siSettings          = new SI_Settings(this);
    siNode              = new SI_Node(this);
    siApiKey            = new SI_ApiKey(this);
    siBridge            = new SI_Bridge(this);
    siSystems           = new SI_Systems(this);
    siBridgeHasSettings = new SI_BridgeHasSettings(this);
    siJsBuildTools      = new SI_JsBuildTools(this);
    siHasAppJs          = new SI_HasAppJs(this);
    siJsBuild           = new SI_JsBuild(this);
    siJbtVersionMatch   = new SI_JbtVersionMatch(this);
    siBuiltOutput       = new SI_BuiltOutput(this);  
    siJsPlugins         = new SI_JsPlugins(this);  

    StatusItem.HideMostButtons();
  }

  //=============================================================================

  //-- Clicks - CHECK READINESS --------------------------------
  private void Clk_CheckIfReady() { // CHECK READINESS  ------------- Click
    Logger.Header("Check If Ready");
    CheckAllStatusForReady();
  }
  static string YN(bool b) => b ? "<color=#33bb33>✔️</color>" : "<b><color=#ff5555>✕</color></b>";
  //=============================================================================
  private void Tgl_ClearLogs(ChangeEvent<bool> e) {
    Logger.Header("Clearing Logs... " + (e.newValue ? "(Enabled)" : "(Disabled)"));
    clearLogs = e.newValue;
  }
  public void ClearLogs() {
    if (clearLogs) {
      Logger.Header("Clearing Logs... " + (clearLogs ? "(Enabled)" : "(Disabled)"));
      var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
      var type = assembly.GetType("UnityEditor.LogEntries");
      var method = type.GetMethod("Clear");
      method.Invoke(new object(), null);
    }
  }
  //=============================================================================
  public void CheckAllStatusForReady() {
    bool allRdy = true;
    // NEVER: allRdy &= Statuses.ready.IsOk() // NEVER want this
    bool stgsOk    = siSettings.Check();          allRdy &= stgsOk;
    bool nodeOk    = siNode.Check();              allRdy &= nodeOk;
    bool apiOk     = siApiKey.Check();            allRdy &= apiOk;
    bool brdgOk    = siBridge.Check();            allRdy &= brdgOk;
    bool sysesOk   = siSystems.Check();           allRdy &= sysesOk;
    bool brgStgsOk = siBridgeHasSettings.Check(); allRdy &= brgStgsOk;
    bool toolsOk   = siJsBuildTools.Check();      allRdy &= toolsOk;
    bool indexJsOk = siHasAppJs.Check();          allRdy &= indexJsOk;
    bool jsBuilt   = siJsBuild.Check();           allRdy &= jsBuilt;
    bool verOk     = siJbtVersionMatch.Check();   allRdy &= verOk;
    bool appBuilt  = siBuiltOutput.Check();       allRdy &= appBuilt;
    bool pluginsOk = siJsPlugins.Check();         allRdy &= pluginsOk;
    Debug.Log($"{YN(stgsOk)}stgs, {YN(nodeOk)}node, {YN(apiOk)}apiKey, {YN(brdgOk)}bridge, {YN(sysesOk)}systms, {YN(brgStgsOk)}brgStgs, {YN(toolsOk)}tools, {YN(indexJsOk)}indexJs, {YN(jsBuilt)}jsBuilt, {YN(verOk)}ver, {YN(appBuilt)}appBuilt, {YN(pluginsOk)}plugins");
    Debug.Log($"<color=#ffff44>All Ready=</color>{(allRdy ? "<color=#33bb33>true</color>" : "<b><color=#ff5555>false</color></b>")}");
    siNode.NodePathsToDropdownAndCheck();
    siReadyTotal.AllAreReady(allRdy);
  }

  //=============================================================================
  void Update() {
    Update_DeltaTime();
    Update_CountdownAndMessage(ref countdown_ToConvertSuccesses, siReadyTotal.messageLabel, StatusSetMgr.ready, true);
  }

  void Update_DeltaTime()  {
    if (lastTime == 0) lastTime = EditorApplication.timeSinceStartup;
    deltaTime = EditorApplication.timeSinceStartup - lastTime;
    lastTime  = EditorApplication.timeSinceStartup;
  }

  void Update_CountdownAndMessage(ref double countdownSeconds, Label messageField, StatusSet status, bool showTimer = false) {

    if (countdownSeconds > 0f) {
      countdownSeconds -= deltaTime;
      if (showTimer) messageField.text = status.success.message + "   <b>" + countdownSeconds.ToString("0.0") + "</b>";
      if (countdownSeconds <= 0f) {
        countdownSeconds = -1;
        messageField.text = status.success.message;
        StatusItem.AllSuccessesToReady(); // <=======
      }
    }
  }

  //====== Singleton ============================================================
  static private MultisynqBuildAssistantEW _Instance;
  static public MultisynqBuildAssistantEW Instance {
    get {
      if (_Instance == null)_Instance = GetWindow<MultisynqBuildAssistantEW>(); // <-- this will auto-create the window
      return _Instance;
    }
    private set{}
  }
  void OnDestroy() {
    _Instance = null;
  }

  //====== Open _this_ Editor Window from   >>>> MENU <<<<    =============================
  [MenuItem("Multisynq/Open Multisynq Build Assistant Window...",priority=0)]
  [MenuItem("Window/Multisynq/Open Build Assistant...",priority=1000)]
  public static void ShowMultisynqWelcome_MenuMethod() {
    if (_Instance != null) _Instance.Close(); // First destroy the old one...
    var icon = AssetDatabase.LoadAssetAtPath<Texture>(Mq_File.ewFolder + "Images/MultiSynq_Icon.png");
    Instance.titleContent = new GUIContent("Multisynq Build Assistant", icon); // Referencing the Instance property will auto-create the window
  }

  [UnityEditor.Callbacks.DidReloadScripts]
  private static void OnScriptsReloaded() { // detect recompile and reopen
    if (_Instance != null) ShowMultisynqWelcome_MenuMethod();
  }
}
