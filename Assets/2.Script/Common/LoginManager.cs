using GooglePlayGames;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour {
    private UserFullData user;
    private Guid requestID;
    private string uuid;
    private bool isGoogleLogin;

    private void Start() {
        uuid = SystemInfo.deviceUniqueIdentifier;
        user = Manager_Singleton_UserData.Instance.Data;

        if (user == null) {
            user = new UserFullData();
            user.userInfo = new UserInfo();
            user.userData = new UserData();
            Manager_Singleton_UserData.Instance.Data = user;
        }
        GPGSManager.Instance.OnGoogleLogin += OnGoogleLoginSuccess;
        APImanager.Request.OnResponse += OnResponse;
        SceneManager.sceneLoaded += OnSceneLoaded;

        RequestLogin();
        //TODO: OnResponse가 Invoke될 때까지 Loading Modal 띄우기
    }

    private void RequestLogin() {
        Debug.LogWarning($"uuid={uuid}");
        if (APImanager.Request.Init(requestID = Guid.NewGuid()))
            APImanager.Request.Get(RequestType.GetGuestLogin, $"uuid={uuid}");
        else requestID = Guid.Empty;
    }

    private void OnResponse(ResponseStatus status, string data) {
        if (requestID == Guid.Empty) return;
        requestID = Guid.Empty;
        Debug.LogWarning($"Login Responsed");

        if (status.code != 200) {
            Debug.LogWarning($"Login status failed");
            return;
        }

        if (isGoogleLogin)
            ProcessGoogleLogin(data);
        else
            ProcessLocalLogin(data);
    }

    private void ProcessLogoutTimeTicket() {
        var logout = Manager_Singleton_UserData.Instance.Data.userInfo.lastLgout;
        var login = Manager_Singleton_UserData.Instance.Data.userInfo.lastLgin;

        var logoutTime = Convert.ToDateTime(logout);
        var loginTime = Convert.ToDateTime(login);

        // 시간 차를 구해서 하트 개수 플러스 해야함.
    }

    private void ProcessLocalLogin(string response) {
        JsonConvert.PopulateObject(response, user);
        Manager_Singleton_UserData.Instance.Data = user;
        ProcessLogoutTimeTicket();

        Debug.LogWarning($"Login succeess");
        if (user.userInfo.userId == null) {
            if (PlayerPrefs.HasKey("isLoginCheck") && PlayerPrefs.HasKey("isPlayed"))
                OpenLoginPopup();
            else
                PlayerPrefs.SetInt("isLoginCheck", 1);
        }
        else
            OnGoogleLoginButton();
    }

    private void ProcessGoogleLogin(string response) {
        CloseLoginPopup();
        isGoogleLogin = false;
        JsonConvert.PopulateObject(response, user);
        Manager_Singleton_UserData.Instance.Data = user;

        requestID = Guid.Empty;
        FindAnyObjectByType<StoreManager>().ReqeustProduct();
    }

    private void OpenLoginPopup() {
        var manager = FindAnyObjectByType<Manager_UI_Main>();
        manager.OpenPopup(manager.loginPopup);
    }

    private void CloseLoginPopup() {
        var manager = FindAnyObjectByType<Manager_UI_Main>();
        manager.ClosePopup(manager.loginPopup);
    }

    public void OnGoogleLoginButton() {
        //TODO: OnResponse가 Invoke될 때까지 Loading Modal 띄우기

#if UNITY_EDITOR
        isGoogleLogin = true;
        string param = PostData.ToJson(
                    ("userUuid", uuid),
                    ("userId", $"userID:{uuid.Substring(0, 15)}"),
                    ("userName", "userName"),
                    ("userEmail", "user.userInfo.userEmail")
                    );

        if (APImanager.Request.Init(requestID = Guid.NewGuid()))
            APImanager.Request.Post(RequestType.PostGoogleLogin, param);
        else
            requestID = Guid.Empty;
#else
        GPGSManager.Instance.GPGS_Login();
#endif
    }

    private void OnGoogleLoginSuccess(PlayGamesPlatform google) {
        isGoogleLogin = true;

        user.userInfo.userId = google.GetUserId();
        user.userInfo.userName = google.GetUserDisplayName();
        user.userInfo.userEmail = google.GetUserEmail();

        string param = PostData.ToJson(
            ("userUuid", uuid),
            ("userId", user.userInfo.userId),
            ("userName", user.userInfo.userName),
            ("userEmail", user.userInfo.userEmail)
            );

        if (APImanager.Request.Init(requestID = Guid.NewGuid()))
            APImanager.Request.Post(RequestType.PostGoogleLogin, param);
        else
            requestID = Guid.Empty;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Intro")
            RequestLogin();
    }
}
