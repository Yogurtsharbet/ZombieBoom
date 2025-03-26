using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;

[Serializable]
public class ResponseStatus {
    public int status;
    public string message;

    public int code => status;
}

public enum RequestType {
    GetAllProduct,
    GetGuestLogin, PostGoogleLogin,
    GetAllRanking, GetMyRanking, PostRanking,
    PostGrantReward,
    PostPurchaseCoin,
    PostPurchaseGoogle,
    PostSignOut, GetUseTicket
}

public enum ProductType {
    ETC, Package, Character, Coins, Churu
}

public enum LanguageType {
    en, ko
}

/// <summary> 
/// <para>
///     Client 내의 모든 API Request 처리를 관장합니다. <br/>
///     각각의 Request는 RequestType enum lable 로 정의됩니다. <br/>
///     사용자는 Request를 보내기 전 고유 값으로 Request를 Initialize 해야합니다.
/// </para>
/// <para> 
///     구현 예제 
///     <code>
///         public class ExampleRequest : MonoBehaviour {
///             private Guid requestId = Guid.Empty;
///             
///             private void Awake() {
///                 APImanager.Request.OnResponse += OnResponse;
///             }
///             
///             private void SendRequest() {
///                 if (APImanager.Request.Init(requestID = Guid.NewGuid()))
///                     APImanager.Request.Get(RequestType.GetProductByType, ProductType.ETC);
///                 else
///                     throw new Exception("Request Failure!");
///             }
/// 
///             private void OnResponse(ResponseStatus status, string data) {
///                 if (requestID == Guid.Empty) return;    
///                 requestID = Guid.Empty;
///                 
///                 if (status.code != 200) return;
///                 /* ... Implement data processing ... */
///             }
///         }
///     </code>
/// </para>
/// </summary>
public class APImanager : MonoBehaviour {
    [SerializeField] private GameObject alert;

    public static APImanager Request => instance;
    private static APImanager instance;

    /// <summary>
    /// <para>
    ///     Request의 Response가 들어왔을 때 발생하는 Event 입니다. <br/>
    ///     사용자는 Response를 받았을 때 처리해야 할 메서드를 Event에 등록해야 합니다.
    /// </para>
    /// </summary>
    public Action<ResponseStatus, string> OnResponse;
    private Guid currentGUID;

    private string domain = "http://teamalphano.site:8081/api/";
    public static Product[] testData;
    public static ResponseStatus parseResponse;

    private Dictionary<RequestType, string> url = new Dictionary<RequestType, string>() {
        { RequestType.GetAllProduct, "shop/products" },
        { RequestType.GetGuestLogin, "user/guest/login" },
        { RequestType.PostGoogleLogin, "user/google/login" },
        { RequestType.GetAllRanking, "rank/world" },
        { RequestType.GetMyRanking, "rank/myrank" },
        { RequestType.PostRanking, "rank/insert" },
        { RequestType.PostGrantReward, "reward/get" },
        { RequestType.PostPurchaseGoogle, "purchase/google"},
        { RequestType.PostPurchaseCoin , "purchase/coin" },
        { RequestType.PostSignOut, "user/signOut" },
        { RequestType.GetUseTicket, "user/data/useTicket" }
    };

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        currentGUID = Guid.Empty;


        alert.GetComponentInChildren<Button>()
            .onClick.AddListener(() =>
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false
#else
            Application.Quit()
#endif
            );
        alert.SetActive(false);
    }

    /// <summary>
    /// <para>
    ///     Get / Post 메서드가 호출되기 직전 반드시 호출되어야 합니다. <br/>
    ///     임시적이면서도 고유한 ID 값으로 Request의 호출 당사자임을 등록합니다.
    /// </para>
    /// <code>
    ///     APImanager.Request.Init(requestID = Guid.NewGuid())
    /// </code>
    /// </summary>
    /// <param name="guid"> Guid.NewGuid() </param>
    /// <returns></returns>
    public bool Init(Guid guid) {
        if (currentGUID != Guid.Empty) return false;
        currentGUID = guid;
        return true;
    }

    public void CheckInited() {
        if (currentGUID == Guid.Empty)
            throw new ArgumentException(
                "Fatal : Request ID is empty. It could cause transaction confilct.\n" +
                "You need to Initialize Request first!");
    }

    public void Get(RequestType type, params string[] args) {
        CheckInited();
        string requestURL = domain + url[type];
        if (args != null) {
            requestURL += "?";
            foreach (var param in args)
                requestURL += param + "&";
        }
        UnityWebRequest request = UnityWebRequest.Get(requestURL);
        StartCoroutine(RequestProcess(request));
    }

    public void Post(RequestType type, string json) {
        CheckInited();
        if(type == RequestType.PostSignOut) currentGUID = Guid.Empty;

        string requestURL = domain + url[type];
        tempJson = json;

        UnityWebRequest request = CreatePostRequest(requestURL, json);
        StartCoroutine(RequestProcess(request));
    }

    private UnityWebRequest CreatePostRequest(string url, string data) {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, data);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    private string tempJson;
    private IEnumerator RequestProcess(UnityWebRequest request) {

#if UNITY_EDITOR
        int retryAttmpt = 1;
#else
        int retryAttmpt = 1;
#endif
        while (retryAttmpt-- != 0) {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                ErrorConnectionClosed();
                break;
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                currentGUID = Guid.Empty;
                ParseResult(request);
                break;
            }
            else {
                Debug.Log(request.error + request.url);

                if (retryAttmpt == 0) {
                    //Try to GET other domain for verify internet connection
                    UnityWebRequest newRequest = UnityWebRequest.Get("https://www.naver.com/");
                    yield return newRequest.SendWebRequest();

                    if (newRequest.result == UnityWebRequest.Result.Success)
                        ErrorServerNotFound(request);
                    else
                        ErrorConnectionClosed();
                }
                else {
                    if (request.method == "GET")
                        request = UnityWebRequest.Get(request.url);
                    else if (request.method == "POST")
                        request = CreatePostRequest(request.url, tempJson);
                }
            }
        }
        currentGUID = Guid.Empty;
    }

    private void ParseResult(UnityWebRequest request) {
        string response = Encoding.UTF8.GetString(request.downloadHandler.data);

        var splitedResult = response.Split(",\"data\":");
        var splitedStatus = splitedResult[0] + "}";
        var data = splitedResult[1].Substring(0, splitedResult[1].Length - 1);

        ResponseStatus status = JsonConvert.DeserializeObject<ResponseStatus>(splitedStatus);


        Debug.LogWarning(
            $"Request : {request.url}" +
            $"Response : {status.code} / {status.message} \n" +
            $"{data}");
        OnResponse?.Invoke(status, data);
    }

    private void ErrorConnectionClosed() {
        Time.timeScale = 0;
        alert.SetActive(true);
    }

    private void ErrorServerNotFound(UnityWebRequest request) {
        //TODO: 게임 서버와 연결이 원활하지 않음 처리 (TIMEOUT)


        ResponseStatus status = new ResponseStatus();
        status.status = (int)request.responseCode;
        OnResponse?.Invoke(status, null);
    }
}

public class PostData {
    public static string ToJson(params (string, object)[] data) {
        var dictionary = data.ToDictionary(item => item.Item1, item => item.Item2);
        return JsonConvert.SerializeObject(dictionary);
    }

    public static string ToJson(object data) {
        return JsonConvert.SerializeObject(data);
    }

}
