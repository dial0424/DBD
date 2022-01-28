using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murder : MonoBehaviour
{
    private Rigidbody rigid = null;
    public Generator generator = null;
    public Board board = null;

    float speed = 10f;
    
    float brokenPower = 0.2f; //판자 20%부숨
    public float stunCoolTime = 10.0f;

    public bool isStun = false;

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        move();
        AttackGenerator();
        Stun();
        
    }

    void move()
    {
        float h = 0f;
        float v = 0f;

        if (!isStun)
        {
            h = Input.GetAxisRaw("Horizontal"); //x
            v = Input.GetAxisRaw("Vertical"); //z
        }

        Vector3 vel = new Vector3(h, 0, v);
        rigid.velocity = vel * speed;
    }

    void AttackGenerator()
    {
        if (generator != null)
        {
            if (generator.isClear == false)
            {
                if (Input.GetKeyDown(KeyCode.Space) && generator.canDestroy)
                {
                    //스페이스바 알림 지움
                    Destroy(generator.obj);
                    generator.BrokenGenerator(brokenPower);
                }
            }
        }
    }

    void Stun()
    {
        if (isStun)
        {
            if (stunCoolTime <= 0)
            {
                stunCoolTime = 10.0f;
                Debug.Log("스턴 풀림");
                isStun = false;
            }
            stunCoolTime -= Time.deltaTime;
        }
    }

}
