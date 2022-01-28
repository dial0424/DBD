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

                    //���콺 input�ư�, ������ ������ �ƴϰ�, �� �������� �ʾ��� ���
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

                    //repairTime�� �����缭 generatorIsRepair���� false�� �Ǹ� �÷��̾�� ���� �ǰ���
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

        //lever������ �Դ�����, �����̴��� üũ
        if (lever != null)
        {
            if (isMoveStop)
            {
                //��ȣ�ۿ� ������ �����Ⱑ ��� ������ ������ ����
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
                        Debug.Log("�÷��̾� ó��");
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
