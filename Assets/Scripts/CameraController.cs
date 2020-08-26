using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour{
    public float speed, mouseLookSpeedY, mouseLookSpeedX;
    Transform camTransform;
    Vector3 movementDirection;
    Vector2 rawMoveDir, mouseLookDir;
    Vector3 targetRotation;
    Rigidbody rb;
    float cameraPitch, camPan;
    const float MIN_CAM_PITCH = -40f;
    const float MAX_CAM_PITCH = 80f;

    private void Start() {
        DontDestroyOnLoad(this);
        rb = GetComponent<Rigidbody>();
        camTransform = GetComponentInChildren<Camera>().transform;
    }
    private void FixedUpdate() {
        LookAround();
        Movement();
    }
    public void Move(InputAction.CallbackContext context) {
        if(context.started || context.canceled || context.performed) {
            rawMoveDir = context.ReadValue<Vector2>();
        }
    }

    public void Look(InputAction.CallbackContext context) {
        if (context.started || context.canceled || context.performed) {
            mouseLookDir = context.ReadValue<Vector2>();
        }
    }

    private void Movement() {
        //explanation comment 
        /*
         * move along the forward/backward and horizontal axes relative to where the camera is looking. we do this by
         * getting the x value from the input callback context and then multiplying it by the transform.right of the camera,
         * this gets us a vector which points to the right of the camera.
         * then we do the same with the forward/backward axis, by getting the y component of the callback context value and multiplying that by the camera's
         * transform.front we then create a vector3 using those 2 scalar values and use that to translate the rb
         */
        movementDirection = rawMoveDir.x * camTransform.right;
        movementDirection += rawMoveDir.y * camTransform.forward;
        movementDirection.Normalize();
        rb.MovePosition(transform.position + movementDirection * Time.fixedDeltaTime * speed);
    }

    private void LookAround() {
        //calculate vertical angle;
        cameraPitch += -mouseLookDir.y * Time.fixedDeltaTime * mouseLookSpeedY;
        cameraPitch = Mathf.Clamp(cameraPitch, MIN_CAM_PITCH, MAX_CAM_PITCH);

        //calculate horizontal angle
        camPan += mouseLookDir.x * Time.fixedDeltaTime * mouseLookSpeedX;

        targetRotation = new Vector3(cameraPitch, camPan);
        camTransform.rotation = Quaternion.Euler(targetRotation);
    }
}