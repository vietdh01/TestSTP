using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestSTP.UI
{
    public class LoginPanel : MonoBehaviour
    {
        [Header("Common")]
        [SerializeField] private Button btnLogin;
        [SerializeField] private TMP_Text txtWarning;
        
        [Header("Email")]
        [SerializeField] private TMP_InputField inputEmail;
        [SerializeField] private TMP_Text txtEmailValidate;
        
        [Header("Password")]
        [SerializeField] private TMP_InputField inputPassword;
        [SerializeField] private TMP_Text txtPasswordValidate;
        
        [Header("Params")]
        [SerializeField] private int wrongPasswordCount = 6;
        [SerializeField] private int wrongPasswordBan = 15;

        private Dictionary<string, List<DateTime>> dictWrongPass = new Dictionary<string, List<DateTime>>();

        private Dictionary<string, DateTime> dictBan;
        // Start is called before the first frame update
        void Start()
        {
            btnLogin.onClick.AddListener(OnLoginClick);
            GameServiceManager.Instance.EventOnResponseError += OnResponseError;
            var strData = PlayerPrefs.GetString("WRONG_PASS_BAN");
            var emailData = PlayerPrefs.GetString("SAVE_EMAIL");
            if (!string.IsNullOrEmpty(emailData))
            {
                inputEmail.text = emailData;
            }
            if (!string.IsNullOrEmpty(strData))
            {
                dictBan = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(strData);
            }
            else
            {
                dictBan = new Dictionary<string, DateTime>();
            }
        }

        private void OnDisable()
        {
            GameServiceManager.Instance.EventOnResponseError -= OnResponseError;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void OnLoginClick()
        {
            if (CheckBanEmail()) return;
            var email = inputEmail.text;
            PlayerPrefs.SetString("SAVE_EMAIL", email);
            var pass = inputPassword.text;
            GameServiceManager.Instance.UserLogin(email, pass);
        }

        public void ValidateEmail(string email)
        {
            if (Utils.IsMailValid(email))
            {
                btnLogin.interactable = true;
                txtEmailValidate.text = "";
            }
            else
            {
                btnLogin.interactable = false;
                txtEmailValidate.text = "Incorrect email";
            }

            CheckBanEmail();
        }

        public bool CheckBanEmail()
        {
            var email = inputEmail.text;
            if (dictBan.ContainsKey(email))
            {
                btnLogin.interactable = false;
                var timeNow = DateTime.Now;
                var remaining = wrongPasswordBan*60 - timeNow.Subtract(dictBan[email]).TotalSeconds;
                if (remaining > 0)
                {
                    txtWarning.text = $"This email is banned for:";
                    if (txtWarning.TryGetComponent(out CountDownTimer timer))
                    {
                        timer.StartNewCountdownTimer($"banEmail", (float)remaining, () =>
                        {
                            btnLogin.interactable = true;
                            dictBan.Remove(email);
                            SaveBanDict();
                            timer.Hide();
                            txtWarning.text = "";
                        });
                    }

                    return true;
                }
                else
                {
                    dictBan.Remove(email);
                    SaveBanDict();
                    return false;
                }
            }

            return false;
        }

        private void OnResponseError(int code, string msg)
        {
            txtEmailValidate.text = "";
            txtWarning.text = "";
            txtPasswordValidate.text = "";
            btnLogin.interactable = true;
            switch (code)
            {
                case (int)ErrorCode.PASSWORD_IS_WRONG:
                case 400:
                    txtPasswordValidate.text = "Incorrect password";
                    CountWrongPassword();
                    break;
                case (int)ErrorCode.USER_NOT_FOUND:
                    txtPasswordValidate.text = "Incorrect email";
                    break;
                default:
                    txtWarning.text = $"{code}: {msg}";
                    break;
            }
        }

        private void CountWrongPassword()
        {
            var email = inputEmail.text;
            var time = DateTime.Now;
            if (!dictWrongPass.ContainsKey(email)) dictWrongPass[email] = new List<DateTime>();
            dictWrongPass[email].Add(time);
            Debug.Log($"{dictWrongPass[email].Count}");
            for (int i = dictWrongPass[email].Count - 1; i >= 0; i--)
            {
                if (dictWrongPass[email].Count < 2) break;
                for (int j = dictWrongPass[email].Count - 2; j >= 0; j--)
                {
                    if ( dictWrongPass[email][i].Subtract(dictWrongPass[email][j]).TotalMinutes <= 2 && i - j >= wrongPasswordCount)
                    {
                        Debug.Log($"Sai pass qua {wrongPasswordCount} lan");
                        dictWrongPass[email].Clear();
                        dictBan[email] = DateTime.Now;
                    }   
                }
            }
        }

        private void SaveBanDict()
        {
            var data = JsonConvert.SerializeObject(dictBan);
            PlayerPrefs.SetString("WRONG_PASS_BAN", data);
        }
    }

}
    
