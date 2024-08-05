using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

static public class CqFile {

  static public string cqSettingsAssetOutputPath = "Assets/Croquet/CroquetSettings_XXXXXXXX.asset";
  static public string pkgRootFolder = "Packages/io.croquet.multiplayer";

  public static string ewFolder = pkgRootFolder + "/Editor/MultisynqEditorWindow/";
  public static string img_root = pkgRootFolder + "/Editor/MultisynqEditorWindow/Images/";

  static public string GetAppNameForOpenScene() {
    CroquetBridge cb = Object.FindObjectOfType<CroquetBridge>();
    if (cb == null) {
      Debug.LogError("Could not find CroquetBridge in scene!");
      return null;
    }
    string appName = cb.appName;
    if (appName == null || appName == "") {
      Debug.LogError("App Name is not set in CroquetBridge!");
      return null;
    }
    return appName;
  }

  static public void EnsureAssetsFolder(string folder) {
    string croquetFolder = Path.Combine("Assets", folder);
    if (!AssetDatabase.IsValidFolder(croquetFolder)) {
      AssetDatabase.CreateFolder("Assets", folder);
    }
  }
  // static public string GetCroquetJSFolder(bool isShortPath = true) {
  //   if (isShortPath) { 
  //     return "Assets/CroquetJS/"; 
  //   } else { 
  //     return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "..", "CroquetJS")); 
  //   }
  // }

  // static public string GetAppNameFolderForOpenScene(bool isShortPath = true) {
  //   string appName = GetAppNameForOpenScene();
  //   if (appName == null || appName == "") {
  //     MultisynqBuildAssistantEW.NotifyAndLogError("Could not find App Name in CroquetBridge!");
  //     return null;
  //   }
  //   string croquetJSFolder = GetCroquetJSFolder(isShortPath);
  //   return Path.Combine(croquetJSFolder, appName);
  // }

  // static public string GetAppNamePathForOpenScene(bool isShortPath = true) {
  //   return GetAppNameFolderForOpenScene(isShortPath) + "/index.js";
  // }

  static public FolderThing PrefabJsFolder() {
    return new FolderThing(Path.GetFullPath(pkgRootFolder + "/PrefabActorJS"));
  }

  static public string GetStarterTemplateFolder() {
    return Path.Combine(PrefabJsFolder().longPath, "templates", "starter");
  }
  static public FolderThing StarterTemplateFolder() {
    return new FolderThing(GetStarterTemplateFolder());
  }
  static public FolderThing CroquetJS() {
    return new FolderThing("Assets/CroquetJS/");
  }
  static public FolderThing AppFolder(bool canBeMissing = false) {
    return new FolderThing("Assets/CroquetJS/" + GetAppNameForOpenScene(), canBeMissing);
  }
  static public FileThing AppIndexJs() {
    return new FileThing("Assets/CroquetJS/" + GetAppNameForOpenScene() + "/index.js");
  }
  static public FolderThing PkgPrefabFolder() {
    return new FolderThing(pkgRootFolder + "/Prefabs");
  }
  static public FileThing CqSettingsTemplateFile() {
    return new FileThing( PkgPrefabFolder().shortPath + "/CroquetSettings_Template.asset");
  }
  static public FolderThing StreamingAssetsAppFolder(string _appNm = null) {
    string appNm = (_appNm != null) ? _appNm : GetAppNameForOpenScene();
    if (appNm == null) {
      Debug.LogError("Could not find App Name in CroquetBridge!");
      return MakeBlank();
    }
    var ft = new FolderThing(Path.Combine(Application.streamingAssetsPath, appNm));
    Debug.Log($"StreamingAssetsAppFolder: {ft.shortPath}");
    return ft;
  }
  static public FolderThing MakeBlank() {
    return new FolderThing("");
  }
  static public FileThing AddAppNameOutputMarker(string appName) {
    // add a "MyFolderIsCroquetBuildOutput.txt" file to the folder to mark it as a Croquet output folder
    FileThing markerFile = StreamingAssetsAppFolder(appName).DeeperFile("MyFolderIsCroquetBuildOutput.txt");
    markerFile.MakeFile("This file marks that its containing folder is a Croquet build output folder.\nDo not delete, please.\nThanks!");
    return markerFile;
  }
  static public bool AppNameOutputFolderHasMarker(string appName) {
    FileThing markerFile = StreamingAssetsAppFolder(appName).DeeperFile("MyFolderIsCroquetBuildOutput.txt");
    return markerFile.Exists();
  }
  static public List<FolderThing> ListAppNameOutputFolders() {
    var dirs = new FolderThing(Application.streamingAssetsPath).ChildFolders();
    // filter out non-Croquet output folders without MyFolderIsCroquetBuildOutput.txt using Linq
    return dirs.Where(dir => dir.DeeperFile("MyFolderIsCroquetBuildOutput.txt").Exists()).ToList();
  }
  static public void RenameToUnHideAppNameOutputFolders() {
    foreach (FolderThing dir in ListAppNameOutputFolders()) {
      if (dir.shortPath.Contains("~")) {
        string newName = dir.shortPath.Replace("~", "");
        AssetDatabase.RenameAsset(dir.shortPath, newName);
      }
    }
  }
  static public void RenameToHideAppNameOutputFoldersExceptOne(string appName) {
    foreach (FolderThing dir in ListAppNameOutputFolders()) {
      if (dir.shortPath.Contains(appName)) {
        dir.SelectAndPing();
      } else {
        string newName = dir.shortPath + "~";
        AssetDatabase.RenameAsset(dir.shortPath, newName);
      }
    }
  }
  static public bool AllScenesHaveBridgeWithAppNameSet() {
      string[] buildingScenes = EditorBuildSettings.scenes.Where( s => s.enabled ).Select( s => s.path ).ToArray();
      // load each scene in the list and get its CroquetBridge
      foreach (string scenePath in buildingScenes) {
        EditorSceneManager.OpenScene(scenePath);
        var bridge = Object.FindObjectOfType<CroquetBridge>();
        if (bridge == null) {
          Debug.LogError("Could not find CroquetBridge in scene: " + scenePath);
          return false;
        } else {
          // grab the appName from the CroquetBridge and make sure there is a folder for it in the StreamingAssets folder
          string appName = bridge.appName;
          if (appName == "") {
            Debug.LogError("CroquetBridge in scene: " + scenePath + " has no appName set.");
            return false;
          } else {
            var appFolder = CqFile.StreamingAssetsAppFolder(appName);
            if (!appFolder.Exists()) {
              Debug.LogError("Could not find app folder: " + appFolder);
              return false;
            }
          }
        }
      }
      if (buildingScenes.Length == 0) {
        Debug.LogError("No scenes in Build Settings.\nAdd some scenes to build.");
        return false;
      } else {
        Debug.Log("Yay! >> All scenes have CroquetBridge with appName set and app folder in StreamingAssets.");
        return true;
      }

  }
}

//=================== |||||||||| ====================
public abstract class PathyThing {

  public string shortPath;
  public string longPath;
  public string folderShort;
  public string folderLong;

  public UnityEngine.Object unityObj;

  public PathyThing(string maybeShortPath) {
    // if it contains Assets/ or Packages/, strip back to that
    string projectFolder = Path.GetFullPath(Application.dataPath + "/..");
    // use replace to remove prefix
    string _shortPath = maybeShortPath.Replace(projectFolder+"/", "");
    // if (!_shortPath.StartsWith("Assets/") && !maybeShortPath.StartsWith("Packages/")) {
    //   MultisynqBuildAssistantEW.NotifyAndLogError($"Got '{maybeShortPath}'. Path must start with 'Assets/' or 'Packages/'");
    //   return;
    // }
    bool isBlank = (_shortPath == "");
    shortPath    = _shortPath;
    longPath     = (isBlank) ? "" : Path.GetFullPath(shortPath);
    Debug.Log($"PathyThing: shortPath: {shortPath} longPath: {longPath}");
    folderShort  = (isBlank) ? "" : Path.GetDirectoryName(shortPath);
    folderLong   = (isBlank) ? "" : Path.GetFullPath(folderShort);
  }

  abstract public bool Exists();

  public void LookupUnityObj() {
    if (unityObj != null) return;
    unityObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(shortPath);
    if (unityObj == null) {
      Debug.LogError("PathyThing: unityObj is null for " + shortPath);
    }
  }

  public bool Select() {
    LookupUnityObj();
    Selection.activeObject = unityObj;
    EditorUtility.FocusProjectWindow();
    return true;
  }

  public void SelectAndPing() {
    if (Select()) {
      EditorGUIUtility.PingObject(unityObj);
    }
  }
}

//========== ||||||||||| ====================
public class FolderThing : PathyThing {

  public FolderThing(string _shortPath, bool canBeMissing = false) : base(_shortPath) {
    bool isValidAstDbFolder = AssetDatabase.IsValidFolder(shortPath);
    if (isValidAstDbFolder) return;
    if (Directory.Exists(longPath)) return;
    if (!canBeMissing) Debug.LogError($"Got '{shortPath}' path must be a valid folder\n longPath='{longPath}'");
  }

  override public bool Exists() {
    bool doesExist =  Directory.Exists(longPath);
    if (!doesExist) {
      Debug.LogError($"FolderThing: does not exist: '{longPath}'");
    }
    return doesExist;
  }

  public FolderThing[] ChildFolders() {
    string[] dirs = Directory.GetDirectories(longPath);
    FolderThing[] folders = new FolderThing[dirs.Length];
    for (int i = 0; i < dirs.Length; i++) {
      folders[i] = new FolderThing(dirs[i]);
    }
    return folders;
  }

  // Deeper sub-folder
  public FolderThing DeeperFolder(params string[] deeperPaths) {    
    string newPath = longPath;
    foreach (string deeperPath in deeperPaths) {
      newPath = Path.Combine(newPath, deeperPath);
    }
    return new FolderThing(newPath);
  }
  // file in this folder
  public FileThing DeeperFile(params string[] file) {
    // return new FileThing(Path.Combine(longPath, file));
    // do the equivalent of JS ...file
    string newPath = longPath;
    foreach (string deeperPath in file) {
      newPath = Path.Combine(newPath, deeperPath);
    }
    return new FileThing(newPath);
  }

  public FileThing FirstFile() {
    string[] files = Directory.GetFiles(longPath);
    if (files.Length == 0) {
      MultisynqBuildAssistantEW.NotifyAndLogError("FolderThing: no files in folder");
      return null;
    }
    return new FileThing(files[0]);
  }
}
//========== ||||||||| ====================
public class FileThing : PathyThing {

  public FileThing(string _shortPath) : base(_shortPath) {
    if (AssetDatabase.IsValidFolder(shortPath)) {
      MultisynqBuildAssistantEW.NotifyAndLogError("FileThing: path must be a file, not a folder");
    }
    unityObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(shortPath);
  }

  override public bool Exists() {
    bool doesExist = File.Exists(longPath);
    if (!doesExist) {
      Debug.LogError($"FileThing: does not exist: '{longPath}'");
    }
    return doesExist;
  }
  public bool MakeFile(string txt) {
    File.WriteAllText(longPath, txt);
    return Exists();
  }
}