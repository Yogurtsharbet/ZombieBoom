using GooglePlayGames.BasicApi;
using GooglePlayGames;
using UnityEngine;
using System;

public enum GPGSState {
    ALRD_LGN,
    LGN_FAIL,
    LGN_SUCCESS,
    NOT_EXIST
}

public class GPGSManager : MonoBehaviour {
    public static GPGSManager Instance;
    public Action<PlayGamesPlatform> OnGoogleLogin;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        GPGS_Setting();
    }

    private void GPGS_Setting() {
        PlayGamesPlatform.DebugLogEnabled = true;

        // GPGS 설정 초기화
#if UNITY_EDITOR
        PlayGamesClientConfiguration config =
            new PlayGamesClientConfiguration.Builder().Build();
#else
        PlayGamesClientConfiguration config = 
            new PlayGamesClientConfiguration.Builder().RequestEmail().Build();
#endif
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
    }

    public void GPGS_Login() {
        if (!Social.localUser.authenticated)
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, ProcessAuth);
    }

    private void ProcessAuth(SignInStatus result) {
        switch (result) {
            case SignInStatus.Success:
                OnGoogleLogin?.Invoke(PlayGamesPlatform.Instance);
                break;
                //TODO: Failed Event Invoke
        }
    }

    public void GPGS_SignOut() {
        PlayGamesPlatform.Instance.SignOut();
    }
}
