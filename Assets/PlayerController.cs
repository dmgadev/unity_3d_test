using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject playerCamera;

    private Vector3 directionVector; // вектор направления взгляда (^)
    private Vector3 directionVectorOpp; // вектор, обратный вектору направления взгляда (v)
    private Vector3 directionVectorNormal; // вектор, перпендикулярный вектору направления взгляда (<)
    private Vector3 directionVectorNormalOpp; // вектор, обратный предыдущему (>)

    private float directionVectorModule; // длина вектора направления взгляда\

    bool sphereIsColliding = false;
    float collisionTime = 0.0f;

    void Start()
    {
        GetDirectionVector(); // получим вектор направления взгляда

        GetOppositeVectors(); // получим вектора, относительно вектора направления взгляда
    }

    // Update is called once per frame
    void Update()
    {
        //print(collisionTime + "|\n");

        /*if (sphereIsColliding == true)
        {*/
        GetDirectionVector(); // получим вектор направления взгляда

        GetOppositeVectors(); // получим вектора, относительно вектора направления взгляда

        //float f = 0.0f;
        Vector3 p = GetBaseInput();

        gameObject.GetComponent<Rigidbody>().AddForce(p, ForceMode.Impulse); // управление шаром задается добавлением силы относительно камеры, однако, при Y камеры = Y шара
        
        /*if (Input.GetKey(KeyCode.Space))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 50, 0), ForceMode.Impulse);
        }*/
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        sphereIsColliding = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        collisionTime = Time.time;
        //print(collisionTime + "|\n"); // восстановить при программировании коллизий
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        sphereIsColliding = false;
        //print("No longer in contact with " + collisionInfo.transform.name); // восстановить при программировании коллизий
    }

    private Vector3 GetBaseInput() //returns the basic values, if it's 0 than it's not active.
    {
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += directionVector;
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += directionVectorOpp;
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += directionVectorNormal;
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += directionVectorNormalOpp;
        }
        return p_Velocity;
    }

    private void GetDirectionVector()
    {
        directionVector = transform.position - playerCamera.transform.position;
        directionVector.y = 0; // чтобы сила прикладывалась горизонтально

        // домножим вектор, чтобы его модуль был равен единице
        directionVectorModule = Mathf.Sqrt(directionVector.x * directionVector.x + directionVector.z * directionVector.z);
        directionVector *= 1 / directionVectorModule;
    }

    private void GetOppositeVectors()
    {
        directionVectorOpp = new Vector3(-directionVector.x, 0, -directionVector.z);

        directionVectorNormal = new Vector3(-directionVector.z, 0, directionVector.x);

        directionVectorNormalOpp = new Vector3(directionVector.z, 0, -directionVector.x);
    }
}
