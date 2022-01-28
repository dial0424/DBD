using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rigid = null;

    public Generator generator = null;
    public Board board = null;
    public Lever lever = null;

    [SerializeField]
    private GameObject gaugeBar = null;

    float speed = 10f;
    float repairPower = 0.02f;
    public bool isRepair = false;

    //public bool isNear = false;

    private bool isMoveStop = false;
    private bool isOpen = false;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();

        RepairGenerator();
        BoardAttack();
        LeverDown();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); //x
        float v = Input.GetAxisRaw("Vertical"); //z

        Vector3 vel = new Vector3(h, 0, v);
        rigid.velocity = vel * speed;

        if (h == 0 && v == 0) isMoveStop = true;
        else isMoveStop = false;
    }

    void RepairGenerator()
    {
        if (generator != null)
        {            

            if (isMoveStop)
            {

                    //마우스 input됐고, 발전기 스턴이 아니고, 다 고쳐지지 않았을 경우
                    if (Input.GetMouseButtonDown(0)&&!generator.isStun&&!generator.isClear)
                {
                    isRepair = true;
                    generator.obj.SetActive(false);
                    gaugeBar.SetActive(true);

                }

                if (isRepair)
                {
                    generator.GaugeUp(repairPower);
                    gaugeBar.GetComponent<GeneratorGaugeBar>().setGauge(generator.gauge);

                    //repairTime을 못맞춰서 generatorIsRepair값이 false가 되면 플레이어에도 전달 되게함
                    FromGenerator();

                    if (generator.isClear == true)
                    {
                        gaugeBar.SetActive(false);
                        generator = null;
                        isRepair = false;
                    }

                }

                if (Input.GetMouseButtonUp(0) && isRepair == true)
                {
                    isRepair = false;
                    gaugeBar.SetActive(false);
                    generator.obj.SetActive(true);
                }
            }
            else
            {
                isRepair = false;
                gaugeBar.SetActive(false);
                if (!generator.isStun) generator.obj.SetActive(true);
                
            }
        }
    }

    void FromGenerator()
    {
        if (generator.isStun == true)
        {
            gaugeBar.SetActive(false);
            generator.obj.SetActive(false);

            isRepair = false;
        }
    }

    void BoardAttack()
    {
        if (board != null)
        {
            if (Input.GetKeyDown(KeyCode.Space)&&board.isDown==false)
            {
                Debug.Log("space");
                board.isPush = true;
                board = null;
            }
        }
    }

    void LeverDown()
    {

        //lever가까이 왔는지와, 움직이는지 체크
        if (lever != null)
        {
            if (isMoveStop)
            {
                //상호작용 했을때 발전기가 모두 고쳐져 있으면 실행
                if (Input.GetMouseButtonDown(0) && lever.canUse == true)
                {
                    isOpen = true;
                    lever.text.SetActive(false);
                    gaugeBar.SetActive(true);
                }
                if (isOpen)
                {
                    lever.LeverGaugeUp();
                    gaugeBar.GetComponent<GeneratorGaugeBar>().setGauge(1f - lever.leverGauge);

                    if (lever.isExitOpen)
                    {
                        gaugeBar.SetActive(false);
                        //Destroy(lever.text);
                        lever = null;
                        Debug.Log("플레이어 처리");
                        isOpen = false;
                    }
                }
                if (Input.GetMouseButtonUp(0)&&isOpen==true)
                {
                    isOpen = false;
                    lever.text.SetActive(true);
                    gaugeBar.SetActive(false);
                }
            }
            else
            {
                isOpen = false;
                gaugeBar.SetActive(false);
                lever.text.SetActive(true);
            }     
        }
    }

}
