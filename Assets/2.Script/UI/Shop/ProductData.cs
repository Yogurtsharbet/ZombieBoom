using System.Collections.Generic;
using System;

[Serializable]
public class Product {
    public int prodNo;
    public int prodPrice;
    public int prodPriceType;
    public int prodType;
    public bool prodLimit;
    public string prodStartDate;
    public string prodEndDate;
    public int prodPurchaseLimit;
    public int prodPurchaseCount;
    public int prodOrdr;
    public bool prodActive;
    public string prodImgKey;
    public string prodId;

    public List<ProductItem> items;

    public string langType;
    public string prodName;
    public string prodDesc;

    public int? unityProdType;

    public bool unique;
    public bool purchased;
}

[Serializable]
public class ProductItem {
    public int itemNo;
    public string itemName;
    public int itemCount;
    public string itemTime;
    public int? itemType;
}
