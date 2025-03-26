using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class StoreModalControl : MonoBehaviour {
    public enum StoreModalType {
        PurchaseComplete, PurchaseFailed, NotEnoughCoin, LinkAccount
    };

    [SerializeField] private LocalizeStringEvent modal;
    private StoreManager storeManager;
    private Button modalButton;

    private const string stringTable = "String Table";
    private Dictionary<StoreModalType, LocalizedString> modalText =
        new Dictionary<StoreModalType, LocalizedString>() {

            { StoreModalType.PurchaseComplete,
                new LocalizedString{ TableReference = stringTable, 
                    TableEntryReference = "Shop_Page_Purchase_Complete Text"} },

            { StoreModalType.PurchaseFailed,
                new LocalizedString{ TableReference = stringTable, 
                    TableEntryReference = "Shop_Page_Purchase_Failed Text Text"} },

            { StoreModalType.NotEnoughCoin,
                new LocalizedString{ TableReference = stringTable, 
                    TableEntryReference = "Shop_Page_Purchase_NotEnough Text"} },

             { StoreModalType.LinkAccount,
                new LocalizedString{ TableReference = stringTable, 
                    TableEntryReference = "Warning_GoogleAccount_Message"} },
        };

    private void Awake() {
        storeManager = FindAnyObjectByType<StoreManager>();
        modalButton = GetComponentInChildren<Button>();
        modalButton.onClick.AddListener(
            () => {
                this.gameObject.SetActive(false);
                storeManager.ReqeustProduct();
            }
            );
    }

    private void Start() {
        gameObject.SetActive(false);
    }

    public void Show(StoreModalType type) {
        gameObject.SetActive(true);
        modal.StringReference = modalText[type];
        FindAnyObjectByType<InfinityTicketControl>().CalculateTime();
    }
}
