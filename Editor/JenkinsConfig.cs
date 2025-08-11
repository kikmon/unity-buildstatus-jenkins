using UnityEngine;

namespace UnityBuildStatusJenkins
{
    [CreateAssetMenu(fileName = "JenkinsConfig", menuName = "Build Status/Jenkins Config")]
    public class JenkinsConfig : ScriptableObject
    {
        [Header("Jenkins Server Settings")]
        public string jenkinsBaseUrl = "https://jenkins.example.com";
        public string jobName = "MyJob";
    }
}
