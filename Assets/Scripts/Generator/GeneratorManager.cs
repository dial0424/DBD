using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GeneratorManager : MonoBehaviourPun
{
    [SerializeField] private GameObject[] generators;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material shaderMaterial;
    [SerializeField] private List<Vector3> spawnPos;

    //[SerializeField] private List<GameObject> generatorList = new List<GameObject>();
    private Generator currentGen = null;

    [SerializeField] private int generatorCount = 0;

    [SerializeField] Text generatorCountText = null;

    public delegate void LeverCallBack();
    public LeverCallBack leverCallback = null;

    private void Awake()
    {
        generatorCount = generators.Length;
    }

    void Start()
    {
        if (generators.Length > 0)
        {
            for (int i = 0; i < generators.Length; i++)
            {
                generators[i].GetComponent<Generator>().GeneratorCallBack = GeneratorGaugeUpCall;
                generators[i].GetComponent<Generator>().BrokenGenCallBack = BrokenGeneratorBack;
            }

            generatorCountText.text = generatorCount.ToString();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < generators.Length; i++)
            {
                int rand = Random.Range(0, spawnPos.Count);
                photonView.RPC("TakeAPosition", RpcTarget.All, i, rand);

                MeshRenderer[] mts = generators[i].GetComponentsInChildren<MeshRenderer>();
                foreach (var mt in mts)
                {
                    mt.material = shaderMaterial;
                }
            }
        }
    }

    [PunRPC]
    public void TakeAPosition(int _num,int _rand)
    {
        generators[_num].transform.position = spawnPos[_rand];
        spawnPos.RemoveAt(_rand);
    }

    public void GeneratorGaugeUpCall(GameObject _currGenerator, float _playerRepairPower)
    {
        if (_playerRepairPower == 0f)
        {
            photonView.RPC("GeneratorClearRpc", RpcTarget.All);
            return;
        }

        currentGen = _currGenerator.GetComponent<Generator>();
        RPCManager.GeneratorGaugeUpRpc(currentGen, _playerRepairPower);
    }

    [PunRPC]
    public void GeneratorClearRpc()
    {
        int count = generators.Length;
        foreach(var ge in generators)
        {
            if (ge.GetComponent<Generator>().isClear == true)
            {
                count--;

                if (PhotonNetwork.IsMasterClient)
                {
                    GameObject clearParticle = PhotonNetwork.Instantiate("ClearGenerator", ge.transform.position + new Vector3(-1f, 1.5f, 0f), Quaternion.identity);
                }

                ge.GetComponent<Generator>().canDestroy = false;
            }
        }

        generatorCount = count;
        generatorCountText.text = generatorCount.ToString();

        //������ �� ������ ��
        if (generatorCount == 0)
        {
            if (photonView.IsMine)
            {
                //���� ��� ���� ������ Ŭ���̾�Ʈ�� �˸�
                photonView.RPC("CanUseLeverCall", RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    public void CanUseLeverCall()
    {
        //���� �Ŵ��� ��ũ��Ʈ�� �Լ� ��
        leverCallback?.Invoke();

        //ui�ٲ��ִ� rpc���
        photonView.RPC("ExitUiChange", RpcTarget.All);
    }

    [PunRPC]
    public void ExitUiChange()
    {
        generatorCountText.transform.parent.GetChild(0).gameObject.SetActive(false);
        generatorCountText.text = "�ⱸ�� ã�� ������";
    }

    public void BrokenGeneratorBack(GameObject _currGenerator,float _power)
    {
        currentGen = _currGenerator.GetComponent<Generator>();
        RPCManager.BrokenGeneratorRpc(currentGen, _power);
    }
}
