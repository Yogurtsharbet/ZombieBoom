using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProductControl : MonoBehaviour {
    public Product product;

    private TMP_Text productName;
    private TMP_Text productDescription;
    private Image productUniqueMark;
    private Image productImage;
    private Image productPurchased;
    private PurchaseButton purchaseButton;

    private void Awake() {
        productName = GetComponentsInChildren<TMP_Text>()[0];
        productDescription = GetComponentsInChildren<TMP_Text>()[1];
        productImage = GetComponentsInChildren<Image>()[1];
        productUniqueMark = GetComponentsInChildren<Image>()[2];
        productPurchased = GetComponentsInChildren<Image>()[4];
        purchaseButton = GetComponentInChildren<PurchaseButton>();
    }

    public void Init(Product product) {
        this.product = product;
        SetProductData();
    }

    private void Start() {
        SetProductData();
    }

    private void OnDisable() {
        gameObject.SetActive(false);
    }

    private void SetProductData() {
        StartCoroutine(GetProductImage());
        productName.text = product.prodName;
        productDescription.text = GetFormattedDesc(product.prodDesc);
        productUniqueMark.gameObject.SetActive(
            product.unique && product.prodType != (int)ProductType.Character);
        productPurchased.gameObject.SetActive(product.purchased);
        purchaseButton.Init(product);
    }

    private IEnumerator GetProductImage() {
        var sprite = ProductImage.Get(product.prodImgKey);
        if (sprite == null) {

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(product.prodImgKey);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                sprite = Sprite.Create(texture,
                                     new Rect(0, 0, texture.width, texture.height),
                                     new Vector2(0.5f, 0.5f));
                ProductImage.Add(product.prodImgKey, sprite);
            }
            //TODO: 통신 실패 예외처리
        }
        productImage.sprite = sprite;
    }

    private string GetFormattedDesc(string desc) {
        var target = desc.ToCharArray();
        int check = 0;
        char blank = ' ', enter = '\n';
        for (int i = 0; i  < target.Length; i++) {
            if (target[i] == blank)
                check++;
            if (check == 3) {
                target[i] = enter;
                check = 0;
            }
        }
        return target.ArrayToString();
    }
}

public class ProductImage {
    public static Dictionary<string, Sprite> Pool = new Dictionary<string, Sprite>();

    public static Sprite Get(string url) {
        if(Pool.ContainsKey(url)) 
            return Pool[url];
        else return null;
    }

    public static void Add(string url, Sprite sprite) {
        if (!Pool.ContainsKey(url))
            Pool.Add(url, sprite);
    }

}