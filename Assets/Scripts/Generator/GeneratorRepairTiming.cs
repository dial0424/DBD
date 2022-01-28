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
    float randRot = 0f; //정해진 구간의 중간점
    float t = 0f;
    bool isStop = false; //

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStop) //멈추기 전에는 회전
        {
            t += Time.deltaTime / speed;
            if (t >= 1) //멈추기 전에 한바퀴가 다 지나면 타이밍 실패
            {
                currGenerator.BrokenGeneratorCall(0.1f);
                //currGenerator.BrokenGenerator(0.1f);

                if (!currGenerator.isStun)
                    currGenerator.GetComponentInChildren<ParticleSystem>().Play();

                currGenerator.isStun = true;

                this.gameObject.SetActive(false);
            }

            //화살표 돌려주기
            float angle = Mathf.Lerp(0f, 360f, t);
            arrowTr.localRotation = Quaternion.Euler(0f, 0f, -angle);       
        }

        //스페이스 눌러서 타이밍 맞추기
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStop = true;
            //true가 되면 화살표 회전이 멈춤

            if (!((360 - randRot) - 22.5 < arrowTr.eulerAngles.z && arrowTr.eulerAngles.z < (360 - randRot) + 22.5))
            {

                currGenerator.BrokenGeneratorCall(0.1f);

                if (!currGenerator.isStun)
                    currGenerator.GetComponentInChildren<ParticleSystem>().Play();

                currGenerator.isStun = true;
            }

            //과정이 끝나면 ui꺼줌
            this.gameObject.SetActive(false);
        }
    }

    //ui세팅 될때 값을 초기화 해줌
    public void SetCircleRotation()
    {
        randRot = Random.Range(0.0f, 360.0f);
        circleTr.localRotation = Quaternion.Euler(0f, 0f, -randRot);
        arrowTr.localRotation = Quaternion.Euler(0f, 0f, 0f);
        t = 0;
        isStop = false;
    }
}
