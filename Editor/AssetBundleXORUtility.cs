using UnityEditor;
using UnityEngine;
using System.IO;

namespace AssetBundleBrowser
{
    /// <summary>
    /// Utility class for XOR encryption/decryption of AssetBundles.
    /// Provides menu items for decrypting bundles that were encrypted during build.
    /// </summary>
    public static class AssetBundleXORUtility
    {
        private const string k_MenuPath = "Assets/AssetBundles/XOR/";

        private static string s_StoredKey = "";

        [MenuItem(k_MenuPath + "Encrypt Bundle(s)", false, 1)]
        public static void EncryptSelectedBundles()
        {
            string key = GetOrPromptKey();
            if (string.IsNullOrEmpty(key))
                return;

            ProcessSelectedBundles(key, true);
        }

        [MenuItem(k_MenuPath + "Decrypt Bundle(s)", false, 2)]
        public static void DecryptSelectedBundles()
        {
            string key = GetOrPromptKey();
            if (string.IsNullOrEmpty(key))
                return;

            ProcessSelectedBundles(key, false);
        }

        [MenuItem(k_MenuPath + "Clear Saved Key", false, 100)]
        public static void ClearSavedKey()
        {
            s_StoredKey = "";
            EditorPrefs.DeleteKey("AssetBundleBrowser.XORKey");
            Debug.Log("AssetBundle XOR: Saved key cleared.");
        }

        [MenuItem(k_MenuPath + "Decrypt Bundle(s)", true)]
        [MenuItem(k_MenuPath + "Encrypt Bundle(s)", true)]
        public static bool ValidateSelection()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        private static string GetOrPromptKey()
        {
            // First check session-stored key
            if (!string.IsNullOrEmpty(s_StoredKey))
                return s_StoredKey;

            // Then check editor prefs
            string savedKey = EditorPrefs.GetString("AssetBundleBrowser.XORKey", "");
            if (!string.IsNullOrEmpty(savedKey))
            {
                s_StoredKey = savedKey;
                return savedKey;
            }

            // Prompt user for key
            return PromptForKey();
        }

        private static string PromptForKey()
        {
            // Create a simple input window
            XORKeyInputWindow.ShowWindow((key) =>
            {
                if (!string.IsNullOrEmpty(key))
                {
                    s_StoredKey = key;
                    EditorPrefs.SetString("AssetBundleBrowser.XORKey", key);
                }
            });
            return s_StoredKey;
        }

        private static void ProcessSelectedBundles(string key, bool encrypt)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("AssetBundle XOR: Key cannot be empty.");
                return;
            }

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            int processedCount = 0;
            int failedCount = 0;

            string operation = encrypt ? "encrypted" : "decrypted";

            foreach (var guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.Exists(path))
                    continue;

                string fileName = Path.GetFileName(path);

                // Skip manifest files
                if (fileName.EndsWith(".manifest"))
                    continue;

                try
                {
                    byte[] fileBytes = File.ReadAllBytes(path);

                    // XOR operation (same for encrypt and decrypt)
                    for (int i = 0; i < fileBytes.Length; i++)
                    {
                        fileBytes[i] ^= keyBytes[i % keyBytes.Length];
                    }

                    File.WriteAllBytes(path, fileBytes);
                    processedCount++;
                    Debug.Log($"AssetBundle XOR: Successfully {operation} '{fileName}'.");
                }
                catch (System.Exception e)
                {
                    failedCount++;
                    Debug.LogWarning($"AssetBundle XOR: Failed to {operation.TrimEnd('d')} '{fileName}': {e.Message}");
                }
            }

            AssetDatabase.Refresh();

            if (processedCount > 0)
                Debug.Log($"AssetBundle XOR: {processedCount} bundle(s) {operation}.");
            if (failedCount > 0)
                Debug.LogWarning($"AssetBundle XOR: {failedCount} bundle(s) failed to process.");
        }

        /// <summary>
        /// Decrypts a single bundle file with the given key.
        /// </summary>
        /// <param name="filePath">Path to the bundle file</param>
        /// <param name="key">XOR key</param>
        /// <returns>True if decryption was successful</returns>
        public static bool DecryptBundle(string filePath, string key)
        {
            return ProcessBundle(filePath, key, false);
        }

        /// <summary>
        /// Encrypts a single bundle file with the given key.
        /// </summary>
        /// <param name="filePath">Path to the bundle file</param>
        /// <param name="key">XOR key</param>
        /// <returns>True if encryption was successful</returns>
        public static bool EncryptBundle(string filePath, string key)
        {
            return ProcessBundle(filePath, key, true);
        }

        private static bool ProcessBundle(string filePath, string key, bool encrypt)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("AssetBundle XOR: Key cannot be empty.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                Debug.LogError($"AssetBundle XOR: File not found: {filePath}");
                return false;
            }

            try
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
                byte[] fileBytes = File.ReadAllBytes(filePath);

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    fileBytes[i] ^= keyBytes[i % keyBytes.Length];
                }

                File.WriteAllBytes(filePath, fileBytes);
                string operation = encrypt ? "encrypted" : "decrypted";
                Debug.Log($"AssetBundle XOR: Successfully {operation} '{Path.GetFileName(filePath)}'.");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AssetBundle XOR: Failed to process '{filePath}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypts bundle data in memory. Useful for runtime decryption.
        /// </summary>
        /// <param name="data">Encrypted bundle data</param>
        /// <param name="key">XOR key</param>
        /// <returns>Decrypted data</returns>
        public static byte[] DecryptData(byte[] data, string key)
        {
            if (data == null || data.Length == 0 || string.IsNullOrEmpty(key))
                return data;

            byte[] result = new byte[data.Length];
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return result;
        }

        /// <summary>
        /// Encrypts bundle data in memory.
        /// </summary>
        /// <param name="data">Plain bundle data</param>
        /// <param name="key">XOR key</param>
        /// <returns>Encrypted data</returns>
        public static byte[] EncryptData(byte[] data, string key)
        {
            // XOR encryption and decryption are the same operation
            return DecryptData(data, key);
        }
    }

    /// <summary>
    /// Editor window for XOR key input.
    /// </summary>
    internal class XORKeyInputWindow : EditorWindow
    {
        private string m_Key = "";
        private System.Action<string> m_OnKeyEntered;

        public static void ShowWindow(System.Action<string> onKeyEntered)
        {
            var window = GetWindow<XORKeyInputWindow>(true, "XOR Key Input");
            window.m_OnKeyEntered = onKeyEntered;
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 350, 100);
            window.minSize = new Vector2(350, 100);
            window.maxSize = new Vector2(350, 100);
            window.ShowUtility();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Enter XOR Key:", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.SetNextControlName("KeyField");
            m_Key = EditorGUILayout.PasswordField(m_Key);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.FocusTextInControl("KeyField");

            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("OK", GUILayout.Width(80)))
            {
                m_OnKeyEntered?.Invoke(m_Key);
                Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                Close();
            }

            GUILayout.EndHorizontal();
        }

        void OnDestroy()
        {
            m_OnKeyEntered?.Invoke(null);
        }
    }
}