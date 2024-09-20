using UnityEngine;
using UnityEditor;
using Multisynq;

[CustomEditor(typeof(SynqCommand_Mgr))]
public class SynqCommandMgrEditor : Editor {
  
  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    SynqCommand_Mgr manager = (SynqCommand_Mgr)target;
    if (GUILayout.Button("Inject JS Plugin Code")) {
      InjectCode(manager);
    }
    if (GUILayout.Button("Select Plugins Folder")) {
      var plFldr = Mq_File.AppFolder().DeeperFolder("plugins").EnsureExists();
      if (plFldr.FirstFile() != null) plFldr.FirstFile().SelectAndPing(true);
      else                            plFldr.SelectAndPing();
    }

  }

  private void InjectCode(SynqCommand_Mgr manager) {
    manager.InjectJsPluginCode();
    AssetDatabase.Refresh();
  }
}