using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestSTP
{
    public class BtnBuffStatus : MonoBehaviour
    {
        [SerializeField] private Button _btn;
        [SerializeField] private TMP_Text _txtBtn;
        void Start()
        {
            if (_btn == null) _btn = GetComponent<Button>();
            _btn.onClick.AddListener(OnBtnClick);
            _btn.interactable = false;
            _txtBtn.text = "Loading...";
            GameServiceManager.Instance.GetItemsConfig();
            StartCoroutine(nameof(GetListItems));
        }

        void OnBtnClick()
        {
            GameServiceManager.Instance.GetItemsInUse();
        }
        
        private IEnumerator GetListItems()
        {
            yield return new WaitUntil(() => GameServiceManager.Instance.configItems != null && GameServiceManager.Instance.configItems.Count > 0);
            _btn.interactable = true;
            _txtBtn.text = "Buff Status";
            StopCoroutine(nameof(GetListItems));
        }
    }

}

