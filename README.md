# Unity Asset Bundle Browser

An enhanced Unity Editor tool for managing and inspecting AssetBundles.

> **Note:** This is an enhanced fork of the original Unity Asset Bundle Browser with additional features.

## Features

### Configure Tab
Assign assets and scenes to bundles through a visual interface. Similar to using the AssetBundle control at the bottom of the Inspector, but with a dedicated management view.

### Build Tab
Build AssetBundles with extended options:
- **Build Target** - Select target platform
- **Bundle Selection** - Build all bundles or a single bundle
- **File Extension** - Append a custom extension to output files (e.g., `.bundle`)
- **XOR Encryption** - Simple XOR encryption for basic bundle obfuscation
- **Compression** - None, LZMA (Standard), or LZ4 (Chunk Based)
- **Advanced Options** - Exclude type info, force rebuild, strict mode, dry run, etc.
- **Copy to StreamingAssets** - Automatically copy builds for standalone player use

### Inspect Tab
View contents of built AssetBundles:
- Load individual files or entire folders
- Decrypt and inspect XOR-encrypted bundles
- View bundle contents and asset details

## XOR Encryption

This fork includes simple XOR encryption for AssetBundles:

### Building Encrypted Bundles
1. In the Build tab, enable **XOR Encryption**
2. Enter your encryption key
3. Click Build

### Inspecting Encrypted Bundles
1. In the Inspect tab, enable **Decrypt XOR**
2. Enter the same key used during build
3. Add and select bundles to inspect

### Runtime Decryption
Use `AssetBundleXORRuntime` class in your game code:

```csharp
using AssetBundleBrowser;

// Decrypt file on disk before loading
AssetBundleXORRuntime.DecryptFile(bundlePath, "your_key");
var bundle = AssetBundle.LoadFromFile(bundlePath);

// Or decrypt in memory
byte[] encryptedData = File.ReadAllBytes(bundlePath);
byte[] decryptedData = AssetBundleXORRuntime.DecryptData(encryptedData, "your_key");
var bundle = AssetBundle.LoadFromMemory(decryptedData);
```

> **Security Note:** XOR encryption provides basic obfuscation only. It prevents casual inspection but should not be relied upon for sensitive data.

### Editor Menu Tools
Right-click bundle files in Project view:
- `Assets > AssetBundles > XOR > Encrypt Bundle(s)`
- `Assets > AssetBundles > XOR > Decrypt Bundle(s)`

## Installation

### Via Git URL (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the **+** button (top left)
3. Select **Add package from git URL...**
4. Enter: `https://github.com/kienbb/AssetBundles-Browser.git`
5. Click **Add**

### Via manifest.json
Add to your `Packages/manifest.json`:
```json
"com.unity.assetbundlebrowser": "https://github.com/kienbb/AssetBundles-Browser.git"
```

## Usage

After installation, access the tool via **Window > AssetBundle Browser**.

## Alternatives

- [Addressables package](https://docs.unity3d.com/Packages/com.unity.addressables@latest) - Recommended for new projects
- [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools) - Alternative bundle inspection

## Documentation

See the [full documentation](Documentation/com.unity.assetbundlebrowser.md) for more details.

## License

See [license.md](license.md) for license information.