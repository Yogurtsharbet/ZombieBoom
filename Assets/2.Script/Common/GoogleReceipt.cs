using Newtonsoft.Json;
using System;

[Serializable]
public class GoogleReceipt {
    public string Store;
    public string TransactionID;
    public string Payload;

    public string purchaseToken {
        get {
#if UNITY_EDITOR
            return "TEST_TOKEN";
#else
            var payload = JsonConvert.DeserializeObject<ReceiptPayload>(Payload);
            var data = JsonConvert.DeserializeObject<ReceiptData>(payload.json);
            return data.purchaseToken;
#endif
        }
    }
    public static GoogleReceipt Parse(string data) {
        return JsonConvert.DeserializeObject<GoogleReceipt>(data);
    }
}

[Serializable]
public class ReceiptPayload {
    public string json;
    public string signature;
    public string[] skuDetails;
}


[Serializable]
public class ReceiptData {
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public int quantity;
    public bool acknowledged;
}