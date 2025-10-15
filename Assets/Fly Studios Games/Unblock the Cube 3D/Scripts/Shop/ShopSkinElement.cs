using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSkinElement : MonoBehaviour
{
    public Text skinName;
    public Text skinPrice;
    public Image skinSprite;
    public Button actionButton; // butonul principal (buy/select)

    // starea curentă
    private string _skinId;
    private int _price;
    private bool _owned;
    private bool _selected;
    private ShopManager _manager;

    public void Initialize(string id, string name, int price, Sprite icon, bool owned, bool selected, ShopManager manager)
    {
        _skinId = id;
        _manager = manager;
        _price = price;
        _owned = owned;
        _selected = selected;

        if (skinName != null) skinName.text = name;
        if (skinSprite != null) skinSprite.sprite = icon;

        UpdateUI();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClicked);
        }
    }

    private void UpdateUI()
    {
        // folosim skinPrice pentru a afișa fie prețul, fie statusul "Owned"/"Selected"
        if (skinPrice != null)
        {
            if (_owned)
                skinPrice.text = _selected ? "Selected" : "Owned";
            else
                skinPrice.text = _price.ToString();
        }

        // opțional: dezactivăm butonul dacă este "Selected"
        if (actionButton != null)
        {
            actionButton.interactable = !_selected;
        }
    }

    private void OnClicked()
    {
        if (_manager != null)
        {
            _manager.OnElementClicked(_skinId, _price, _owned);
        }
    }
}
