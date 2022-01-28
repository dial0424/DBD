using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorRepairTiming : MonoBehaviour
{
    public Generator currGenerator = null;

    [SerializeField]
    private Transform circleTr = null;
    [SerializeField]
    private Transform arrowTr = null;

    int speed = 2;
    float randRot = 0f; //������ ������ �߰���
    float t = 0f;
    bool isStop = false; //

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStop) //���߱� ������ ȸ��
        {
            t += Time.deltaTime / speed;
            if (t >= 1) //���߱� ���� �ѹ����� �� ������ Ÿ�̹� ����
            {
                currGenerator.BrokenGeneratorCall(0.1f);
                //currGenerator.BrokenGenerator(0.1f);

                if (!currGenerator.isStun)
                    currGenerator.GetComponentInChildren<ParticleSystem>().Play();

                currGenerator.isStun = true;

                this.gameObject.SetActive(false);
            }

            //ȭ��ǥ �����ֱ�
            float angle = Mathf.Lerp(0f, 360f, t);
            arrowTr.localRotation = Quaternion.Euler(0f, 0f, -angle);       
        }

        //�����̽� ������ Ÿ�̹� ���߱�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStop = true;
            //true�� �Ǹ� ȭ��ǥ ȸ���� ����

            if (!((360 - randRot) - 22.5 < arrowTr.eulerAngles.z && arrowTr.eulerAngles.z < (360 - randRot) + 22.5))
            {

                currGenerator.BrokenGeneratorCall(0.1f);

                if (!currGenerator.isStun)
                    currGenerator.GetComponentInChildren<ParticleSystem>().Play();

                currGenerator.isStun = true;
            }

            //������ ������ ui����
            this.gameObject.SetActive(false);
        }
    }

    //ui���� �ɶ� ���� �ʱ�ȭ ����
    public void SetCircleRotation()
    {
        randRot = Random.Range(0.0f, 360.0f);
        circleTr.localRotation = Quaternion.Euler(0f, 0f, -randRot);
        arrowTr.localRotation = Quaternion.Euler(0f, 0f, 0f);
        t = 0;
        isStop = false;
    }
}
