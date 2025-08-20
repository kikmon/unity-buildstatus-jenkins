using UnityEditor;
using UnityEngine;
using System.Diagnostics;

namespace UnityBuildStatusJenkins
{
    public static class PluginUtilities
    {
        [MenuItem("Window/Build Status/Open Project Folder")]
        public static void OpenProjectFolder()
        {
            var projectPath = Application.dataPath.Replace("/Assets", "");

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = projectPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
