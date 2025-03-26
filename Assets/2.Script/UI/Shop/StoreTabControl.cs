using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreTabControl : MonoBehaviour {
    public enum StoreTab {
        ETC, Package, Character, Coin, Key
    };

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private StoreManager store;
    private Image[] buttonSprites;
    private StoreTab storeTab;
    public int currentTab => (int)storeTab;

    private void Awake() {
        InitStoreButton();
    }

    private void InitStoreButton() {
        store = FindAnyObjectByType<StoreManager>();
        buttonSprites = GetComponentsInChildren<Image>();
        Button[] storeButtons = GetComponentsInChildren<Button>();
        for (int i = 0; i < storeButtons.Length; i++) {
            Button button = storeButtons[i];
            int index = i + 1;
            button.onClick.AddListener(() => Select(index));
        }
        Select((int)StoreTab.Package);
        storeTab = StoreTab.Package;
    }

    private void Select(int index) {
        for(int i = 0; i <  buttonSprites.Length; i++)
            DeSelect(i);

        buttonSprites[index - 1].sprite = selectedSprite;
        storeTab = (StoreTab)index;
        store.InitializeProducts();
    }

    private void DeSelect(int index) {
        buttonSprites[index].sprite = normalSprite;
    }
}
