using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class HS_DemoShooting : MonoBehaviour
{
    [Header("Fire rate")]
    private int Prefab;
    [Range(0.0f, 1.0f)]
    public float fireRate = 0.1f;
    private float fireCountdown = 0f;

    public GameObject FirePoint;
    public Camera Cam;

    //How far you can point raycast for projectiles
    public float MaxLength;
    public GameObject[] Prefabs;

    private Ray RayMouse;
    private Vector3 direction;
    private Quaternion rotation;

    //Double-click protection
    private float buttonSaver = 0f;

    //For Camera shake 
    public Animation camAnim;

    void Start()
    {
        Counter(0);
    }

    void Update()
    {
        //Single shoot
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool fire1Pressed = false;
        bool mouseRButton = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            fire1Pressed = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
            mouseRButton = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
        }

        if (fire1Pressed)
        {
            if (camAnim != null) camAnim.Play(camAnim.clip.name);
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
        }

        //Fast shooting
        if (mouseRButton && fireCountdown <= 0f)
        {
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
            fireCountdown = fireRate;
        }
        fireCountdown -= Time.deltaTime;

        //To change projectiles
        if (UnityEngine.InputSystem.Keyboard.current != null && buttonSaver >= 0.4f)
        {
            if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed)
            {
                buttonSaver = 0f;
                Counter(-1);
            }
            if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed)
            {
                buttonSaver = 0f;
                Counter(+1);
            }
        }
#else
        if (Input.GetButtonDown("Fire1"))
        {
            if (camAnim != null) camAnim.Play(camAnim.clip.name);
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
        }

        //Fast shooting
        if (Input.GetMouseButton(1) && fireCountdown <= 0f)
        {
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
            fireCountdown = 0;
            fireCountdown += fireRate;
        }
        fireCountdown -= Time.deltaTime;

        //To change projectiles
        if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0) && buttonSaver >= 0.4f)// left button
        {
            buttonSaver = 0f;
            Counter(-1);
        }
        if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0) && buttonSaver >= 0.4f)// right button
        {
            buttonSaver = 0f;
            Counter(+1);
        }
#endif
        buttonSaver += Time.deltaTime;

        //To rotate fire point
        if (Cam != null)
        {
            RaycastHit hit;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mousePos = UnityEngine.InputSystem.Mouse.current != null ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector3.zero;
#else
            var mousePos = Input.mousePosition;
#endif
            RayMouse = Cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(RayMouse.origin, RayMouse.direction, out hit, MaxLength))
            {
                RotateToMouseDirection(gameObject, hit.point);
            }
        }
        else
        {
            Debug.Log("No camera");
        }
    }

    // To change prefabs (count - prefab number)
    void Counter(int count)
    {
        Prefab += count;
        if (Prefab > Prefabs.Length - 1)
        {
            Prefab = 0;
        }
        else if (Prefab < 0)
        {
            Prefab = Prefabs.Length - 1;
        }
    }

    //To rotate fire point
    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        direction = destination - obj.transform.position;
        rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }
}
