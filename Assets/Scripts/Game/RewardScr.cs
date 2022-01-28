using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class RewardScr : MonoBehaviour
{
    [SerializeField] private List<GameObject> rewardText = new List<GameObject>();
    [SerializeField] private Text finalText = null;
    private bool survOrMur;

    void Start()
    {
        if (!(PlayerPrefs.GetString("Dead").IsNullOrEmpty())) survOrMur = true;
        else survOrMur = false;

        SetRewardText(survOrMur);
    }

    private void SetRewardText(bool survOrMur)
    {
        int finalReward = 300;

        if (survOrMur)
        {
            rewardText[0].GetComponentInChildren<Text>().text = "Survive : " + PlayerPrefs.GetInt("Dead");
            rewardText[1].GetComponentInChildren<Text>().text = "Rescue : " + PlayerPrefs.GetInt("Rescue");
            rewardText[2].GetComponentInChildren<Text>().text = "Repair : " + PlayerPrefs.GetInt("Repair");
            rewardText[3].GetComponentInChildren<Text>().text = "Treat : " + PlayerPrefs.GetInt("Treat");
            finalReward += PlayerPrefs.GetInt("Dead") + PlayerPrefs.GetInt("Rescue") + PlayerPrefs.GetInt("Repair") + PlayerPrefs.GetInt("Treat");
        }
        else
        {
            rewardText[0].SetActive(false);
            rewardText[3].SetActive(false);
            rewardText[1].GetComponentInChildren<Text>().text = "Hang : " + PlayerPrefs.GetInt("Hang");
            rewardText[2].GetComponentInChildren<Text>().text = "DeadSurvivor : " + PlayerPrefs.GetInt("DeadSurv");

            finalReward += PlayerPrefs.GetInt("Hang") + PlayerPrefs.GetInt("DeadSurv");
        }

        if (PlayerPrefs.GetInt("Result") == 1) finalReward *= 2;

        finalText.text = "FinalReward : " + finalReward;
    }
}
