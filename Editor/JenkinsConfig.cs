using UnityEngine;

namespace UnityBuildStatusJenkins
{
    [CreateAssetMenu(fileName = "JenkinsConfig", menuName = "Build Status/Jenkins Config")]
    public class JenkinsConfig : ScriptableObject
    {
        [Header("Jenkins Server Settings (Project-wide)")]
        public string jenkinsBaseUrl = "https://jenkins.example.com";
        public string jobName = "MyJob";

        // Add this array to fix your error:
        public string[] jobParameters;
    }
}
