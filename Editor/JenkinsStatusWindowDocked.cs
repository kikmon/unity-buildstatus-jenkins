using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace UnityBuildStatusJenkins
{
    public class JenkinsStatusWindowDocked : EditorWindow
    {
        private Label statusLabel;
        private VisualElement colorIndicator;
        private double nextCall;
        private const double interval = 30.0;

        private JenkinsConfig config;
        private string username;
        private string apiToken;

        [MenuItem("Window/Build Status/Jenkins Dock")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<JenkinsStatusWindowDocked>("Jenkins Status");
            wnd.minSize = new Vector2(200, 40);
        }

        private void CreateGUI()
        {
            colorIndicator = new VisualElement();
            colorIndicator.style.width = 12;
            colorIndicator.style.height = 12;
            colorIndicator.style.marginRight = 4;

            statusLabel = new Label("Jenkins: UNKNOWN");

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingTop = 4,
                    paddingLeft = 4,
                    paddingBottom = 4
                }
            };

            container.Add(colorIndicator);
            container.Add(statusLabel);

            rootVisualElement.Add(container);

            config = LoadConfig();
            username = EditorPrefs.GetString("UBSJ_Username", "");
            apiToken = EditorPrefs.GetString("UBSJ_ApiToken", "");
            nextCall = EditorApplication.timeSinceStartup;

            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
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
                using var client = new HttpClient();
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

                var response = await client.GetAsync($"{config.jenkinsBaseUrl}/job/{config.jobName}/lastBuild/api/json");
                if (!response.IsSuccessStatusCode)
                {
                    SetStatus($"HTTP {response.StatusCode}", Color.magenta);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("\"result\":\"SUCCESS\""))
                    SetStatus("SUCCESS", Color.green);
                else if (json.Contains("\"result\":\"FAILURE\""))
                    SetStatus("FAILURE", Color.red);
                else if (json.Contains("\"building\":true"))
                    SetStatus("BUILDING", Color.yellow);
                else
                    SetStatus("UNKNOWN", Color.gray);
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
