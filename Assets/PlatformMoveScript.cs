using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMoveScript : MonoBehaviour
{
    public GameObject mazeGenerator;
    private MazeGenerationScript mazeGenerationScriptRef;

    public GameObject wayIndicator;

    private bool movementStarted = false;

    private Vector3 moveToPosition = new Vector3(0, 0, 0);
    private Quaternion basicRotation = new Quaternion(0, 0, 0, 0);

    private Vector2 stackPos; // что достаем из стека

    private int stackLength;

    private Vector2 prevPos; // храним предыдущее содержимое того, что достали из стека

    void Start()
    { 
        mazeGenerationScriptRef = mazeGenerator.gameObject.GetComponent<MazeGenerationScript>();
    }

    void Update()
    {
        if (mazeGenerationScriptRef.mazeGenerated == true && movementStarted == false)
        {
            InvokeRepeating("MovePlatform", 3f, 0.25f); // перемещает платформу в следующую клетку
            movementStarted = true;
            stackLength = mazeGenerationScriptRef.wayPointsCopied.Count;
        }
    }

    private void MovePlatform()
    {
        if (mazeGenerationScriptRef.wayPointsCopied.Count == stackLength)
        {
            stackPos = mazeGenerationScriptRef.wayPointsCopied.Pop();
            gameObject.transform.position = new Vector3(1000 + stackPos.x * 5f, 0, 1000 + stackPos.y * 5f);
            prevPos = stackPos;
        }
        else
        {
            stackPos = mazeGenerationScriptRef.wayPointsCopied.Pop();
            gameObject.transform.position = new Vector3(1000 + stackPos.x * 5f, 0, 1000 + stackPos.y * 5f);
            Instantiate(wayIndicator, new Vector3(1000 + prevPos.x * 5f, 0, 1000 + prevPos.y * 5f), basicRotation);
            prevPos = stackPos;
        }

        if (mazeGenerationScriptRef.wayPointsCopied.Count == 0)
        {
            CancelInvoke("MovePlatform");
            //Debug.Log("action finished");
        }
    }
}
