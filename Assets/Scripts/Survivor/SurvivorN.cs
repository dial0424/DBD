using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class SurvivorN : SurvivorScr
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (transform.parent.name.ToString().Equals("Survivor1(Clone)")) treatSpeed = 4f;
        else if (transform.parent.name.ToString().Equals("Survivor2(Clone)")) sRepairPower = 0.08f;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();
    }

    [PunRPC]
    public override void DamageCall()
    {
         damageCallback?.Invoke(this.gameObject);
    }

    [PunRPC]
    public override void HangInCall()
    {
        hangSurvivor?.Invoke(this.gameObject);
    }

    [PunRPC]
    public override void HangHookCall(string _tag)
    {
        hangHook?.Invoke(this.gameObject, _tag);
    }
}
