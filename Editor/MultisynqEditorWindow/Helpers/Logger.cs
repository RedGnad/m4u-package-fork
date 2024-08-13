using System.Text.RegularExpressions;
using UnityEngine;

static public class Logger {

  static private string blue = "#006AFF";
  static private string lightBlue = "#0196FF";

  static private string spacer  = "-------------";
  // static private string hspacer = "=============";
  // static private string wave    = "◠‿◠‿◠‿◠‿◠‿◠‿◠‿";
  // static private string wave2   = "'``'-.,_,.-'``'-.,_,.";
  static private string wave3   = "ø,¸¸,ø¤º°`°º¤";
  static private string wave3r  = "¤º°`°º¤ø,¸¸,ø";

  static private string docsRootUrl = "https://croquet.io/dev/docs/unity/#";

  public static void Header(string message, string s1 = null, string s2 = null, string c1 = null, string c2 = null, string suffix = null) {
    if (s1 == null) s1 = wave3;
    if (s2 == null) s2 = wave3r;
    if (c1 == null) c1 = blue;
    if (c2 == null) c2 = lightBlue;
    Debug.Log($"<color={c1}>{s1} [ <color={c2}>{message}</color> ] {s2}</color>{suffix}");
  }

  static public void MethodHeader() {
    Header(GetClassAndMethod(), spacer, spacer);
  }
  static public void MethodHeaderAndOpenUrl() {
    var shortNm = Regex.Replace(GetMethodName(), @"^Clk_(.+)_Docs$", "$1");
    string url = $"{docsRootUrl}{shortNm}";
    Header(GetClassAndMethod(), spacer, spacer, null, null, "\n   " + url);
    Application.OpenURL(url);
  }

  static public string GetMethodName(int depth = 2) {
    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
    System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(depth);
    System.Reflection.MethodBase methodBase = stackFrame.GetMethod();
    return methodBase.Name;
  }
  static public string GetClassAndMethod(int depth = 2) {
    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
    System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(depth);
    System.Reflection.MethodBase methodBase = stackFrame.GetMethod();
    return $"{methodBase.ReflectedType.Name}.{methodBase.Name}";
  }
}