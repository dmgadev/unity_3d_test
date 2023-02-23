using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 targetPosOffset = new Vector3(0, 0, 0);
        public float lookSmooth = 100f;
        public float distanceFromTarget = -8;
        public float zoomSmooth = 100;
        [HideInInspector]
        public float firstPersonCamDistance = -1; // на этом растоянии камера прячется в сферу
        public float maxZoom = -4;
        public float minZoom = -15;

        public bool smoothFollow = true;
        public float smooth = 0.05f;

        // camera collision additions
        [HideInInspector]
        public float newDistance = -8; // set by zoom input
        [HideInInspector]
        public float adjustmentDistance = -8;
        // camera collision additions
    }

    [System.Serializable]
    public class OrbitSettings
    {
        public float xRotation = -20;
        public float yRotation = -180;
        public float maxXRotation = 25;
        public float minXRotation = -85;
        public float vOrbitSmooth = 150;
        public float hOrbitSmooth = 150;
    }

    [System.Serializable]
    public class InputSettings
    {
        public string ORBIT_HORIZONTAL_SNAP = "OrbitHorizontalSnap";
        public string ORBIT_HORIZONTAL = "OrbitHorizontal";
        public string ORBIT_VERTICAL = "OrbitVertical";
        public string ZOOM = "Mouse ScrollWheel";
    }

    public PositionSettings position = new PositionSettings();
    public OrbitSettings orbit = new OrbitSettings();
    public InputSettings input = new InputSettings();

    // camera collision additions
    public CollisionHandler collision = new CollisionHandler();
    // camera collision additions

    Vector3 targetPos = Vector3.zero;
    Vector3 destination = Vector3.zero;

    // camera collision additions
    Vector3 adjustedDestination = Vector3.zero;
    Vector3 camVel = Vector3.zero;
    // camera collision additions

    float vOrbitInput, hOrbitInput, zoomInput, hOrbitSnapInput;

    void Start()
    {
        MoveToTarget();

        // camera collision additions
        collision.Initialize(Camera.main);
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);
        // camera collision additions
    }

    void GetInput()
    {
        vOrbitInput = Input.GetAxisRaw(input.ORBIT_VERTICAL);
        hOrbitInput = Input.GetAxisRaw(input.ORBIT_HORIZONTAL);
        hOrbitSnapInput = Input.GetAxisRaw(input.ORBIT_HORIZONTAL_SNAP);
        zoomInput = Input.GetAxisRaw(input.ZOOM);
    }

    void Update()
    {
        GetInput();
        ZoomInOnTarget();
    }

    void FixedUpdate()
    {
        // moving
        MoveToTarget();
        // rotating
        LookAtTarget();
        OrbitTarget();

        // camera collision additions
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

        collision.CheckColliding(targetPos); // using raycasts here
        position.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
        if (position.adjustmentDistance < Mathf.Abs(position.maxZoom))
        {
            position.adjustmentDistance = Mathf.Abs(position.firstPersonCamDistance); // если какой-то объект перекрывает камеру и она приближается слишком близко к сфере - переключаемся в 1ое лицо
        }
        
        /*
        Debug.Log("ad:" + position.adjustmentDistance);
        Debug.Log("tp:" + transform.position);
        Debug.Log("de:" + destination);

        // camera collision additions

        // draw debug lines
        for (int i = 0; i < 5; i++)
        {
            Debug.DrawLine(targetPos, collision.desiredCameraClipPoints[i], Color.white);
            Debug.DrawLine(targetPos, collision.adjustedCameraClipPoints[i], Color.green);
        }
        */
    }

    void MoveToTarget()
    {
        targetPos = target.position + Vector3.up * position.targetPosOffset.y + Vector3.forward * position.targetPosOffset.z + transform.TransformDirection(Vector3.right * position.targetPosOffset.x);
        destination = Quaternion.Euler(orbit.xRotation, orbit.yRotation/* + target.eulerAngles.y*/, 0) * -Vector3.forward * position.distanceFromTarget;
        destination += targetPos;

        if(collision.colliding)
        {
            adjustedDestination = Quaternion.Euler(orbit.xRotation, orbit.yRotation/* + target.eulerAngles.y*/, 0) * Vector3.forward * position.adjustmentDistance;
            adjustedDestination += targetPos;

            if (position.smoothFollow)
            {
                // use smooth damp function
                transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
            }
            else
                transform.position = adjustedDestination;
        }
        else
        {
            if (position.smoothFollow)
            {
                // use smooth damp function
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
            }
            else
                transform.position = destination;
        }
    }

    void LookAtTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, position.lookSmooth * Time.deltaTime);
    }

    void OrbitTarget()
    {
        if (hOrbitSnapInput > 0)
        {
            orbit.yRotation = -180;
        }

        orbit.xRotation += -vOrbitInput * orbit.vOrbitSmooth * Time.deltaTime;
        orbit.yRotation += -hOrbitInput * orbit.hOrbitSmooth * Time.deltaTime;

        if (orbit.xRotation > orbit.maxXRotation)
        {
            orbit.xRotation = orbit.maxXRotation;
        }
        if (orbit.xRotation < orbit.minXRotation)
        {
            orbit.xRotation = orbit.minXRotation;
        }
    }

    void ZoomInOnTarget()
    {
        float buffCurPos = position.distanceFromTarget;
        position.distanceFromTarget += zoomInput * position.zoomSmooth * Time.deltaTime;

        if (position.distanceFromTarget > position.maxZoom && buffCurPos <= position.maxZoom)
        {
            position.distanceFromTarget = position.firstPersonCamDistance;
        }

        if (position.distanceFromTarget > position.firstPersonCamDistance)
        {
            position.distanceFromTarget = position.firstPersonCamDistance;
        }

        if (position.distanceFromTarget < position.firstPersonCamDistance && buffCurPos >= position.firstPersonCamDistance)
        {
            position.distanceFromTarget = position.maxZoom;
        }

        if (position.distanceFromTarget < position.minZoom)
        {
            position.distanceFromTarget = position.minZoom;
        }
    }

    [System.Serializable]
    public class CollisionHandler
    {
        public LayerMask collisionLayer;

        [HideInInspector]
        public bool colliding = false;
        [HideInInspector]
        public Vector3[] adjustedCameraClipPoints;
        [HideInInspector]
        public Vector3[] desiredCameraClipPoints;

        Camera camera;

        public void Initialize(Camera cam)
        {
            camera = cam;
            adjustedCameraClipPoints = new Vector3[5];
            desiredCameraClipPoints = new Vector3[5];
        }

        public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
        {
            if (!camera)
                return;

            // clear the contents of intoArray
            intoArray = new Vector3[5];

            float z = camera.nearClipPlane;
            float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
            float y = x / camera.aspect;

            // top left
            intoArray[0] = (atRotation * new Vector3(-x, y, z)) + cameraPosition; // added and rotated to the point relative to the camera
            // top right
            intoArray[1] = (atRotation * new Vector3(x, y, z)) + cameraPosition; // added and rotated to the point relative to the camera
            // bottom left
            intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + cameraPosition; // added and rotated to the point relative to the camera
            // bottom right
            intoArray[3] = (atRotation * new Vector3(x, -y, z)) + cameraPosition; // added and rotated to the point relative to the camera
            // camera's position
            intoArray[4] = cameraPosition - camera.transform.forward;
        }

        bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
        {
            for (int i = 0; i < clipPoints.Length; i++)
            {
                Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
                float distance = Vector3.Distance(clipPoints[i], fromPosition);
                if (Physics.Raycast(ray,distance,collisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        public float GetAdjustedDistanceWithRayFrom(Vector3 from)
        {
            float distance = -1;

            for (int i = 0; i < desiredCameraClipPoints.Length; i++)
            {
                Ray ray = new Ray(from, desiredCameraClipPoints[i] - from);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (distance == -1)
                        distance = hit.distance;
                    else
                    {
                        if (hit.distance < distance)
                            distance = hit.distance;
                    }
                }
            }

            if (distance == -1)
                return 0;
            else
                return distance;
        }

        public void CheckColliding(Vector3 targetPosition)
        {
            if (CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition))
            {
                colliding = true;
            }
            else
            {
                colliding = false;
            }
        }
    }
}
