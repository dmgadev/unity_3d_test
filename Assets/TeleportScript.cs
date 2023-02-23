using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    public GameObject teleportDestination;
    public GameObject playerSphere;
    public GameObject playerCamera;

    Vector3 vect;

    float time = 0;

    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider col)// можно сделать регистрацию времени и телепортацию в онтриггерстэй через пару секунд
    {
        if (col.CompareTag("Player"))
        {
            time = Time.time;
            //Debug.Log(time); //Завершить игру
        }
    }

    private void OnTriggerStay(Collider col)// можно сделать регистрацию времени и телепортацию в онтриггерстэй через пару секунд
    {
        if (col.CompareTag("Player") && (Time.time - time >= 2))
        {
            vect = teleportDestination.transform.position - gameObject.transform.position;
            playerSphere.transform.position += vect;
            playerCamera.transform.position += vect;
        }
    }

    private void OnTriggerExit(Collider col)// можно сделать регистрацию времени и телепортацию в онтриггерстэй через пару секунд
    {
        if (col.CompareTag("Player"))
        {
            time = 0;
        }
    }

    void Update()
    {
        
    }
}
