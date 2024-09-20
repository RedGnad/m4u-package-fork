using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SI_Systems: StatusItem {

  Button AddCqSys_Btn;        
  Button ListMissingCqSys_Btn;

    public SI_Systems(MultisynqBuildAssistantEW parent = null) : base(parent)
    {
    }

    override public void InitUI() {
    //Debug.Log("SI_Systems.InitUI()");
    SetupVisElem("HasCqSys_Img",                      ref statusImage);
    SetupLabel(  "HasCqSys_Message_Lbl",              ref messageLabel);
    SetupButton( "AddCqSys_Btn",                      ref AddCqSys_Btn,                      Clk_AddCqSys);
    SetupButton( "ListMissingCqSys_Btn",              ref ListMissingCqSys_Btn,              Clk_ListMissingCqSys);
  }
  override public void InitText() {
    //Debug.Log("SI_Systems.InitText()");
    MqWelcome_StatusSets.hasCqSys = new StatusSet( messageLabel, statusImage,
      // (info, warning, error, success, blank)
      "Croquet Systems are ready to go!",
      "Croquet Systems are missing",
      "Croquet Systems are missing! Click <b>Add Croquet Systems</b> to get them.",
      "Croquet Systems installed!!! Well done!",
      "Croquet Systems status"
    );
    statusSet = MqWelcome_StatusSets.hasCqSys;
  }

  override public bool Check() { // SYSTEMS
    (string critRpt, string optRpt) = MissingSystemsRpt();
    bool noneMissing = (critRpt + optRpt == "");
    bool critMissing = (critRpt != "");

    if (noneMissing) {
      HideVEs( AddCqSys_Btn, ListMissingCqSys_Btn );
      MqWelcome_StatusSets.hasCqSys.success.Set();
    } else {
      ShowVEs( AddCqSys_Btn, ListMissingCqSys_Btn );
      if (critMissing) {
        MqWelcome_StatusSets.hasCqSys.error.Set();
        Debug.LogError("Missing Critical Croquet Systems:\n" + critRpt);
      } else {
        MqWelcome_StatusSets.hasCqSys.warning.Set();
        Debug.LogWarning("Missing Optional Croquet Systems:\n" + optRpt);
      }
    }
    return noneMissing;
  }

  //-- Clicks - HAS CROQUET SYSTEMS --------------------------------
  void Clk_AddCqSys() { // HAS CQ SYSTEMS  ------------- Click
    var cqBridge = Object.FindObjectOfType<CroquetBridge>();
    if (cqBridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      var cqGob = cqBridge.gameObject;
      string rpt = "";
      rpt += SceneHelp.EnsureComp<CroquetRunner>(cqGob);
      rpt += SceneHelp.EnsureComp<CroquetEntitySystem>(cqGob);
      rpt += SceneHelp.EnsureComp<CroquetSpatialSystem>(cqGob);
      rpt += SceneHelp.EnsureComp<CroquetMaterialSystem>(cqGob);
      rpt += SceneHelp.EnsureComp<CroquetFileReader>(cqGob);
      if (rpt == "") NotifyAndLog("All Croquet Systems are present in CroquetBridge GameObject.");
      else           NotifyAndLog("Added:\n"+rpt);
    }
    Check(); // recheck self (Cq Systems)
    edWin.CheckAllStatusForReady();
  }
  //--------------------------------------------------------------------------------
  (string,string) MissingSystemsRpt() {
    string critRpt = "";
    critRpt += (Object.FindObjectOfType<CroquetRunner>()         == null) ? "CroquetRunner\n"         : "";
    critRpt += (Object.FindObjectOfType<CroquetFileReader>()     == null) ? "CroquetFileReader\n"     : "";
    critRpt += (Object.FindObjectOfType<CroquetEntitySystem>()   == null) ? "CroquetEntitySystem\n"   : "";
    critRpt += (Object.FindObjectOfType<CroquetSpatialSystem>()  == null) ? "CroquetSpatialSystem\n"  : "";
    string optRpt = "";
    optRpt += (Object.FindObjectOfType<CroquetMaterialSystem>() == null) ? "CroquetMaterialSystem\n" : "";
    return (critRpt, optRpt);
  }

  void Clk_ListMissingCqSys() { // HAS CQ SYSTEMS  ------------- Click
    var cqBridge = Object.FindObjectOfType<CroquetBridge>();
    if (cqBridge == null) {
      NotifyAndLogError("Could not find CroquetBridge in scene!");
      return;
    } else {
      (string critRpt, string optRpt) = MissingSystemsRpt();
      if (critRpt + optRpt == "") NotifyAndLog("All Croquet Systems present.");
      else {
        if      (critRpt != "") NotifyAndLogError(  "Missing Critical:\n"+critRpt);
        else if (optRpt  != "") NotifyAndLogWarning("Missing Optional:\n"+optRpt);
      }
    }
  }



}
