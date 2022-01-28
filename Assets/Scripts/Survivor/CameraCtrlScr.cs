using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrlScr : MonoBehaviour
{
    public Vector3 offset;
    [SerializeField] private GameObject survivor;

    public float followSpeed = 0.15f;
    public float speed = 10f;
    
    [SerializeField] private SurvivorState survivorState;

    private void Awake()
    {
        offset = new Vector3(0, 5f, -3f);
    }

    void Start()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        survivorState = survivor.GetComponent<SurvivorScr>().survivorState;

        Vector3 camera_pos = survivor.transform.position + offset;
        Vector3 lerp_pos = Vector3.Lerp(transform.position, camera_pos, followSpeed);
        transform.position = lerp_pos;

        transform.RotateAround(survivor.transform.position, Vector3.up,
                                            Input.GetAxis("Mouse X") * speed * Time.deltaTime);

        offset = transform.position - survivor.transform.position;
    }

    void Update()
    {
        /*survivorState = survivor.GetComponent<SurvivorScr>().survivorState;

        Vector3 camera_pos = survivor.transform.position + offset;
        Vector3 lerp_pos = Vector3.Lerp(transform.position, camera_pos, followSpeed);
        transform.position = lerp_pos;

        transform.RotateAround(survivor.transform.position, Vector3.up,
                                            Input.GetAxis("Mouse X") * speed * Time.deltaTime);

        offset = transform.position - survivor.transform.position;*/
    }
}
