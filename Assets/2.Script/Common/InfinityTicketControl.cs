using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfinityTicketControl : MonoBehaviour {
    [SerializeField] private GameObject infinityPopup;
    [SerializeField] private TMP_Text[] ticketText;
    [SerializeField] private Image[] infiniteImage;

    private TMP_Text timeText;
    private DateTime infTime;
    private Coroutine countTime;

    private WaitForSeconds refreshTime = new WaitForSeconds(3f);
    public static bool isInfinity;

    private void Awake() {
        timeText = infinityPopup.GetComponentInChildren<TMP_Text>();
        CloseInfinity();

        Manager_Singleton_UserData.Instance.OnChange_Data += OnChangeData;
    }

    private void OnChangeData() {
        Debug.Log("CHANGE DATA : INFINITE TICKET");
        infTime = Convert.ToDateTime(Manager_Singleton_UserData.Instance.Data.userData.timeTicketEndDate);
        if (DateTime.Compare(infTime, DateTime.Now) > 0 && countTime == null) {
            countTime = StartCoroutine(Tick());
            Debug.Log("START TICK");
        }

    }

    private IEnumerator Tick() {
        ShowInfinity();
        CalculateTime(immediately: true);
        do {
            yield return refreshTime;
            CalculateTime();
        } while (DateTime.Compare(infTime, DateTime.Now) > 0);

        countTime = null;
        CloseInfinity();
    }

    private void ShowInfinity() {
        isInfinity = true;
        infinityPopup.SetActive(true);
        foreach (var each in ticketText)
            each.enabled = false;
        foreach (var each in infiniteImage)
            each.enabled = true;
    }

    private void CloseInfinity() {
        isInfinity = false;
        infinityPopup.SetActive(false);
        foreach (var each in ticketText)
            each.enabled = true;
        foreach (var each in infiniteImage)
            each.enabled = false;
    }

    public void CalculateTime(bool immediately = false) {
        var timeSpan = (infTime - DateTime.Now);
        if (timeSpan.Seconds < 3 || timeSpan.Seconds > 57 || immediately)
            timeText.text = $"{timeSpan.Hours} : {timeSpan.Minutes}";
    }
}
