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

    public GameObject repairTimingObject = null;//�����̽��� Ÿ�̹� ui
    
    public bool generatorIsRepair = false;//�����Ⱑ �������� �������� Ȯ��
    public bool PisNear = false;//�÷��̾ ������ �ִ��� Ȯ��
    public bool isClear = false;//������ �� �ƴ��� Ȯ��
    public bool isStun = false;//Ÿ�̸� ��������� Ȯ��
    public bool canDestroy = false;//������ ��ȣ�ۿ��� ���θ� ���� ������ ���� Ȯ��
                                   //�����ڰ� ��ȣ�ۿ� : true, ���θ��� ��ȣ�ۿ� : false

    //Ÿ�̹��� 3��, �������� ����, ������ 10�� �̻�
    private int repairTimingCount = 3;
    private float[] repairTiming = new float[3];

    public GameObject text = null;
    public GameObject spaceNotice = null;
    //text, spaceNotice�� private���� ���� �ؾ���
    //text �̸� mouseleftClick���� �̸� ���� �ؾ���
    public GameObject aaa = null;

    public GameObject obj = null;// ui�� ��� text�� ���� ������Ʈ

    public AudioSource genAudioSource = null;

    void Start()
    {
        repairTimingObject.SetActive(false);

        if (!PhotonNetwork.IsMasterClient)
        {
            gauge = 1.0f;
            setRepairTime();//�����̽��� ���� Ÿ�̹� ����
        }

    }

    void Update()
    {
        if (generatorIsRepair)
        {
            SetRepairTimeUi();//repairTiming�� ui ����
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
        //isclear rpc���
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

    //�����⿡ Ÿ�̹� ui �ߴ� �ð��� �����ִ� �Լ� (start�Լ�)
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

    //������ �ð��� �°� Ÿ�̸� ui����
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

    //Ÿ�ֿ̹� �����ϸ� �����Ⱑ ���ϰɸ�
    public void StunGenerator()
    {
        if (isStun)
        {
            if (coolTime <= 0) //��Ÿ���� �� ������
            {
                coolTime = 3.0f;
                isStun = false; 
                
            }
            coolTime -= Time.deltaTime;
        }
    }

    //���θ��� �����⿡�� �����̽� ������ ȣ���
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
