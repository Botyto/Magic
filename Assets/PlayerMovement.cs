using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform cameraPivot, targetCamera;
    public float moveSpeed = 1.0f;
    public float maxSpeed = 5.0f;
    public float jumpForce = 15.0f;
    public float mouseSensitivity = 1.0f;
    public float zoomSpeed = 10.0f;
    public float cameraDistance = 14.0f;
    public bool initiallyLockMouse = true;

    public Vector3 cursorPosition;
    public GameObject selectedObject;

    public Rigidbody body {  get { return GetComponent<Rigidbody>(); } }
    public bool mouseLocked { get { return Cursor.lockState == CursorLockMode.Locked; } }

    private void Start()
    {
        if (initiallyLockMouse)
        {
            SetMouseLock(true);
        }
    }
    
    void FixedUpdate()
    {
        //Movement
        HandleMovement();
        if (Mathf.Abs(body.velocity.y) < 0.01f)
        {
            HandleJump();
        }

        //Camera look
        if (mouseLocked)
        {
            HandleMouseOrbit();
        }
        HandleMouseZoom();
        var ray = new Ray(cameraPivot.transform.position, -cameraPivot.transform.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, cameraDistance))
        {
            var cameraPos = targetCamera.localPosition;
            cameraPos.z = -hitInfo.distance + 1.0f;
            targetCamera.localPosition = cameraPos;
        }
        else
        {
            var cameraPos = targetCamera.localPosition;
            cameraPos.z = -cameraDistance;
            targetCamera.localPosition = cameraPos;
        }

        //Camera mode
        HandleToggleMouseLock();

        //Select object
        TrySelectObject();

        //Find cursor position
        ray = new Ray(targetCamera.transform.position, cameraPivot.transform.position - targetCamera.transform.position);
        if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity))
        {
            cursorPosition = hitInfo.point;
        }
        else
        {
            cursorPosition = targetCamera.transform.position + (cameraPivot.transform.position - targetCamera.transform.position).SetLength(cameraDistance + 10);
        }
    }

    #region Movement

    void HandleMovement()
    {   
        Vector3 targetVelocity = new Vector3(Gameplay.GetAxis("Horizontal"), 0, Gameplay.GetAxis("Vertical"));
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= maxSpeed;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = body.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        var maxVelocityChange = maxSpeed * 2.0f;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        body.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void HandleJump()
    {
        if (Gameplay.GetKeyDown(KeyCode.Space))
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    #endregion

    #region Camera look

    void HandleMouseOrbit()
    {
        //Vertical look
        var mouseY = Gameplay.GetAxis("Mouse Y");
        var pivotRot = cameraPivot.localRotation.eulerAngles;
        var pivotRotX = pivotRot.x;
        if (pivotRotX > 180.0f) pivotRotX -= 360.0f;
        pivotRotX = Mathf.Clamp(pivotRotX - mouseY * mouseSensitivity, -10.0f, 90.0f);
        cameraPivot.localRotation = Quaternion.Euler(pivotRotX, 0.0f, 0.0f);

        //Horizontal look
        var mouseX = Gameplay.GetAxis("Mouse X");
        var charRot = transform.localRotation.eulerAngles;
        var charRotY = charRot.y;
        charRotY += mouseX * mouseSensitivity;
        while (charRotY < 0.0f) charRotY += 360.0f;
        while (charRotY > 360.0f) charRotY -= 360.0f;
        transform.localRotation = Quaternion.Euler(charRot.x, charRotY, charRot.z);
    }

    void HandleMouseZoom()
    {
        var mouseScroll = Gameplay.GetAxis("Mouse ScrollWheel");
        cameraDistance -= mouseScroll * zoomSpeed;
    }

    #endregion

    #region Camera mode

    void HandleToggleMouseLock()
    {
        if (Gameplay.GetKeyDown(KeyCode.Escape))
        {
            ToggleMouseLock();
        }
    }

    void ToggleMouseLock()
    {
        SetMouseLock(!mouseLocked);
    }

    void SetMouseLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion

    void TrySelectObject()
    {
        RaycastHit hitInfo;
        Physics.Raycast(targetCamera.transform.position, targetCamera.transform.rotation.eulerAngles, out hitInfo);
        if (hitInfo.distance < 100)
        {
            selectedObject = hitInfo.collider != null ? hitInfo.collider.gameObject : null;
            if (selectedObject != null)
            {
                var selectableComp = selectedObject.GetComponent<Selectable>();
                if (selectableComp == null || !selectableComp.enabled)
                {
                    selectedObject = null;
                }
            }
        }
        else
        {
            selectedObject = null;
        }
    }
}
