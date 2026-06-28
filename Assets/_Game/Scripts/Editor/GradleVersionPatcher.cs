using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EditorScript
{
    public class GradleVersionPatcher : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            SetGradleVersion();
        }

        [MenuItem("Tools/Set Gradle Version")]
        private static void SetGradleVersion()
        {
            AndroidExternalToolsSettings.Gradle.path = string.Empty;
        }
        
        // [MenuItem("Gradle/Log")]
        // private static void LogGradlePath()
        // {
        //     Debug.LogError(AndroidExternalToolsSettings.gradlePath);
        // }
    }
}