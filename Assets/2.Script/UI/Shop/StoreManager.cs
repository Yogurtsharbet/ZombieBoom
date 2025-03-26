using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour {
    [SerializeField] private GameObject StorePopup;
    [SerializeField] private GameObject TicketPopup;
    [SerializeField] private ProductControl productControl;

    private ScrollRect storeScroll;
    private RectTransform storeContent;

    private enum StoreType { Store, Ticket };
    private StoreType currentStore;
    private StoreTabControl storeTabControl;
    private PurchaseManager purchaseManager;
    public StoreModalControl Modal { get; private set; }

    private List<ProductControl> productPool;

    public Product[] Products { get; private set; }
    private Guid requestId;

    private void Awake() {
        storeTabControl = StorePopup.GetComponentInChildren<StoreTabControl>();
        storeScroll = StorePopup.GetComponentInChildren<ScrollRect>();
        purchaseManager = FindAnyObjectByType<PurchaseManager>();
        Modal = FindAnyObjectByType<StoreModalControl>();
        
        productPool = new List<ProductControl>();
        storeContent = storeScroll.content;

        APImanager.Request.OnResponse += OnResponse;
    }

    private void Start() {
        StorePopup.SetActive(false);
        TicketPopup.SetActive(false);
    }

    public void JoinStore() {
        currentStore = StoreType.Store;
        StorePopup.SetActive(true);
        ReqeustProduct();
    }

    public void JoinTicketPopup() {
        currentStore = StoreType.Ticket;
        TicketPopup.SetActive(true);
        ReqeustProduct();
    }

    public void ReqeustProduct() {
        Debug.LogWarning("REQUEST PRODUCT");
        if (APImanager.Request.Init(requestId = Guid.NewGuid())) {
            APImanager.Request.Get(RequestType.GetAllProduct,
                $"userNo={Manager_Singleton_UserData.Instance.Data.userInfo.userNo}&" +
                $"langType={(LanguageType)Manager_Singleton_UserData.Instance.languageIndex}");
        }
        else
            requestId = Guid.Empty;
    }

    private void OnResponse(ResponseStatus status, string data) {
        if (status.message != "AllProducts") return;
        if (requestId == Guid.Empty) return;
        requestId = Guid.Empty;

        if (status.code != 200) return;

        Debug.LogWarning("REQUEST PRODUCT Responsed");
        Products = JsonConvert.DeserializeObject<Product[]>(data);
        purchaseManager.InitPurchase(Products);
        if (currentStore == StoreType.Store) {
            InitializeProducts();
        }
    }

    public void InitializeProducts() {
        Debug.LogWarning("Initialize Store Products");

        var currentTab = storeTabControl.currentTab;
        int productCount = 0;
        foreach (var product in productPool)
            product.gameObject.SetActive(false);

        foreach (var product in Products) {
            if (product.prodType == currentTab) {
                var productBox = GetProductAtPool();
                productBox.Init(product);
                productCount++;
            }
        }

        Debug.LogWarning("Initialized Store Products Complete");
        SetProductScroll(productCount > 4);
    }

    private void SetProductScroll(bool flag) {
        storeScroll.enabled = flag;
        storeScroll.horizontalNormalizedPosition = 0;

        if (!flag) {
            var rect = storeContent.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

    }

    private ProductControl GetProductAtPool() {
        foreach (var product in productPool) {
            if (!product.gameObject.activeSelf) {
                product.gameObject.SetActive(true);
                return product;
            }
        }

        var newProduct = Instantiate(productControl, parent: storeContent);
        productPool.Add(newProduct);
        return newProduct;
    }
}
