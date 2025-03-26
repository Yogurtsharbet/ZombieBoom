using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using ModalType = StoreModalControl.StoreModalType;

public class PurchaseManager : MonoBehaviour, IDetailedStoreListener {
    private StoreManager storeManager;
    public Product[] productList { get; private set; }

    private IStoreController storeController;
    private UnityEngine.Purchasing.Product pendingProduct;

    private Guid requestID;

    private void Awake() {
        storeManager = GetComponent<StoreManager>();
        APImanager.Request.OnResponse += OnResponse;
    }

    private void OnDisable() {
        APImanager.Request.OnResponse -= OnResponse;
    }

    public Product GetProductData(string productID) {
        if (productList == null) return null;
        foreach (var product in productList) {
            if (product.prodId == productID)
                return product;
        }
        return null;
    }

    public void InitPurchase(Product[] productList) {
        Debug.LogWarning("Start Init Purchase");

        this.productList = productList;
        InitUnityIAP();
    }

    private void OnResponse(ResponseStatus status, string data) {
        if (requestID == Guid.Empty) return;
        requestID = Guid.Empty;

        if (status.code != 200) {
            //통신 실패 예외처리 팝업 등 필요
            //TODO: 구매에 실패함 처리 필요
            return;
        }

        Debug.LogWarning("PRODUCT PURCHASED");
        Debug.Log(data);
        var userData = JsonConvert.DeserializeObject<UserFullData>(data);
        Manager_Singleton_UserData.Instance.Data = userData;

        storeController.ConfirmPendingPurchase(pendingProduct);
        pendingProduct = null;
        AdsManager.Instance.isPurchasing = false;

        storeManager.Modal.Show(ModalType.PurchaseComplete);
    }

    /// <summary>
    /// <para>구매 가능한 상태일 경우 true를 return 합니다.</para>
    /// <para>true가 구매에 성공했다는 것을 보장하지 않습니다.</para>
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    public void Purchase(Product _product) {
        Debug.LogWarning("Purchase Going : " + _product.prodName);
        var product = storeController.products.WithID(_product.prodId);
        pendingProduct = product;
        AdsManager.Instance.isPurchasing = true;

        switch (_product.prodPriceType) {
            case 1:
                if (APImanager.Request.Init(requestID = Guid.NewGuid()))
                    APImanager.Request.Post(RequestType.PostPurchaseCoin, PostData
                        .ToJson(
                        ("userNo", Manager_Singleton_UserData.Instance.Data.userInfo.userNo),
                        ("prodId", _product.prodId),
                        ("transactionId", requestID)
                        ));
                else
                    requestID = Guid.Empty;
                break;

            case 2:
                if (product != null && product.availableToPurchase)
                    storeController.InitiatePurchase(product);
                else
                    storeManager.Modal.Show(ModalType.PurchaseFailed);
                break;

            default:
                Debug.Log("ERROR : Undefined Product Type");
                storeManager.Modal.Show(ModalType.PurchaseFailed);
                break;
        }
    }

    private void InitUnityIAP() {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        for (int i = 0; i < productList.Length; i++) {
            if (productList[i].prodPriceType != 2) continue;

            IDs storeIDs = new IDs();
            storeIDs.Add(productList[i].prodId, GooglePlay.Name);
            builder.AddProduct(
                productList[i].prodId,
                (UnityEngine.Purchasing.ProductType)productList[i].prodType,
                storeIDs);
        }
        UnityPurchasing.Initialize(this, builder);
        Debug.LogWarning("Initilized Purchase");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("Purchase Initiatlized Suceess.");
        storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log("Purchase Initiatlized Failed!" + error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) {
        Debug.Log("Purchase Initiatlized Failed! : " + error.ToString() + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
        if (pendingProduct == null) {
            return PurchaseProcessingResult.Complete;
        }
        else {
            foreach (var product in productList) {
                if (purchaseEvent.purchasedProduct.definition.id == product.prodId) {
                    SendReceipt(purchaseEvent.purchasedProduct, product);
                    break;
                }
            }
            Debug.Log("Purchase Pending. : " + purchaseEvent.purchasedProduct.definition.id);

            //TODO: Pending Product가 존재할 때 추가 결제 처리가 안되도록?
            return PurchaseProcessingResult.Pending;
        }
    }

    private void SendReceipt(UnityEngine.Purchasing.Product product, Product productData) {
        GoogleReceipt googleReceipt = GoogleReceipt.Parse(product.receipt);

        if (APImanager.Request.Init(requestID = Guid.NewGuid()))
            APImanager.Request.Post(RequestType.PostPurchaseGoogle, PostData.ToJson(
                //TODO: Log Data need to add
                ("userNo", Manager_Singleton_UserData.Instance.Data.userInfo.userNo),
                ("prodId", productData.prodId),
                ("transactionId", requestID),
                ("purchaseToken", googleReceipt.purchaseToken)
                ));
        else
            requestID = Guid.Empty;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason) {

        Debug.Log("Purchase Initiatlized Failed! : " + product.ToString() + failureReason.ToString());
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureDescription failureDescription) {
        Debug.Log("Purchase Initiatlized Failed! : " + product.ToString() + failureDescription.reason);
    }
}
