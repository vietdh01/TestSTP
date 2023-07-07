using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestSTP.UI
{
    public class PanelBuff : MonoBehaviour
    {
        [SerializeField] private Transform _itemScrollContent;
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private Button _btnLogout;
        private readonly List<BuffItem> _items = new List<BuffItem>();
        private CanvasGroup _canvasGroup;
         protected void Start()
         {
             if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
             _btnLogout.onClick.AddListener(OnBtnLogoutClicked);
            GameServiceManager.Instance.EventOnGetItemsInUse += UpdateBuffPanelUI;
        }

        protected void OnDestroy()
        {
            GameServiceManager.Instance.EventOnGetItemsInUse -= UpdateBuffPanelUI;
        }
        
        public void Show()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        
        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void UpdateBuffPanelUI(ItemsInUse itemsInUse){
            _itemPrefab.SetActive(false);
            var listItems = itemsInUse.data;
            for(var i = 0; i < listItems.Count; i++) {
                if(i < _items.Count) { // Update
                    _items[i].UpdateValue(listItems[i], itemsInUse.currentTime);
                } else { // Create new one
                    var newItem = Instantiate(_itemPrefab, _itemScrollContent);
                    if(!newItem.TryGetComponent(out BuffItem item)) continue;
                    item.UpdateValue(listItems[i], itemsInUse.currentTime);
                    _items.Add(item);
                    item.gameObject.SetActive(true);
                }
            }
            for (var j = 0; j < _items.Count; j++)
            {
                if (j < listItems.Count)
                {
                    _items[j].gameObject.SetActive(true);
                    if(!_items[j].HasData()) _items[j].gameObject.SetActive(false);
                } else {
                    _items[j].gameObject.SetActive(false);
                }
            }
            Show();
        }

        private void OnBtnLogoutClicked()
        {
            Hide();
            SceneManager.LoadScene("LoginScene", LoadSceneMode.Single);
        }
    }
}

