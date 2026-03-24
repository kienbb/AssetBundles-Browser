using System.IO;

namespace AssetBundleBrowser
{
    /// <summary>
    /// Runtime utility for XOR decryption of AssetBundles.
    /// Use this class in your game to decrypt bundles that were encrypted during build.
    ///
    /// Usage examples:
    ///
    /// 1. Decrypt a file on disk before loading:
    /// <code>
    ///   AssetBundleXORRuntime.DecryptFile("path/to/bundle", "your_secret_key");
    ///   var bundle = AssetBundle.LoadFromFile("path/to/bundle");
    /// </code>
    ///
    /// 2. Decrypt data in memory (for use with custom loading):
    /// <code>
    ///   byte[] encryptedData = File.ReadAllBytes("path/to/bundle");
    ///   byte[] decryptedData = AssetBundleXORRuntime.DecryptData(encryptedData, "your_secret_key");
    ///   var bundle = AssetBundle.LoadFromMemory(decryptedData);
    /// </code>
    /// </summary>
    public static class AssetBundleXORRuntime
    {
        /// <summary>
        /// Decrypts an encrypted bundle file on disk.
        /// The file is modified in place.
        /// </summary>
        /// <param name="filePath">Path to the encrypted bundle file</param>
        /// <param name="key">The XOR key used during encryption</param>
        /// <returns>True if decryption was successful</returns>
        public static bool DecryptFile(string filePath, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                UnityEngine.Debug.LogError("AssetBundleXOR: Key cannot be empty.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogError($"AssetBundleXOR: File not found: {filePath}");
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
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"AssetBundleXOR: Failed to decrypt '{filePath}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypts encrypted bundle data in memory.
        /// Use this when you want to load from memory without modifying the original file.
        /// </summary>
        /// <param name="data">Encrypted bundle data</param>
        /// <param name="key">The XOR key used during encryption</param>
        /// <returns>Decrypted data (new array, original is not modified)</returns>
        public static byte[] DecryptData(byte[] data, string key)
        {
            if (data == null || data.Length == 0)
            {
                UnityEngine.Debug.LogError("AssetBundleXOR: Data is null or empty.");
                return null;
            }

            if (string.IsNullOrEmpty(key))
            {
                UnityEngine.Debug.LogError("AssetBundleXOR: Key cannot be empty.");
                return null;
            }

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
        /// Note: XOR encryption and decryption are the same operation.
        /// </summary>
        /// <param name="data">Plain bundle data</param>
        /// <param name="key">The XOR key</param>
        /// <returns>Encrypted data (new array, original is not modified)</returns>
        public static byte[] EncryptData(byte[] data, string key)
        {
            return DecryptData(data, key);
        }

        /// <summary>
        /// Encrypts a bundle file on disk.
        /// The file is modified in place.
        /// </summary>
        /// <param name="filePath">Path to the bundle file</param>
        /// <param name="key">The XOR key</param>
        /// <returns>True if encryption was successful</returns>
        public static bool EncryptFile(string filePath, string key)
        {
            return DecryptFile(filePath, key);
        }
    }
}