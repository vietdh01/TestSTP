using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestSTP
{
    public class GameServiceManager : Singleton<GameServiceManager>
    {

        public static bool IsNetworkAvaible
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }
        
        public Action<int, string> EventOnResponseError { get; set; }
        
        public Action<ItemsInUse> EventOnGetItemsInUse { get; set; }
        
        public LoginDataRes loginData { get; private set; }

        public List<ItemData> configItems { get; private set; }
        
        
        public void UserLogin(string email, string pass)
        {

            ClientAPI.UserLogin(email, pass,"auth/login", OnLoginSuccess, (errorCode, error) =>
            {
                OnResponseError(errorCode, error);
            });
        }
        
        private void OnLoginSuccess(LoginDataRes dataRes)
        {
            if (!string.IsNullOrEmpty(dataRes.accessToken))
            {
                loginData = dataRes;
                ClientAPI.SetAccessToken(dataRes.accessToken);
                SceneManager.LoadScene("Main");
            }
            else
            {
                EventOnResponseError?.Invoke((int)dataRes.errorStatus.errorCode, dataRes.msg);
            }
        }
        
        private void OnResponseError(int errorCode, string error)
        {
            EventOnResponseError?.Invoke(errorCode, error);
        }
        
        public void GetItemsInUse()
        {

            ClientAPI.GetRequest<ItemsInUse>("user/items/inUse", GetItemsInUseSuccess, (errorCode, error) =>
            {
                OnResponseError(errorCode, error);
            });
        }
        
        public void GetItemsConfig()
        {

            ClientAPI.GetRequest<List<ItemData>>("item/config", GetItemsConfigSuccess, (errorCode, error) =>
            {
                OnResponseError(errorCode, error);
            });
        }

        private void GetItemsInUseSuccess(ItemsInUse dataRes)
        {
            if (dataRes != null)
            {
                EventOnGetItemsInUse?.Invoke(dataRes);
            }
            else
            {
                EventOnResponseError?.Invoke(0, "GetItemsInUseSuccess:  Khong co Data");
            }
        }

        private void GetItemsConfigSuccess(List<ItemData> items)
        {
            if (items != null)
            {
                configItems = items;
            }
            else
            {
                EventOnResponseError?.Invoke(0, "GetItemsConfigSuccess: Khong co Data");
            }
        }

        public ItemData FindItemData(string id)
        {
            return configItems.Find(i => i.id == id);
        }
    }
    
    [Serializable]
    
    public class LoginDataRes
    {
        public string accessToken { get; set; }
        
        public string refreshToken { get; set; }

        public ErrorStatus errorStatus;

        public string msg
        {
            get
            {
                if (GameServiceManager.IsNetworkAvaible)
                {
                    return "Thiet bi khong the ket noi mang";
                }

                return errorStatus.message;
            }
        }

        public string timestamp;
    }

    public class ErrorStatus
    {
        public ErrorCode errorCode {
            get { return (ErrorCode)System.Enum.Parse(typeof(ErrorCode), code); }
        }
        public string code;
        public string message;
    }
    
    public class ItemData
    {
        public string id { get; set; }
        [JsonProperty("namespace")]
        public string _namespace { get; set; }
        public string namespaceKey { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string thumbnail { get; set; }
        public string description { get; set; }
        public string unit { get; set; }
        public string message { get; set; }
    }

    public class ItemsInUse
    {
        public List<ItemInUse> data;
        public string currentTime;
    }

    public class ItemInUse
    {
        public string itemId;
        public int duration;
        public string useTime;
        public string expiredDate;
    }

    public enum ErrorCode
    {
        Undefined,
        PASSWORD_IS_WRONG,
        USER_NOT_FOUND
    }
}

