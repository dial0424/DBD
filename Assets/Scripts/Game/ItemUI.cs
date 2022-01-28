using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image timerImg = null;
    private float fillTime = 0f;
    private bool setAmount = false;

    public float fullTime = 0f;

    // Update is called once per frame
    void Update()
    {
        if(fullTime != 0f)
            SetImageFill();
    }

    private void OnEnable()
    {
        fillTime = 0f;
        timerImg.fillAmount = 0f;
    }

    private void SetImageFill()
    {
        if (fillTime <= fullTime)
        {
            fillTime += Time.deltaTime;
            timerImg.fillAmount += 1 / fullTime * Time.deltaTime;
        }
        else
        {
            fullTime = 0f;
            gameObject.SetActive(false);
        }
    }
}
