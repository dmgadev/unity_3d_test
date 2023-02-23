using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTest : MonoBehaviour
{
    //private float speed = 10;
    //gameObject.transform.position += new Vector3(speed*Time.deltaTime, 0 , 0); // works

    public GameObject mazeGenerator;
    private MazeGenerationScript mazeGenerationScriptRef;

    public GameObject wayIndicator;

    private Vector3 moveToPosition = new Vector3(0, 0, 0);

    private Vector2 stackPos; // что достаем из стека

    private Vector2 prevPos; // храним предыдущее содержимое того, что достали из стека

    private bool preparationsSet = false;

    private float platformSpeed = 2f; // шаг сетки лабиринта = 5, поэтому этот шаг будем умножать на скорость и на dT

    private bool movingInProcess = false;

    private float cheatTime = 0;

    void Start()
    {
        mazeGenerationScriptRef = mazeGenerator.gameObject.GetComponent<MazeGenerationScript>();
    }

    /*private void FixedUpdate()
    {
        if ((mazeGenerationScriptRef.mazeGenerated == true) && (gameObject.transform.position == moveToPosition))
        {
            // код
        }
    }*/
    void FixedUpdate()
    {
        if (Time.time > 5)
        {
            if (mazeGenerationScriptRef.mazeGenerated == true)
            {
                if (preparationsSet == false)
                {
                    prevPos = mazeGenerationScriptRef.wayPointsCopied.Pop(); // получаем начальную точку маршрута и заносим ее обратно в стек
                    mazeGenerationScriptRef.wayPointsCopied.Push(prevPos);
                    prevPos = stackPos;
                    preparationsSet = true;
                }

                if (preparationsSet == true && mazeGenerationScriptRef.wayPointsCopied.Count != 0 && movingInProcess == false && Time.time - cheatTime > 0.25)
                {
                    prevPos = stackPos;
                    stackPos = mazeGenerationScriptRef.wayPointsCopied.Pop();
                    moveToPosition = new Vector3((1000 + stackPos.x * 5f) - gameObject.transform.position.x, 0, (1000 + stackPos.y * 5f) - gameObject.transform.position.z);
                    //gameObject.transform.position = new Vector3(1000 + stackPos.x * 5f, 0, 1000 + stackPos.y * 5f);
                    movingInProcess = true;
                }

                if (movingInProcess == true)
                {
                    //gameObject.transform.position += new Vector3(10 * Time.deltaTime, 0, 0);
                    if ((Mathf.Abs(gameObject.transform.position.x - (1000 + stackPos.x * 5f)) < 0.1)
                    && (Mathf.Abs(gameObject.transform.position.z - (1000 + stackPos.y * 5f)) < 0.1))
                    {
                        movingInProcess = false;
                        cheatTime = Time.time; // делаем небольшую задержку, чтобы не было дерганий
                    }
                    else
                    {
                        gameObject.transform.position += moveToPosition * platformSpeed * Time.deltaTime;
                    }
                }
            }
        }
    }
}
