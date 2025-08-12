using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
/*
namespace UnityBuildStatusJenkins
{
    [EditorToolbarElement("UnityBuildStatusJenkins.JenkinsStatus")]
    public class JenkinsToolbarStatus : EditorToolbarElement
    {
        private Color statusColor = Color.gray;
        private string statusText = "UNKNOWN";

        private JenkinsConfig config;
        private string username;
        private string apiToken;

        private const string UsernamePrefKey = "UBSJ_Username";
        private const string ApiTokenPrefKey = "UBSJ_ApiToken";

        private double nextRefreshTime = 0;
        private const double refreshIntervalSeconds = 30.0;

        public JenkinsToolbarStatus()
        {
            config = LoadConfig();
            username = EditorPrefs.GetString(UsernamePrefKey, "");
            apiToken = EditorPrefs.GetString(ApiTokenPrefKey, "");

            nextRefreshTime = EditorApplication.timeSinceStartup;
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
                Repaint();
            }
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();

            Rect rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16), GUILayout.Height(16));
            EditorGUI.DrawRect(rect, statusColor);

            GUILayout.Space(4);
            GUILayout.Label(statusText);

            GUILayout.EndHorizontal();
        }

        private JenkinsConfig LoadConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:JenkinsConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<JenkinsConfig>(path);
            }
            return null;
        }

        private async Task CheckStatusAsync()
        {
            if (config == null)
            {
                statusText = "No config!";
                statusColor = Color.magenta;
                Repaint();
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(apiToken))
            {
                statusText = "Set username & token!";
                statusColor = Color.magenta;
                Repaint();
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                    var response = await client.GetAsync($"{config.jenkinsBaseUrl}/job/{config.jobName}/lastBuild/api/json");

                    if (!response.IsSuccessStatusCode)
                    {
                        statusText = $"HTTP {response.StatusCode}";
                        statusColor = Color.magenta;
                        Repaint();
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    if (json.Contains("\"result\":\"SUCCESS\""))
                    {
                        statusColor = Color.green;
                        statusText = "SUCCESS";
                    }
                    else if (json.Contains("\"result\":\"FAILURE\""))
                    {
                        statusColor = Color.red;
                        statusText = "FAILURE";
                    }
                    else if (json.Contains("\"building\":true"))
                    {
                        statusColor = Color.yellow;
                        statusText = "BUILDING";
                    }
                    else
                    {
                        statusColor = Color.gray;
                        statusText = "UNKNOWN";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error fetching Jenkins status: " + e.Message);
                statusColor = Color.magenta;
                statusText = "ERROR";
            }

            Repaint();
        }
    }
}

*/