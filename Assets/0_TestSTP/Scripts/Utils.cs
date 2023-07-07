using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace TestSTP {
    public class Utils : MonoBehaviour {
        /// <summary>
        /// Returns true if the scene 'name' exists and is in your Build settings, false otherwise
        /// </summary>
        public static bool DoesSceneExist(string name) {
            if (string.IsNullOrEmpty(name)) return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/");
                var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

                if (string.Compare(name, sceneName, true) == 0) return true;
            }

            return false;
        }

        public static string GetText(string content) {
            return $"{Application.productName} {Application.version}";
        }

        public static string GetAppVersion() {
            return Application.version;
        }

        public static string GetPlatform() {
            #if UNITY_STANDALONE
            return "PC";
            #elif UNITY_ANDROID
            return "Android";
            #elif UNITY_IOS
            return "iOS";
            #else
            return "Others";
            #endif
        }

        public static string GetDeviceInfo() {
            return
                $"{SystemInfo.deviceName} | {SystemInfo.deviceModel} | {SystemInfo.deviceType} | {SystemInfo.deviceUniqueIdentifier}";
        }

        public static string GetCurentBuild() {
            return "Test";
        }

        public static string SecondsToHourForLand(float seconds) {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
        }

        public static string SecondsToTotalHours(double seconds) {
            var t = TimeSpan.FromSeconds(seconds);
            if (t.Days < 1) {
                return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            } else {
                return $"{Math.Floor(t.TotalHours)}:{t.Minutes:D2}:{t.Seconds:D2}";
            }
        }

        public static bool IsInternetAvailable() {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public static float GetRemainingTime(int startTime, int currentTime, float duration) {
            return duration - (currentTime - startTime);
        }

        public static string StringToSHA1(string content) {
            using var sha1Hash = SHA1.Create();
            var sourceBytes = Encoding.UTF8.GetBytes(content);
            var hashBytes = sha1Hash.ComputeHash(sourceBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            Debug.Log("The SHA1 hash of " + content + " is: " + hash);
            return hash.ToLower();
        }

        public static string SecondsToHour(float seconds) {
            string result = "";
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            if (seconds <= 60) {
                result = $"{t.Minutes:D2}:{t.Seconds:D2}'";
            } else if (seconds < 3600) {
                result = $"{t.Minutes:D2}:{t.Seconds:D2}\"";
            } else {
                result = $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}'";
            }

            return $"{t.Hours:D2}:{t.Minutes:D2}'";
        }
        
        public static string SecondsToHourF2P(float seconds) {
            string result = "";
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            if (seconds < 60) {
                result = $"{t.Seconds:D2}s";
            } else if (seconds < 3600) {
                result = $"{t.Minutes:D2}m";
            } else {
                result = $"{t.Hours:D2}h";
            }

            return result;
        }
        
        public static string SecondsToRacingRecord(float seconds) {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}:{t.Milliseconds:D3}";
        }
        
        public static string MilisecondsToRacingRecord(long miliseconds) {
            TimeSpan t = TimeSpan.FromMilliseconds(miliseconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}:{t.Milliseconds:D3}";
        }
        
        public static string MilisecondsToRacingRecord(double miliseconds) {
            TimeSpan t = TimeSpan.FromMilliseconds(miliseconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}:{t.Milliseconds:D3}";
        }

        public static int GetCurrentUnixTime() {
            return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static int GetDayLeft(int endTime) {
            var timeLeft = UnixToDateTime(double.Parse(endTime.ToString())).Subtract(DateTime.Now);
            return timeLeft.Days;
        }
        
        public static double GetHourLeft(int endTime) {
            var timeLeft = UnixToDateTime(double.Parse(endTime.ToString())).Subtract(DateTime.Now);
            return timeLeft.TotalHours;
        }
        
        public static double GetHourMinutesLeft(int endTime) {
            var timeLeft = UnixToDateTime(double.Parse(endTime.ToString())).Subtract(DateTime.Now);
            return timeLeft.TotalHours;
        }

        public static DateTime UnixToLocalDateTime(double unixTimeStamp) {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        
        public static DateTime UnixToLocalDateTimeRanking(double unixTimeStamp) {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime UnixToDateTime(double unixTimeStamp) {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        public static async Task<Texture2D> GetRemoteTexture(string url) {
            using var www = UnityWebRequestTexture.GetTexture(url);
            // begin request:
            var asyncOp = www.SendWebRequest();

            // await until it's done: 
            while (asyncOp.isDone == false) await Task.Delay(1000 / 30); //30 hertz

            // read results:
            if (www.result == UnityWebRequest.Result.Success) return DownloadHandlerTexture.GetContent(www);
            // log error:
#if DEBUG
            Debug.Log($"{www.error}, URL:{www.url}");
#endif

            // nothing to return on error:
            return null;
            // return valid results:
        }

        public static void WriteString(string fileName, string content) {
            string path = $"Assets/{fileName}.txt";
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(content);
            writer.Close();
        }

        public static bool IsEmulator() {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")
                .Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass cls = new AndroidJavaClass("com.nekolaboratory.EmulatorDetector");
            bool result = cls.CallStatic<bool>("isEmulator", context);
            if (result) {
                return true;
            }

            var osBuild = new AndroidJavaClass("android.os.Build");
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            return fingerPrint.Contains("generic");
#endif
            return false;
        }

        public static bool IsJailbroken() {
#if UNITY_IOS && !UNITY_EDITOR
            string[] paths = new string[10] {
                "/Applications/Cydia.app",
                "/private/var/lib/cydia",
                "/private/var/tmp/cydia.log",
                "/System/Library/LaunchDaemons/com.saurik.Cydia.Startup.plist",
                "/usr/libexec/sftp-server",
                "/usr/bin/sshd",
                "/usr/sbin/sshd",
                "/Applications/FakeCarrier.app",
                "/Applications/SBSettings.app",
                "/Applications/WinterBoard.app",
            };
            int i;
            bool jailbroken = false;
            for (i = 0; i < paths.Length; i++) {
                if (System.IO.File.Exists(paths[i])) {
                    jailbroken = true;
                }
            }

            return jailbroken;
#endif

            return false;
        }

        public static string GetData() {
            string result = "";

            if (Application.platform == RuntimePlatform.Android) {
                var osBuild = new AndroidJavaClass("android.os.Build");
                string brand = osBuild.GetStatic<string>("BRAND");
                string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
                string model = osBuild.GetStatic<string>("MODEL");
                string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
                string device = osBuild.GetStatic<string>("DEVICE");
                string product = osBuild.GetStatic<string>("PRODUCT");

                result += Application.installerName;
                result += "/";
                result += Application.installMode.ToString();
                result += "/";
                result += Application.buildGUID;
                result += "/";
                result += "Genuine :" + Application.genuine;
                result += "/";
                result += "Rooted : " + IsAndroidRooted();
                result += "/";
                result += "Emulator : " + IsEmulator();
                result += "/";
                result += "Model : " + model;
                result += "/";
                result += "Menufacturer : " + menufacturer;
                result += "/";
                result += "Device : " + device;
                result += "/";
                result += "Fingerprint : " + fingerPrint;
                result += "/";
                result += "Product : " + product;
            } else {
                result += Application.installerName;
                result += "/";
                result += Application.installMode.ToString();
                result += "/";
                result += Application.buildGUID;
                result += "/";
                result += "Genuine :" + Application.genuine;
                result += "/";
            }

            return result;
        }
        
        public static bool IsAndroidRooted() {
#if UNITY_ANDROID && !UNITY_EDITOR
            var isRoot = false;

            if (isRootedPrivate("/system/bin/su")) isRoot = true;
            if (isRootedPrivate("/system/xbin/su")) isRoot = true;
            if (isRootedPrivate("/system/app/SuperUser.apk")) isRoot = true;
            if (isRootedPrivate("/data/data/com.noshufou.android.su")) isRoot = true;
            if (isRootedPrivate("/sbin/su")) isRoot = true;

            return isRoot;
#endif
            return false;
        }

        public static bool IsEmulator2() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.Android) {
                var osBuild = new AndroidJavaClass("android.os.Build");
                string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
                string model = osBuild.GetStatic<string>("MODEL");
                string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
                string brand = osBuild.GetStatic<string>("BRAND");
                string device = osBuild.GetStatic<string>("DEVICE");
                string product = osBuild.GetStatic<string>("PRODUCT");

                return fingerPrint.Contains("generic")
                       || fingerPrint.Contains("unknown")
                       || model.Contains("google_sdk")
                       || model.Contains("Emulator")
                       || model.Contains("Android SDK built for x86")
                       || menufacturer.Contains("Genymotion")
                       || (brand.Contains("generic") && device.Contains("generic"))
                       || product.Equals("google_sdk")
                       || product.Equals("unknown");
            }
#endif
            return false;
        }

        public static bool IsMailValid(string mail) {
            // var validateEmailRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
            // return validateEmailRegex.IsMatch(mail);
            return Regex.IsMatch(mail, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static bool isRootedPrivate(string path) {
            return File.Exists(path);
        }

        public static void GoUrl(string url) {
            Application.OpenURL(url);
        }
        
        public static void SaveFile(string filename, string folderPath, string content) {
            if (!File.Exists(folderPath)) {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            
            string path = folderPath + filename;
            File.WriteAllText(path, content);
            // var writer = new StreamWriter(path, false);
            // writer.WriteLine(content);
            // writer.Close();
            // if (!File.Exists(path)) {
            //     using var sw = File.CreateText(path);
            //     sw.WriteLine(content);
            //     sw.Close();
            // } else {
            //     var writer = new StreamWriter(path, false);
            //     writer.WriteLine(content);
            //     writer.Close();
            // }
            
            Debug.Log($"Saved file to {path}");
        }

        public static string OpenFile(string filename) {
            string path = Application.persistentDataPath + "/record/" + filename;
            var content = "";
            using StreamReader sr = File.OpenText(path);
            while (sr.ReadLine() is { } s) {
                content += s;
            }

            return content;
        }
        
        public static T Deserialize<T>(byte[] data) where T : class {
            using var stream = new MemoryStream(data);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

        public static string RemoveSuffixString(string source, string from) {
            var index = source.IndexOf(from, StringComparison.Ordinal);
            return index <= 0 ? source : source[..index];
        }
    }
}