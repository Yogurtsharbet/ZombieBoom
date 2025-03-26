using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ModalType = StoreModalControl.StoreModalType;

[AddComponentMenu("UI/Purchase Button", 30)]
public class PurchaseButton : Button {
    [SerializeField] private Sprite coinButton;
    [SerializeField] private Sprite cashButton;
    private Image buttonImage;

    private static StoreManager _storeManager;
    private static StoreManager storeManager {
        get {
            if (_storeManager == null)
                _storeManager = FindAnyObjectByType<StoreManager>();
            return _storeManager;
        }
    }
    private static PurchaseManager _purchaseManager;
    private static PurchaseManager purchaseManager {
        get {
            if (_purchaseManager == null)
                _purchaseManager = FindAnyObjectByType<PurchaseManager>();
            return _purchaseManager;
        }
    }

    public string productID;
    public Product product;
    private TMP_Text productPrice;

    private bool isInit;

    protected override void Awake() {
        base.Awake();
        buttonImage = GetComponent<Image>();
        productPrice = GetComponentInChildren<TMP_Text>();
    }

    protected override void Start() {
        base.Start();
        if (!isInit &&
            Manager_Singleton_UserData.Instance.Data.userInfo.userEmail != null)
            Init(purchaseManager.GetProductData(productID));

    }

    public void Init(Product product) {
        this.product = product;
        productID = product.prodId;
        buttonImage.sprite =
            product.prodPriceType == 1 ? coinButton : cashButton;
        productPrice.text = product.prodPrice.ToString("N0");

        onClick.RemoveAllListeners();
        onClick.AddListener(() => Purchase(product));
        isInit = true;
    }

    private void Purchase(Product product) {
        Debug.LogWarning("Purchase Button Logic");
        if (product.prodPriceType == 1 &&
            product.prodPrice > Manager_Singleton_UserData.Instance.currentCoin) {
            storeManager.Modal.Show(ModalType.NotEnoughCoin);
            return;
        }
        else if (product.prodPriceType == 2 &&
            Manager_Singleton_UserData.Instance.Data.userInfo.userEmail == null) {
            storeManager.Modal.Show(ModalType.LinkAccount);
            return;
        }

        purchaseManager.Purchase(product);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PurchaseButton))]
public class PurchaseButtonEditor : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();
        PurchaseButton purchaseButton = (PurchaseButton)target;

        purchaseButton.interactable =
            EditorGUILayout.Toggle("Interactable", purchaseButton.interactable);

        EditorGUILayout.LabelField("Purchase Settings", EditorStyles.boldLabel);
        string newProductID =
            EditorGUILayout.TextField("Product Key", purchaseButton.productID);
        if (newProductID != purchaseButton.productID) {
            purchaseButton.productID = newProductID;
            EditorUtility.SetDirty(purchaseButton); // 변경 사항 저장
        }

        GUI.enabled = false;
        EditorGUILayout.TextField("Product Name", purchaseButton.product.prodName);
        EditorGUILayout.IntField("Product Price", purchaseButton.product.prodPrice);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
