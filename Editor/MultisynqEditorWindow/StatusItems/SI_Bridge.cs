using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SI_Bridge: StatusItem {

  Button GotoBridgeGob_Btn;
  Button CreateBridgeGob_Btn;

    public SI_Bridge(MultisynqBuildAssistantEW parent = null) : base(parent)
    {
    }

    override public void InitUI() {
    SetupVisElem("HaveBridge_Status_Img",             ref statusImage);
    SetupLabel(  "HaveBridge_Message_Lbl",            ref messageLabel);
    SetupButton( "GotoBridgeGob_Btn",                 ref GotoBridgeGob_Btn,                 Clk_GotoBridgeGob);
    SetupButton( "CreateBridgeGob_Btn",               ref CreateBridgeGob_Btn,               Clk_CreateBridgeGob);
  }
  override public void InitText() {
    MqWelcome_StatusSets.bridge = new StatusSet( messageLabel, statusImage,
      // (info, warning, error, success)
      "Bridge GameObject is ready to go!",
      "Bridge GameObject is missing!",
      "Bridge GameObject is missing in scene! Click <b>Create Bridge</b> to make one.",
      "Bridge Gob <color=#888888>(GameObject)</color> found!! Well done!",
      "Bridge GameObject status"
    );
    statusSet = MqWelcome_StatusSets.bridge;
  }
  override public bool Check() { // BRIDGE
    var bridge = Object.FindObjectOfType<CroquetBridge>();
    bool fountIt = (bridge != null);
    MqWelcome_StatusSets.bridge.SetIsGood(fountIt);
    SetVEViz( fountIt, GotoBridgeGob_Btn  );
    SetVEViz(!fountIt, CreateBridgeGob_Btn);
    return fountIt;
  }
  
  //-- Clicks - BRIDGE --------------------------------

  private void Clk_GotoBridgeGob() { // BRIDGE  ------------- Click
    var bridge = Object.FindObjectOfType<CroquetBridge>();  // find ComponentType CroquetBridge in scene
    if (bridge == null) NotifyAndLogError("Could not find\nCroquetBridge in scene!");
    else {
      Selection.activeGameObject = bridge.gameObject; // select in Hierachy
      EditorGUIUtility.PingObject(bridge.gameObject); // highlight in yellow for a sec
      NotifyAndLog("Selected in Hierarchy.\nSee CroquetBridge in Inspector.");
    }
  }

  private void Clk_CreateBridgeGob() { // BRIDGE  ------------- Click
    var bridge = Object.FindObjectOfType<CroquetBridge>();
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
      var cqStgs = CqFile.FindProjectCqSettings();
      if (cqStgs != null) cb.appProperties = cqStgs;

      Selection.activeGameObject = cbGob;
      string msg = "Created CroquetBridge\nGameObject in scene.\nSelected it.";
      Notify(msg); Debug.Log(msg);
    }
    Check(); // check self (bridge)
    edWin.siSettings.Check();    // Check_BridgeHasSettings();
    edWin.siSystems.Check(); // Check_HasCqSystems();
    edWin.CheckAllStatusForReady();
  }
}
