using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Lever : MonoBehaviourPun
{
    public bool canUse = false;
    //������ ������ ��� ������ GameManager���� true�� �ٲ���

    public bool isExitOpen = false;

    public float leverGauge = 0;
    private float leverOpnePower = 0.1f;
    private float openTime = 3f;

    [SerializeField]
    private Transform doorTr = null;

    [SerializeField]
    private GameObject textobj = null;
    public GameObject text = null;

    public Animation leverAnim = null;

    //[SerializeField] private GeneratorManager genManager = null;

    // Start is called before the first frame update
    void Start()
    {
        //genManager.leverCallback = CanUseLeverBack;

        leverAnim = this.GetComponent<Animation>();
        //leverAnim.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (isExitOpen&&openTime>=0)
        {

            doorTr.position -= this.transform.right * Time.deltaTime * 1.2f;

            openTime -= Time.deltaTime;

        }
        
    }

    public void LeverGaugeUp()
    {
        if (leverGauge >= 1 && isExitOpen == false)
        {
            //canUse = false; //�������� ������ �ٽ� ��ȣ�ۿ� �� �� ���� ��
            //Destroy(text);
            //isExitOpen = true;

            photonView.RPC("LeverOpenPlzMaster", RpcTarget.MasterClient);

        }
        else
        {
            //leverGauge += leverOpnePower * Time.deltaTime;

            //�����Ϳ��� �˸���
            photonView.RPC("SendLeverGaugeToMaster", RpcTarget.MasterClient);
        }

    }

    [PunRPC]
    public void SendLeverGaugeToMaster()
    {
        //rpc
        photonView.RPC("LeverGaugeUpRpc", RpcTarget.All);
    }

    [PunRPC]
    public void LeverGaugeUpRpc()
    {
        leverGauge += leverOpnePower * Time.deltaTime;
    }

    [PunRPC]
    public void LeverOpenPlzMaster()
    {
        photonView.RPC("LeverOpenAll", RpcTarget.All);
    }

    [PunRPC]
    public void LeverOpenAll()
    {
        canUse = false;
        isExitOpen = true;
    }

    //������ �Ŵ��� ���� �ݹ� �� �Լ�
    //public void CanUseLeverBack()
    //{
    //    photonView.RPC("CanUseLeverAll", RpcTarget.All);
    //}

    //[PunRPC]
    //public void CanUseLeverAll()
    //{
    //    canUse = true;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PLAYER"&&canUse==true)
        {
            other.GetComponent<Player>().lever = this.gameObject.GetComponent<Lever>();
            
            text = Instantiate(textobj, transform.position, Quaternion.identity);
            text.transform.SetParent(GameObject.Find("Canvas").transform);
            text.transform.position = new Vector2(450f, 130f);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PLAYER" && canUse == true)
        {
            other.GetComponent<Player>().lever = null;

            Destroy(text);
        }
    }
}
