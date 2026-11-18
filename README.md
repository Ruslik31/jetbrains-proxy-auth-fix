# JetBrains & Android Studio Proxy Authentication Fix

Fix and universal GUI utility for the infamous **recurring Proxy Authentication dialog bug** across all IntelliJ Platform IDEs (**Android Studio**, **IntelliJ IDEA**, **PyCharm**, **WebStorm**, **CLion**, **Rider**, **GoLand**, **PhpStorm**, **DataGrip**, **RustRover**).

## 🐛 The Problem
When using an HTTP proxy without authentication (or transparent/local proxies like Clash, v2ray, Xray, Charles, Fiddler), JetBrains IDEs repeatedly pop up a **"Proxy Authentication"** modal on every background network request (AI Assistant, Gemini, Gradle Sync, plugins, updates).

Checking **"Remember password"** fails to persist credentials due to upstream platform bugs in `IdeProxyAuthenticator`.

**Upstream Issues:**
* JetBrains YouTrack: [IJPL-37161](https://youtrack.jetbrains.com/issue/IJPL-37161)
* JetBrains YouTrack: [IJPL-233618](https://youtrack.jetbrains.com/issue/IJPL-233618)
* JetBrains YouTrack: [IJPL-248850](https://youtrack.jetbrains.com/issue/IJPL-248850)
* Google Issue Tracker: [Issue 348084787](https://issuetracker.google.com/issues/348084787)
* Official Pull Request: [JetBrains/intellij-community#3770](https://github.com/JetBrains/intellij-community/pull/3770)

---

## 🚀 The Fix

### Option 1: Universal One-Click GUI Tool (Recommended)
Download **`JetBrains_Proxy_Fix.exe`** (standalone, no installation required):
1. Run `JetBrains_Proxy_Fix.exe`.
2. Select your installed IDEs from the checklist.
3. Verify your proxy HTTP port (default `10809` for v2ray/Xray, `7890` for Clash).
4. Click **"Apply Fix"**.
5. In your IDE settings: **Settings (`Ctrl + Alt + S`) → System Settings → HTTP Proxy → select "No proxy"**.
6. Restart the IDE.

### Option 2: Manual Configuration
Add JVM arguments directly to `<product>64.exe.vmoptions` (**Help → Edit Custom VM Options...**):
```properties
-Dhttp.proxyHost=127.0.0.1
-Dhttp.proxyPort=10809
-Dhttps.proxyHost=127.0.0.1
-Dhttps.proxyPort=10809
-Dhttp.nonProxyHosts=localhost|127.0.0.1
-Djdk.http.auth.tunneling.disabledSchemes=""
-Djdk.http.auth.proxying.disabledSchemes=""
```
