using System;
using System.Collections;
using System.Collections.Generic;
using TestSTP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffItem : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private Image _imgIcon;
    [SerializeField] private TMP_Text _txtName;
    [SerializeField] private TMP_Text _txtDesc;
    [SerializeField] private CountDownTimer _timer;
    
    private ItemInUse _itemInUse;
    private ItemData _itemData;

    private string currentTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void UpdateValue(ItemInUse item, string curTime)
    {
        currentTime = curTime;
        _itemInUse = item;
        _itemData = GameServiceManager.Instance.FindItemData(item.itemId);
        if (_itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        
        _txtName.text = _itemData.name;
        _txtDesc.text = _itemData.description;
        
        var timeNow = DateTime.Parse(curTime);
        var timeUsed = DateTime.Parse(_itemInUse.useTime);
        var remaining = _itemInUse.duration - timeNow.Subtract(timeUsed).TotalSeconds;
        
        if (remaining > 0)
        {
            _timer.StartNewCountdownTimer(item.itemId, (float)remaining , () =>
            {
                gameObject.SetActive(false);
            }, _itemInUse.duration);
        }
        else
        {
            _timer.SetExpired(() =>
            {
                gameObject.SetActive(false);
            });
        }

        FillThumbnail();
    }

    public bool HasData()
    {
        return _itemData != null;
    }

    async void FillThumbnail()
    {
        var texture = await Utils.GetRemoteTexture($"{ClientAPI.URL}{_itemData.thumbnail}");
        if (texture == null) return;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
        _imgIcon.sprite = sprite;
    }
}
