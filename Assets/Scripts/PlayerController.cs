using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private float swordTimer = 0.2f;
    private float damageTimer = 0.1f;
    private bool hit = false;
    public GameObject sword;
    public GameObject start;
    public static bool swung = false;
    public static int health;
    public float speed = 5.0f;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 swordPos;
    private Vector3 startPos;
    private Quaternion originalRot;
    Scene scene;
    public GameOverManager gameOverManager;
    // Start is called before the first frame update
    void Start()
    {
        scene = SceneManager.GetActiveScene();
        startPos = start.transform.localPosition;
        swordPos = sword.transform.localPosition;
        health = 3;
        EventManager.OnRestart += OnDeath;
    }

    void OnDisable()
    {
        EventManager.OnRestart -= OnDeath;
    }

    public void OnDeath()
    {
        swung = false;
        hit = false;
        //SceneManager.LoadScene(scene.buildIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (swung)
        {
            swordTimer -= Time.deltaTime;
            if (swordTimer <= 0.0f)
            {
                swordTimer = 0.2f;
                swung = false;
                sword.transform.localPosition = swordPos;
            }
        }

        if (hit)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0.0f)
            {
                damageTimer = 0.1f;
                hit = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !swung)
        {
            swung = true;
            sword.transform.localPosition = startPos;
        }
    }

    void FixedUpdate()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirect = new Vector3(horizontalInput, 0, verticalInput);

        transform.Translate(moveDirect * speed * Time.deltaTime, Space.World);

        if (moveDirect != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirect), 1.0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile" && !hit)
        {
            health--;
            hit = true;
            if (health <= 0)
            {
                Time.timeScale = 0;
                gameOverManager.SetGameOver();
            }
        }
    }
}
