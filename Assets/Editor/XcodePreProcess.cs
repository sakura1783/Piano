#if UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace iOSNative.Editor
{
    sealed class XcodePreProcess : IPreprocessBuildWithReport
    {
        public void OnPreprocessBuild(BuildReport report)
        {
            PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
            PlayerSettings.iOS.backgroundModes = iOSBackgroundMode.Audio;
        }

        public int callbackOrder { get; }
    }
}
#endif
