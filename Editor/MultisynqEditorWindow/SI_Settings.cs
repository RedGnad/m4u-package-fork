using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SI_Settings: StatusItem {

  Button GotoSettings_Btn;
  Button SettingsCreate_Btn;
  Button GotoNodePath_Btn;
  Button GotoApiKey_Btn;

  public SI_Settings(MultisynqBuildAssistantEW parent) : base(parent) { }

  override public void InitUI() {
    Debug.Log("SI_Settings.InitUI()");
    SetupVisElem("Settings_Status_Img",  ref statusImage);
    SetupLabel(  "Settings_Message_Lbl", ref messageLabel);
    SetupButton( "GotoSettings_Btn",     ref GotoSettings_Btn,   Clk_GotoSettings);
    SetupButton( "SettingsCreate_Btn",   ref SettingsCreate_Btn, Clk_SettingsCreate);
    SetupButton( "GotoNodePath_Btn",     ref GotoNodePath_Btn,   null);
    SetupButton( "GotoApiKey_Btn",       ref GotoApiKey_Btn,     null);
  }
  override public void InitText() {
    Debug.Log("SI_Settings.InitText()");
    MqWelcome_StatusSets.settings = new StatusSet( messageLabel, statusImage,
      // (info, warning, error, success)
      $"Settings are ready to go!",
      $"Settings are set to defaults! Look for other red items below to fix this.",
      $"Settings asset is missing! Click <b>Create Settings</b> to make some.",
      $"Settings are configured!!! Well done!",
      "Settings status"
    );
    GotoSettings_Btn.SetEnabled(false);
    statusSet = MqWelcome_StatusSets.settings;
  }
  override public bool Check() {
    Debug.Log("SI_Settings.Check()");
    var cqStgs = CqFile.FindProjectCqSettings();
    if (cqStgs == null) {
      GotoSettings_Btn.SetEnabled(false);
      ShowVEs(SettingsCreate_Btn);
      MqWelcome_StatusSets.settings.error.Set();
      return false;
    } else {
      GotoSettings_Btn.SetEnabled(true);
      MqWelcome_StatusSets.settings.success.Set();
      HideVEs(SettingsCreate_Btn);
      ShowVEs(GotoSettings_Btn);
      return true;
    }
  }
  
  private void Clk_GotoSettings() { // SETTINGS  ------------- Click
    GotoSettings();
    Notify("Selected in Project.\nSee Inspector.");
  }


  private void Clk_SettingsCreate() { // SETTINGS  ------------- Click
    // CroquetSettings in scene
    var cqStgs = CqFile.EnsureSettingsFile();
    MqWelcome_StatusSets.ready.SetIsGood(cqStgs != null);
    if (cqStgs == null) Debug.LogError("Could not find or create CroquetSettings file");
    GotoSettings();
    ShowVEs(GotoNodePath_Btn, GotoApiKey_Btn);
    Check();
    parentWindow.CheckAllStatusForReady();
  }

  public void GotoSettings() { // SETTINGS  ------------- Click
    // Select the file in Project pane of the Editor so it shows up in the Inspector
    var cqStgs = CqFile.FindProjectCqSettings();
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

}


/*
  //-- Click - SETTINGS --------------------------------
  void SetupUI_Settings() {
    SetupVisElem("Settings_Status_Img",  ref Settings_Status_Img);
    SetupLabel(  "Settings_Message_Lbl", ref Settings_Message_Lbl);
    SetupButton( "GotoSettings_Btn",     ref GotoSettings_Btn,   Clk_GotoSettings);
    SetupButton( "SettingsCreate_Btn",   ref SettingsCreate_Btn, Clk_SettingsCreate);
  }

  private bool Check_Settings() {
    var cqStgs = FindProjectCqSettings();
    if (cqStgs == null) {
      GotoSettings_Btn.SetEnabled(false);
      ShowVEs(SettingsCreate_Btn);
      MqWelcome_StatusSets.settings.error.Set();
      return false;
    } else {
      GotoSettings_Btn.SetEnabled(true);
      MqWelcome_StatusSets.settings.success.Set();
      HideVEs(SettingsCreate_Btn);
      ShowVEs(GotoSettings_Btn);
      return true;
    }
  }

  private void Clk_GotoSettings() { // SETTINGS  ------------- Click
    GotoSettings();
    Notify("Selected in Project.\nSee Inspector.");
  }

  private void Clk_SettingsCreate() { // SETTINGS  ------------- Click
    // CroquetSettings in scene
    var cqStgs = EnsureSettingsFile();
    MqWelcome_StatusSets.ready.SetIsGood(cqStgs != null);
    if (cqStgs == null) Debug.LogError("Could not find or create CroquetSettings file");
    GotoSettings();
    ShowVEs(GotoNodePath_Btn, GotoApiKey_Btn);
    Check_Settings();
    CheckAllStatusForReady();
  }

 */