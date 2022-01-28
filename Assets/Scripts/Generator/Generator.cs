using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Generator : MonoBehaviourPun
{
    public delegate void GeneratorDelegate(GameObject currGenerator, float playerRepairGauge);
    public GeneratorDelegate GeneratorCallBack = null;

    public delegate void BrokeGenDelegate(GameObject currGenerator,float _power);
    public BrokeGenDelegate BrokenGenCallBack = null;

    public float gauge = 1.0f;
    private float coolTime = 3.0f;

    public GameObject repairTimingObject = null;//스페이스바 타이밍 ui
    
    public bool generatorIsRepair = false;//발전기가 수리중인 상태인지 확인
    public bool PisNear = false;//플레이어가 가까이 있는지 확인
    public bool isClear = false;//수리가 다 됐는지 확인
    public bool isStun = false;//타이머 못맞췄는지 확인
    public bool canDestroy = false;//마지막 상호작용이 살인마 인지 생존자 인지 확인
                                   //생존자가 상호작용 : true, 살인마가 상호작용 : false

    //타이밍은 3번, 랜덤으로 생성, 간격은 10초 이상
    private int repairTimingCount = 3;
    private float[] repairTiming = new float[3];

    public GameObject text = null;
    public GameObject spaceNotice = null;
    //text, spaceNotice는 private으로 수정 해야함
    //text 이름 mouseleftClick으로 이름 수정 해야함
    public GameObject aaa = null;

    public GameObject obj = null;// ui에 띄울 text를 닮을 오브젝트

    public AudioSource genAudioSource = null;

    void Start()
    {
        repairTimingObject.SetActive(false);

        if (!PhotonNetwork.IsMasterClient)
        {
            gauge = 1.0f;
            setRepairTime();//스페이스바 누를 타이밍 세팅
        }

    }

    void Update()
    {
        if (generatorIsRepair)
        {
            SetRepairTimeUi();//repairTiming에 ui 세팅
        }
        StunGenerator();
        
    }

    public void GaugeUp(float playerRepairGauge)
    {
        if (gauge <= 0f && isClear == false)
        {
            isClear = true;
            photonView.RPC("SendGaugeToMaster", RpcTarget.MasterClient, 0f);
        }
        else
        {
            photonView.RPC("SendGaugeToMaster", RpcTarget.MasterClient, playerRepairGauge);
        }        

    }

    [PunRPC]
    public void SendGaugeToMaster(float _playerRepairPower)
    {
        //isclear rpc쏘기
        if (_playerRepairPower == 0f)
        {
            photonView.RPC("GenIsClear", RpcTarget.All);
        }

        GeneratorCallBack?.Invoke(this.gameObject, _playerRepairPower);
    }

    [PunRPC]
    public void GenIsClear()
    {
        isClear = true;
    }


    public void GaugeUpRpcCall(float _playerRepairPower)
    {
        photonView.RPC("GaugeUpRpc", RpcTarget.All, _playerRepairPower);
    }

    [PunRPC]
    public void GaugeUpRpc(float _playerRepairPower)
    {
        gauge -= _playerRepairPower * Time.deltaTime;
    }

    [PunRPC]
    public void BrokenGenerator(float _brokenPower)
    {
        float g = gauge + (gauge * _brokenPower);
        if (g >= 1f)
        {
            gauge = 1f;
        }
        else
        {
            gauge = g;
        }
    }

    //발전기에 타이밍 ui 뜨는 시간이 정해주는 함수 (start함수)
    private void setRepairTime()
    {
        for (int i = 0; i < repairTimingCount; i++)
        {
            float randNum = Random.Range(0.0f, 5.0f);
            if (i == 0)
            {
                repairTiming[i] = 10.0f + randNum;
            }
            else
            {
                repairTiming[i] = repairTiming[i - 1] + 10.0f + randNum;
            }
        }
    }

    //정해진 시간에 맞게 타이머 ui세팅
    private void SetRepairTimeUi()
    {
        if (repairTimingCount > 0)
        {
            if ( gauge <= repairTiming[repairTimingCount - 1] * 0.02f)
            {
                repairTimingObject.SetActive(true);

                repairTimingObject.GetComponent<GeneratorRepairTiming>().currGenerator = this.gameObject.GetComponent<Generator>();
                repairTimingObject.GetComponent<GeneratorRepairTiming>().SetCircleRotation();

                repairTimingCount--;                

            }
        }
    }

    //타이밍에 실패하면 발전기가 스턴걸림
    public void StunGenerator()
    {
        if (isStun)
        {
            if (coolTime <= 0) //쿨타임이 다 지나면
            {
                coolTime = 3.0f;
                isStun = false; 
                
            }
            coolTime -= Time.deltaTime;
        }
    }

    //살인마가 발전기에서 스페이스 누르면 호출됨
    public void BrokenGeneratorCall(float _power)
    {
        BrokenGenCallBack?.Invoke(this.gameObject,_power);
    }

    public void MasterCanDestroy(bool _change)
    {
        photonView.RPC("CanDestroyAll", RpcTarget.MasterClient, _change);
    }

    [PunRPC]
    public void CanDestroyAll(bool _change)
    {
        photonView.RPC("CanDestroyChange", RpcTarget.All, _change);
    }

    [PunRPC]
    public void CanDestroyChange(bool _change)
    {
        canDestroy = _change;
    }


    public void BrokenGenRPC(float _power)
    {
        photonView.RPC("BrokenGenerator", RpcTarget.All, _power);
    }

    public void PlaySoundAll()
    {
        if (genAudioSource.isPlaying == false) photonView.RPC("PlaySound", RpcTarget.All);

    }

    [PunRPC]
    public void PlaySound()
    {
        genAudioSource.Play();
        Debug.LogError("play");
    }

    public void StopSoundAll()
    {
        if (genAudioSource.isPlaying == true) photonView.RPC("StopSound", RpcTarget.All);

    }

    [PunRPC]
    public void StopSound()
    {
        genAudioSource.Stop();
        Debug.LogError("stop");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SURVIVOR"&&isClear==false)
        {
            PisNear = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "SURVIVOR"&&isClear==false)
        {
            other.GetComponentInChildren<SurvivorN>().sGenerator = null;

            PisNear = false;
        }
    }
}
