using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Camera_fix : MonoBehaviour
{

    public float cameraSpeed = 5.0f;

    public GameObject player;

    private void Start()
    {

    }

    private void Update()
    {
        if (player == null) return;
        Vector3 dir = player.transform.position - this.transform.position;
        Vector3 moveVector = new Vector3(dir.x * cameraSpeed * Time.deltaTime, dir.y * cameraSpeed * Time.deltaTime, 0.0f);
        this.transform.Translate(moveVector);
    }
}
