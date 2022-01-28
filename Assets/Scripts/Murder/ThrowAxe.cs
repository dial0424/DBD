using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ThrowAxe : MonoBehaviourPun
{
    private float speed = 20f;

    private void Start()
    {
        Destroy(this.gameObject, 5f);
    }

    private void Update()
    {
        transform.position += (transform.forward * Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if (other.CompareTag("SURVIVOR") && other.GetComponentInChildren<SurvivorN>().sLife > 1)
            {
                other.GetComponentInChildren<SurvivorN>().CallMaster("DamageCall");
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
    }
}
