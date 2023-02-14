using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject swordPrefab;
    public GameObject start;
    public static bool swung = false;
    public static int health = 1;
    public float speed = 5.0f;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 originalPos;
    private Quaternion originalRot;
    Scene scene;

    // Start is called before the first frame update
    void Start()
    {
        scene = SceneManager.GetActiveScene();
        originalPos = transform.position;
        originalRot = transform.rotation;
        EventManager.OnRestart += OnDeath;
    }

    void OnDisable()
    {
        EventManager.OnRestart -= OnDeath;
    }

    public void OnDeath()
    {
        swung = false;
        SceneManager.LoadScene(scene.buildIndex);
    }

    IEnumerator SwordDespawn(GameObject sword)
    {
        yield return new WaitForSeconds(0.2f);
        swung = false;
        sword.transform.localPosition = new Vector3(0.44f, 0, 0.56f);
        //Destroy(sword);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !swung)
        {
            swung = true;
            /*GameObject sword = Instantiate(swordPrefab, start.transform.position, start.transform.rotation);*/
            GameObject sword = GameObject.FindGameObjectWithTag("Sword");
            sword.transform.localPosition += new Vector3(0, 0, 0.3f);
            StartCoroutine(SwordDespawn(sword));
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
        if (other.gameObject.tag == "Projectile")
        {
            health--;
        }
    }
}
