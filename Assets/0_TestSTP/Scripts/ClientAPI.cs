using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace TestSTP
{
    public static class ClientAPI
    {

        const string APIEndPoint = "http://27.72.98.184:1688/v1/";
        
        public static string URL = "http://27.72.98.184:1688/";
        
        public static string APIToken = "";

        public static string AccessToken { get; private set; }

        public static void SetAccessToken(string token)
        {
            AccessToken = token;
        }

        private static MonoBehaviour WebHelper
        {
            get
            {
                return GameServiceManager.Instance;
            }
        }

        #region APIs
    
    
    
        public static void UserLogin(string userEmail, string userPassword, string endpoint, Action<LoginDataRes> OnSuccess, Action<int, string> OnError)
        {
            Debug.Log($"LoginRequest: {userEmail}");
            if(string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword)) return;
            var request = new LoginInfo() { email = userEmail, password =  userPassword};
            var data = JsonConvert.SerializeObject(request);
            var webRq = CreateWebRequestWithData($"{endpoint}", data, HTTPMethods.Post);
            WebHelper.StartCoroutine(SendWebRequest(webRq, OnSuccess, OnError));
        }
        
        public static void GetRequest<T>(string endpoint, Action<T> OnSuccess, Action<int, string> OnError)
        {
            var webRq = CreateWebRequest($"{endpoint}", HTTPMethods.Get);
            WebHelper.StartCoroutine(SendWebRequest(webRq, OnSuccess, OnError));
        }

        #endregion

        #region Others
        public static void DownloadTexture(string url, Action<Texture2D> OnSuccess, Action<int, string> OnError = null)
        {
            UnityWebRequest webRq = UnityWebRequestTexture.GetTexture(url);
            WebHelper.StartCoroutine(SendWebRequest(webRq, () =>
            {
                var myTexture = DownloadHandlerTexture.GetContent(webRq);
                OnSuccess?.Invoke(myTexture);
            }, OnError));
        }


        private static string KEY = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG"; // pick some other 32 chars
        private static byte[] KEY_BYTES = Encoding.UTF8.GetBytes(KEY);
        public static string Encrypt(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (Rijndael algorithm = Rijndael.Create())
            {
                algorithm.Key = KEY_BYTES;

                // Create a decrytor to perform the stream transform.
                var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write IV first
                            msEncrypt.Write(algorithm.IV, 0, algorithm.IV.Length);
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return Convert.ToBase64String(encrypted);
        }
        static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }
            Debug.LogError(plainText);
            // Return the decrypted data as a string
            return plainText;
        }

        public static string DecryptString(string strInput)
        {
            string password = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG";
            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = Encoding.ASCII.GetBytes(password);
            // Create secret IV
            byte[] iv = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOP");

            return DecryptString(strInput, key, iv);
        }
        public static string EncryptString(string plainText)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;
            string password = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG";
            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            encryptor.Key = Encoding.ASCII.GetBytes(password);
            encryptor.IV = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOP");
            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            // Convert the plainText string into a byte array
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert the encrypted byte array to a base64 encoded string
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            // Return the encrypted data as a string
            return cipherText;
        }
        #endregion


        #region Common
        private static IEnumerator SendWebRequest<T>(UnityWebRequest request, Action<T> OnSuccess, Action<int, string> OnError)
        {
            yield return request.SendWebRequest();

            OnRequestFinished(request, OnSuccess, OnError);
        }

        private static IEnumerator SendWebRequest(UnityWebRequest request, Action OnSuccess, Action<int, string> OnError)
        {
            yield return request.SendWebRequest();

            OnRequestFinished(request, OnSuccess, OnError);
        }

        private static void OnRequestFinished<T>(UnityWebRequest request, Action<T> OnSucces, Action<int, string> OnError)
        {
            switch (request.result)
            {
                // The request finished without any problem.
                case UnityWebRequest.Result.Success:
#if !NO_LOGS
                    Debug.Log(request.downloadHandler.text);
#endif
                    if (string.IsNullOrEmpty(request.error))
                    {
                        string strData = request.downloadHandler.text;
                        // Everything went as expected!
                        try
                        {
                            var result = JsonConvert.DeserializeObject<T>(strData);

                            OnSucces?.Invoke(result);
                        }
                        catch (JsonException)
                        {
                            Debug.Log(request.downloadHandler.text);
                            Debug.Log(request.url + " \nSerializable Error !");
                            OnError?.Invoke(1, "Serializable Error !");
                        }
                    }
                    else
                    {
                        Debug.LogError(request.url + " \n" + request.responseCode + " " + request.error);
                        OnError?.Invoke((int)request.responseCode, request.error);
                    }
                    break;

                default:
                    Debug.LogError(request.url + " \n" + request.responseCode + " \n" + request.error + " \n" + request.downloadHandler.text);
                    string error = request.downloadHandler.text;
                    ErrorMessage errorContent = null;

                    try
                    {
                        errorContent = JsonConvert.DeserializeObject<ErrorMessage>(error);
                    }
                    catch (Exception) { }

                    if (errorContent != null)
                        error = errorContent.errorMessage;
                    else
                        error = request.error;

                    OnError?.Invoke((int)request.responseCode, error);
                    break;
            }
            request.Dispose();
        }

        private static void OnRequestFinished(UnityWebRequest request, Action OnSucces, Action<int, string> OnError)
        {
            switch (request.result)
            {
                // The request finished without any problem.
                case UnityWebRequest.Result.Success:
#if !NO_LOGS
                    Debug.Log(request.downloadHandler.text);
#endif
                    if (string.IsNullOrEmpty(request.error))
                    {
                        OnSucces?.Invoke();
                    }
                    else
                    {
                        Debug.LogError(request.url + " \n" + request.responseCode + " " + request.error);
                        OnError?.Invoke((int)request.responseCode, request.error);
                    }
                    break;

                default:
                    Debug.LogError(request.url + " \n" + request.responseCode + " \n" + request.error + " \n" + request.downloadHandler.text);
                    string error = request.downloadHandler.text;
                    ErrorMessage errorContent = null;

                    try
                    {
                        errorContent = JsonConvert.DeserializeObject<ErrorMessage>(error);
                    }
                    catch (Exception) { }

                    if (errorContent != null)
                        error = errorContent.errorMessage;
                    else
                        error = request.error;

                    OnError?.Invoke((int)request.responseCode, error);
                    break;
            }
        }

        private static UnityWebRequest CreateWebRequest(string endPoint, HTTPMethods methodType)
        {
            var request = new UnityWebRequest($"{APIEndPoint}{endPoint}", methodType.ToString());
            request.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(AccessToken))
                request.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            if (!string.IsNullOrEmpty(APIToken))
            {
                request.SetRequestHeader("token", APIToken);
            }
            return request;
        }

        private static UnityWebRequest CreateWebRequestWithData(string endPoint, string json, HTTPMethods methodType)
        {
            var request = CreateWebRequest(endPoint, methodType);
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            return request;
        }

        #endregion



        #region Models



        public class LoginInfo
        {
            public string email;
            public string password;
        }
        
        public class TesterData
        {
            public int id;
            public string name;

        }


        public class LoginResult
        {
            public string accessToken;
        }

        public class UpdateVersionResult
        {
            public int success;
            public UpdateVersionData data;
        }

        public class UpdateVersionData
        {
            public List<TableVersion> versions;
        }

        public class TableVersion
        {
            public string name;
            public int version;
        }

    
        public class ErrorMessage
        {
            public string error;
            public string errorCode;
            public string errorMessage;
        }



        #endregion

        public enum HTTPMethods : byte
        {
            Get,
            Head,
            Post,
            Put,
            Delete,
            Patch,
            Merge,
            Options,
            Connect
        }
    }
}

