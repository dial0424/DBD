using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//20210714_±è¶õ
public class FlashlightScr : MonoBehaviour
{
    public float time = 3f;

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0f) gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        time = 3f;
    }
}
