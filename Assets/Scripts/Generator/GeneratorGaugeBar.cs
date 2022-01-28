using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorGaugeBar : MonoBehaviour
{
    public GameObject bar = null;
    private Image gaugeBar = null;
    private bool isRepair = false;



    void Start()
    {
        gaugeBar = this.GetComponent<Image>();
        bar.SetActive(isRepair);
    }

    void Update()
    {

    }

    public void setGauge(float GeneratorGauge)
    {
        gaugeBar.fillAmount = 1.0f-GeneratorGauge;
    }
}
