using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System;

namespace UnityBuildStatusJenkins
{
    public class UnityBuildStatusJenkinsWindow : EditorWindow
    {
        private JenkinsConfig config;

        // User credentials - saved in EditorPrefs
        private string username;
        private string apiToken;

        private string buildStatus = "UNKNOWN";
        private Color statusColor = Color.gray;

        private const string UsernamePrefKey = "UBSJ_Username";
        private const string ApiTokenPrefKey = "UBSJ_ApiToken";

        private double nextRefreshTime = 0;
        private const double refreshIntervalSeconds = 30.0;

        [MenuItem("Window/Build Status/Jenkins")]
        public static void ShowWindow()
        {
            _ = GetWindow<UnityBuildStatusJenkinsWindow>("Jenkins Status");
        }

        private void OnEnable()
        {
            // Load JenkinsConfig asset from the project
            var guids = AssetDatabase.FindAssets("t:JenkinsConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<JenkinsConfig>(path);
            }
            else
            {
                Debug.LogWarning("No JenkinsConfig asset found in project. Please create one.");
            }

            // Load user credentials from EditorPrefs
            username = EditorPrefs.GetString(UsernamePrefKey, "");
            apiToken = EditorPrefs.GetString(ApiTokenPrefKey, "");

            EditorApplication.update += OnEditorUpdate;

            _ = CheckStatusAsync();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup >= nextRefreshTime)
            {
                nextRefreshTime = EditorApplication.timeSinceStartup + refreshIntervalSeconds;
                _ = CheckStatusAsync();
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Project Settings (Shared)", EditorStyles.boldLabel);

            if (config == null)
            {
                EditorGUILayout.HelpBox("No JenkinsConfig asset found! Create one under Assets → Create → Build Status → Jenkins Config.", MessageType.Error);
                if (GUILayout.Button("Create JenkinsConfig"))
                {
                    CreateConfigAsset();
                }
                return;
            }

            // Show project settings as read-only fields
            EditorGUI.BeginDisabledGroup(true);
            _ = EditorGUILayout.TextField("Jenkins Base URL", config.jenkinsBaseUrl);
            _ = EditorGUILayout.TextField("Job Name", config.jobName);
            if (config.jobParameters != null && config.jobParameters.Length > 0)
            {
                GUILayout.Label("Job Parameters:");
                foreach (var param in config.jobParameters)
                {
                    EditorGUILayout.LabelField("- " + param);
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(15);

            GUILayout.Label("Your Credentials (User-specific)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            username = EditorGUILayout.TextField("Username", username);
            apiToken = EditorGUILayout.PasswordField("API Token", apiToken);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(UsernamePrefKey, username);
                EditorPrefs.SetString(ApiTokenPrefKey, apiToken);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Refresh Status"))
            {
                _ = CheckStatusAsync();
            }

            GUILayout.Space(10);

            // Draw the colored status indicator box
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(50, 50), statusColor);
            GUILayout.Label("Status: " + buildStatus);
        }

        private string GetJobApiUrl()
        {
            if (config == null)
            {
                return "";
            }
            // Add parameter support here if needed
            return $"{config.jenkinsBaseUrl}/job/{config.jobName}/lastBuild/api/json";
        }

        private async Task CheckStatusAsync()
        {
            if (config == null)
            {
                buildStatus = "No config!";
                statusColor = Color.magenta;
                Repaint();
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(apiToken))
            {
                buildStatus = "Set username & token!";
                statusColor = Color.magenta;
                Repaint();
                return;
            }

            try
            {
                using HttpClient client = new();
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                HttpResponseMessage response = await client.GetAsync(GetJobApiUrl());

                if (!response.IsSuccessStatusCode)
                {
                    buildStatus = $"HTTP Error: {response.StatusCode}";
                    statusColor = Color.magenta;
                    Repaint();
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                if (json.Contains("\"result\":\"SUCCESS\""))
                {
                    statusColor = Color.green;
                    buildStatus = "SUCCESS";
                }
                else if (json.Contains("\"result\":\"FAILURE\""))
                {
                    statusColor = Color.red;
                    buildStatus = "FAILURE";
                }
                else if (json.Contains("\"building\":true"))
                {
                    statusColor = Color.yellow;
                    buildStatus = "BUILDING";
                }
                else
                {
                    statusColor = Color.gray;
                    buildStatus = "UNKNOWN";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error fetching Jenkins status: " + ex.Message);
                statusColor = Color.magenta;
                buildStatus = "ERROR";
            }

            Repaint();
        }

        private void CreateConfigAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create JenkinsConfig", "JenkinsConfig", "asset", "Save Jenkins config asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            JenkinsConfig newConfig = CreateInstance<JenkinsConfig>();
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            config = newConfig;
        }
    }
}
