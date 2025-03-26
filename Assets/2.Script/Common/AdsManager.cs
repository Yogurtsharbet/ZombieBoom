using GoogleMobileAds.Api;
using Newtonsoft.Json;
using System;
using UnityEditor;
using UnityEngine;

public enum AdsType {
    Null, Reward_Ticket5
}

public class AdsManager : MonoBehaviour {
    private static AdsManager instance;
    public static AdsManager Instance => instance;
    public bool isPurchasing;

    private string adUnitId = "ca-app-pub-2676723305076551/6818934106";
    private RewardedAd rewardAds;
    private AdRequest adsRequest;

    private AdsType adsType;
    private Guid requestID;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        APImanager.Request.OnResponse += OnResponse;
    }

    public void Start() {
        MobileAds.Initialize(initStatus => { });
        adsRequest = new AdRequest.Builder().Build();
        rewardAds = new RewardedAd(adUnitId);

        LoadRewardAds();
        RegisterEventHandler();
    }

    public void LoadRewardAds() {
        Debug.Log("Loading");
        isPurchasing = false;
        rewardAds.LoadAd(adsRequest);
    }

    public void ShowAds(AdsType type) {
        adsType = type;
        Debug.Log("Showing");
        if (rewardAds.IsLoaded()) {
            isPurchasing = true;
            rewardAds.Show();
            Debug.Log("Showed");
        }
        else
            Debug.Log("Ads not loaded");
    }

    private void RegisterEventHandler() {
        rewardAds.OnAdLoaded +=
        (object sender, EventArgs args) => {
            Debug.Log("Loaded");
        };

        rewardAds.OnAdFailedToLoad +=
        (object sender, AdFailedToLoadEventArgs args) => {
            Debug.Log("Load Failed : " + args.LoadAdError.GetMessage());
            adsType = AdsType.Null;
            isPurchasing = false;
        };

        rewardAds.OnAdOpening +=
        (object sender, EventArgs args) => {
            Debug.Log("Ads Opened");
        };

        rewardAds.OnAdFailedToShow +=
        (object sender, AdErrorEventArgs args) => {
            Debug.Log("Ads Open Failed : " + args.AdError.GetMessage());
            adsType = AdsType.Null;
            LoadRewardAds();
        };

        rewardAds.OnUserEarnedReward +=
        (object sender, Reward args) => {
            Debug.Log("Rewarded");
            GrantReward(adsType);
            adsType = AdsType.Null;
        };

        rewardAds.OnAdClosed +=
        (object sender, EventArgs args) => {
            Debug.Log("Ads Closed");
            LoadRewardAds();
        };
    }

    private void GrantReward(AdsType adsType) {
        var postBody = new {
            userFullData = Manager_Singleton_UserData.Instance.Data,
            type = (int)adsType,
            amount = 1,
        };

        Debug.LogWarning($"Reward request");
        if (APImanager.Request.Init(requestID = Guid.NewGuid()))
            APImanager.Request.Post(RequestType.PostGrantReward, PostData.ToJson(postBody));
        else {
            requestID = Guid.Empty;
            Debug.LogWarning($"request denined");

        }
    }

    private void OnResponse(ResponseStatus status, string data) {
        if (requestID == Guid.Empty) return;
        requestID = Guid.Empty;

        Debug.LogWarning($"Reward responsed");

        if (status.code != 200) return;

        Debug.LogWarning($"Reward granted");

        var userData = JsonConvert.DeserializeObject<UserFullData>(data);
        Manager_Singleton_UserData.Instance.Data = userData;
        isPurchasing = false;
    }

    private void OnApplicationPause(bool pause) {
#if !UNITY_EDITOR
        if (Manager_Singleton_UserData.Instance.Data == null ||
            isPurchasing) 
            return;

        if (APImanager.Request.Init(Guid.NewGuid())) {
            APImanager.Request.Post(RequestType.PostSignOut, PostData.ToJson(
                Manager_Singleton_UserData.Instance.Data));
        }
#endif
    }

    private void OnApplicationQuit() {
        OnApplicationPause(true);
    }

    private void OnApplicationFocus(bool focus) {
        if (!focus) {
            OnApplicationPause(true);
        }
    }
}
