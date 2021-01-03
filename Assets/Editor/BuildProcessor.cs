using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public class BuildProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS
        // Get plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument ();
        plist.ReadFromString (File.ReadAllText (plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // Set encryption usage boolean
        string encryptKey = "ITSAppUsesNonExemptEncryption";
        rootDict.SetBoolean (encryptKey, false);

        // remove exit on suspend if it exists.
        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (rootDict.values.ContainsKey (exitsOnSuspendKey)) {
            rootDict.values.Remove (exitsOnSuspendKey);
        }

        // Write to file
        File.WriteAllText (plistPath, plist.WriteToString ());
#endif
    }
}