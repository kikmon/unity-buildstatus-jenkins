using UnityEngine.UIElements;
using UnityEditor;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using UnityEngine;

namespace UnityBuildStatusJenkins
{
    public class JenkinsToolbarUI : VisualElement
    {
        private readonly Label statusLabel;
        private readonly VisualElement colorIndicator;

        private double nextCall;
        private const double interval = 30.0;
        private readonly JenkinsConfig config;
        private readonly string username;
        private readonly string apiToken;

        public JenkinsToolbarUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            colorIndicator = new VisualElement();
            colorIndicator.style.width = 12;
            colorIndicator.style.height = 12;
            colorIndicator.style.marginRight = 4;
            Add(colorIndicator);

            statusLabel = new Label("Jenkins: UNKNOWN");
            Add(statusLabel);

            nextCall = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;

            config = LoadConfig();
            username = EditorPrefs.GetString("UBSJ_Username", "");
            apiToken = EditorPrefs.GetString("UBSJ_ApiToken", "");
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup >= nextCall)
            {
                nextCall = EditorApplication.timeSinceStartup + interval;
                _ = UpdateStatusAsync();
            }
        }

        private JenkinsConfig LoadConfig()
        {
            var guids = AssetDatabase.FindAssets("t:JenkinsConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<JenkinsConfig>(path);
            }
            return null;
        }

        private async Task UpdateStatusAsync()
        {
            if (config == null)
            {
                SetStatus("No config", Color.magenta);
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(apiToken))
            {
                SetStatus("Set creds", Color.magenta);
                return;
            }

            try
            {
                using HttpClient client = new();
                var auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

                HttpResponseMessage resp = await client.GetAsync($"{config.jenkinsBaseUrl}/job/{config.jobName}/lastBuild/api/json");
                if (!resp.IsSuccessStatusCode)
                {
                    SetStatus($"HTTP {resp.StatusCode}", Color.magenta);
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                if (json.Contains("\"result\":\"SUCCESS\""))
                {
                    SetStatus("SUCCESS", Color.green);
                }
                else if (json.Contains("\"result\":\"FAILURE\""))
                {
                    SetStatus("FAILURE", Color.red);
                }
                else if (json.Contains("\"building\":true"))
                {
                    SetStatus("BUILDING", Color.yellow);
                }
                else
                {
                    SetStatus("UNKNOWN", Color.gray);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Jenkins fetch error: " + e.Message);
                SetStatus("ERROR", Color.magenta);
            }
        }

        private void SetStatus(string text, Color color)
        {
            statusLabel.text = "Jenkins: " + text;
            colorIndicator.style.backgroundColor = new StyleColor(color);
        }
    }
}
