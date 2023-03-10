using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

using Debug = UnityEngine.Debug;
using UnityEngine.Networking;
using System;
using System.Runtime.CompilerServices;

public class EnemyBehavior : MonoBehaviour
{
    private Color originalColor;
    public Color damageColor;
    public Color stunColor;
    private Renderer renderer;
    private float flashDuration;
    private GameObject player;
    public GameObject resetPane;
    public GameObject pushIcon;
    public GameObject completePane;
    public float completeDelayTime = 4.0f;
    private bool isBossBeat = false;
    //public static float projectileTime = 2.0f;
    private float resetPaneTMAX = 2.0f;
    private float resetPanetimer = 0;
    //public Light light;
    [SerializeField]
    private int originalHealth;
    public static int health;
    public float knockBack = 5.0f;
    public static bool wallTouch = false;
    //public static bool fired = false;
    public bool startDelay = true;
    public float knockBackTimer = 0.5f;
    public static float shockTimer = 6.0f;
    private float shockDecrement = 1.0f;
    public GameObject stunCanvas;
    private TextMeshProUGUI stunTextMesh;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Rigidbody rb;
    private bool hitSword;
    Scene scene;
    public int no_of_tries = 0;
    public float[,] locations = new float[100, 100];
    private string URL_blocks = "https://docs.google.com/forms/u/1/d/e/1FAIpQLSfkVBkGzZ9kS2AIiRUbRBfmfkyHXdaP1gnOObQaXEaadvs1GQ/formResponse";
    private string URL_L4 = "https://docs.google.com/forms/u/1/d/e/1FAIpQLSfnr02I1SZJkgIjl6Pm0_Z6kY-BfdR60gd4iLn1elLoUgcKRg/formResponse";
    private string URL_Level = "https://docs.google.com/forms/u/1/d/e/1FAIpQLSe9F3Kx3zY_TaWaLRi-p7lrBwy7QgaZIN9U9pWKzSgKO7c1EQ/formResponse";
    private string URL = "https://docs.google.com/forms/u/1/d/e/1FAIpQLSd8oLR_OzoW0uwK2OmbssF7nDTgOBva4IXGj-17CwF3ZJrFSg/formResponse";
    private string _sessionID;
    private Vector3 temploc;
    private int level = 0;
    private int temphits = 0;
    private int nforward = 0, npushable = 0, nstun = 0, nblock = 0, healthred = 0, ndirecthits = 0;
    private GameObject[] fblock;
    public static bool completed = false;
    private String blocks_pref = "";


    public enum State
    {
        NORMAL,
        HIT,
        COMBO,
    }

    public static State st = State.NORMAL;

    private void Awake()
    {
        health = originalHealth;
        stunTextMesh = stunCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        //rb.isKinematic = false;
        flashDuration = 0.5f;
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;
        damageColor = Color.Lerp(Color.white, Color.yellow, 0.25f);
        stunColor = Color.Lerp(Color.red, Color.yellow, 0.75f);
        renderer.material.color = originalColor;
        stunTextMesh.text = "";
        completePane.SetActive(false);
        player = GameObject.FindWithTag("Player");
        originalPos = transform.position;
        originalRot = transform.rotation;
        EventManager.OnRestart += OnDeath;
        rb = gameObject.GetComponent<Rigidbody>();
        //rb.isKinematic = false;
        scene = SceneManager.GetActiveScene();
        resetPane.SetActive(false);
        pushIcon.SetActive(false);
        isBossBeat = false;
        _sessionID = DateTime.Now.Ticks.ToString();
        completed = false;
        PlayerPrefs.DeleteKey("pushed");
        PlayerPrefs.DeleteKey("pulled");

}

void OnDisable()
    {
        EventManager.OnRestart -= OnDeath;
    }

    public void OnDeath()
    {
        temploc = GameObject.Find("Player").transform.position;
        locations[no_of_tries, 0] = temploc.x;
        locations[no_of_tries, 1] = temploc.y;
        locations[no_of_tries, 2] = temploc.z;
        if (PlayerPrefs.HasKey(_sessionID+"blocks"))
        {
            if (scene.buildIndex == 13 || scene.buildIndex == 15)
            {
                StartCoroutine(Post_blocks(_sessionID));
                Debug.Log("Sending stuff on death");
            }
            
        }
        PlayerPrefs.DeleteKey(_sessionID+"blocks");
        //UnityEngine.Debug.Log("no_of_tries: "+no_of_tries+" "+ Math.Abs(level));
        //UnityEngine.Debug.Log(locations[no_of_tries, 0]+" "+locations[no_of_tries, 1]+" "+locations[no_of_tries, 2]+" "+(no_of_tries + 1));
        if (no_of_tries >= 0)
        {
            if (scene.buildIndex == 13 || scene.buildIndex == 15)
            UnityEngine.Debug.Log("no_of_tries: " + no_of_tries + " " + Math.Abs(level));
            StartCoroutine(Post_tries(_sessionID));
        }

        no_of_tries = 0;
        PlayerPrefs.DeleteKey("pushed");
        PlayerPrefs.DeleteKey("pulled");
        if (!isBossBeat)
        {
            Restart();
        }
    }

    IEnumerator OnComplete()
    {
        if (nforward + npushable + nstun != 0)
        {
            
            UnityEngine.Debug.Log("OUTTTF"+nforward+"  P"+npushable+"  S"+nstun+"  B"+nblock+"   health"+healthred+"  Level"+scene.buildIndex);
            blocks_pref = PlayerPrefs.GetString(_sessionID+"blocks");
            print("blockprefs"+blocks_pref);
            blocks_pref += "F"+nforward+"P"+npushable+"S"+nstun+"B"+nblock+"D"+temphits+"H"+healthred+"L"+scene.buildIndex;
            PlayerPrefs.DeleteKey(_sessionID+"blocks");
            PlayerPrefs.SetString(_sessionID+"blocks", blocks_pref);
        }
        UnityEngine.Debug.Log("OUTTTno_of_tries: " + no_of_tries + " " + Math.Abs(level));
        if (no_of_tries >= 0)
        {
            if (scene.buildIndex != 13 || scene.buildIndex != 15)
                UnityEngine.Debug.Log("no_of_tries: " + no_of_tries + " " + Math.Abs(level));
            StartCoroutine(Post_tries(_sessionID));
        }
        if (PlayerPrefs.HasKey(_sessionID+"blocks"))
        {
            //StartCoroutine(Post_blocks(_sessionID));
            if (scene.buildIndex != 13 && scene.buildIndex != 15)
            {
                Debug.Log("SEnding block data");
                StartCoroutine(Post_blocks(_sessionID));
            }
        }
        
        nforward = 0;
        npushable = 0;
        nstun = 0;
        nblock = 0;
        healthred = 0;
        ndirecthits = 0;
        
        PlayerPrefs.DeleteKey(_sessionID+"blocks");
        UnityEngine.Debug.Log("Boss Beat"+ scene.buildIndex);
        completePane.SetActive(true);
        rb.velocity = Vector3.zero;
        transform.position = new Vector3(1000.0f, 0.0f, 0.0f);
        rb.constraints = RigidbodyConstraints.FreezePosition;
        completed = true;
        PlayerPrefs.DeleteKey("pushed");
        PlayerPrefs.DeleteKey("pulled");
        Debug.Log("complete LEVELLLLL");
        StartCoroutine(Post_Level(_sessionID));
        yield return new WaitForSeconds(completeDelayTime);
        beatBoss();
    }

    IEnumerator DamageFlash()
    {
        renderer.material.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        renderer.material.color = originalColor;
    }

    IEnumerator StunShockFlash()
    {
        renderer.material.color = stunColor;
        yield return new WaitForSeconds(flashDuration);
        renderer.material.color = originalColor;
    }

    private void StunTime()
    {
        shockTimer -= shockDecrement;
        //Debug.Log(shockTimer.ToString("0"));

        if (shockTimer <= 0.0f)
        {
            CancelInvoke("StunTime");
            stunTextMesh.text = "";

            if (!isBossBeat)
            {
                Restart();
            }
        }
        else
        {
            StartCoroutine(StunShockFlash());
            stunTextMesh.text = shockTimer.ToString("0");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wallTouch && shockTimer > 0.0f)
        {
            if (!IsInvoking("StunTime"))
            {
                InvokeRepeating("StunTime", 1.0f, 1.0f);
            }
        }
        else
        {
            CancelInvoke("StunTime");
            stunTextMesh.text = "";
        }

        if (st == State.HIT)
        {
            if (ChaseBlock.chasing)
            {
                ChaseBlock.chasing = false;
            }

            knockBackTimer -= Time.deltaTime;

            if (knockBackTimer <= 0.0f)
            {
                if (SwitchOn.on)
                {
                    ChaseBlock.chasing = true;
                }

                if (!isBossBeat)
                {
                    Restart();
                }
            }
        }
        else if (st == State.COMBO)
        {
            if (ChaseBlock.chasing)
            {
                ChaseBlock.chasing = false;
            }
        }

        if (resetPane.activeSelf)
        {
            resetPanetimer -= Time.deltaTime;

            if (resetPanetimer <= 0.0f)
            {
                resetPane.SetActive(false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Sword" && PlayerController.swung && !PlayerController.swordHit)
        {
            PlayerController.swordHit = true;
            pushIcon.SetActive(false);
            rb.velocity = Vector3.zero;
            rb.AddForce(other.gameObject.transform.forward * knockBack, ForceMode.Impulse);
            st = State.HIT;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Sword" && PlayerController.swung && !PlayerController.swordHit)
        {
            //rb.isKinematic = false;
            //UnityEngine.Debug.Log("Collission happens now");
            ndirecthits += 1;
            temphits = ndirecthits;
            //UnityEngine.Debug.Log("DIRECT HITTTT"+ndirecthits);
        }
    }

    private void Restart()
    {
        UnityEngine.Debug.Log("REstart");
        //print("restart");
        pushIcon.SetActive(false);
        if (nforward + npushable + nstun != 0)
        {
            UnityEngine.Debug.Log("F"+nforward+"  P"+npushable+"  S"+nstun+"  B"+nblock+" D"+temphits+"   health"+healthred+"  Level"+scene.buildIndex);
            //temphits = ndirecthits;
            //StartCoroutine(Post_blocks(_sessionID));
            blocks_pref = PlayerPrefs.GetString(_sessionID+"blocks");
            blocks_pref += "F"+nforward+"P"+npushable+"S"+nstun+"B"+nblock+"D"+temphits+"H"+healthred+"L"+scene.buildIndex;
            
            PlayerPrefs.DeleteKey(_sessionID+"blocks");
            PlayerPrefs.SetString(_sessionID+"blocks", blocks_pref);
            print("RRRRPREFS"+PlayerPrefs.GetString(_sessionID+"blocks"));
        }

        nforward = 0;
        npushable = 0;
        nstun = 0;
        nblock = 0;
        healthred = 0;
        ndirecthits = 0;

        if (no_of_tries >= 0 && !isBossBeat)
        {
            resetPane.SetActive(true);
            resetPanetimer = resetPaneTMAX;
        }

        rb.velocity = Vector3.zero;
        transform.position = originalPos;
        transform.rotation = originalRot;
        st = State.NORMAL;
        startDelay = true;
        wallTouch = false;
        shockTimer = 6.0f;
        knockBackTimer = 0.5f;
        health = originalHealth;
        completePane.SetActive(false);
        isBossBeat = false;

        if (SwitchOn.on)
        {
            ChaseBlock.chasing = true;
        }

        no_of_tries += 1;
        print("nooftriesrestart"+no_of_tries);
    }

    private void Res()
    {
        if (nforward + npushable + nstun != 0)
        {
            UnityEngine.Debug.Log("F"+nforward+"  P"+npushable+"  S"+nstun+"  B"+nblock+" D"+temphits+"   health"+healthred+"  Level"+scene.buildIndex);
            //temphits = ndirecthits;
            //StartCoroutine(Post_blocks(_sessionID));
            blocks_pref = PlayerPrefs.GetString(_sessionID+"blocks");
            blocks_pref += "F"+nforward+"P"+npushable+"S"+nstun+"B"+nblock+"D"+temphits+"H"+healthred+"L"+scene.buildIndex;
            
            PlayerPrefs.DeleteKey(_sessionID+"blocks");
            PlayerPrefs.SetString(_sessionID+"blocks", blocks_pref);
            print("RRRRPREFS"+PlayerPrefs.GetString(_sessionID+"blocks"));
        }

        nforward = 0;
        npushable = 0;
        nstun = 0;
        nblock = 0;
        healthred = 0;
        ndirecthits = 0;
        
        Debug.Log("Res called");
        rb.velocity = Vector3.zero;
        st = State.NORMAL;
        startDelay = true;
        wallTouch = false;
        shockTimer = 6.0f;
        //fired = false;
        //projectileTime = 2.0f;
        //light.intensity = 0.0f;
        no_of_tries += 1;
        print("RESTRIES"+no_of_tries);
        SceneManager.LoadScene(scene.buildIndex);
        
    }

    private void beatBoss()
    {
        rb.velocity = Vector3.zero;
        st = State.NORMAL;
        startDelay = true;
        wallTouch = false;
        //fired = false;
        completePane.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "PushBlock")
        {
            npushable++;
            
            StartCoroutine(DamageFlash());
            health--;
            healthred++;

            if (health <= 0)
            {
                isBossBeat = true;
                StartCoroutine(OnComplete());
            }
            else
            {
                if (!isBossBeat)
                {
                    Restart();
                }
            }
        }
        else if (other.gameObject.tag == "ForwardBlock")
        {
            nforward++;
            if (st == State.HIT || st == State.NORMAL)
            {
                knockBackTimer = 0.5f;
                st = State.COMBO;
            }

            StartCoroutine(DamageFlash());
            health--;
            healthred++;

            rb.velocity = Vector3.zero;
            rb.AddForce(other.gameObject.transform.forward * knockBack / 2.0f, ForceMode.Impulse);
            pushIcon.SetActive(true);
            pushIcon.GetComponent<RectTransform>().eulerAngles = new Vector3(-90, 0, other.gameObject.transform.eulerAngles.y);
            //Debug.Log(pushIcon.GetComponent<RectTransform>().rotation);
            if (health <= 0)
            {
                isBossBeat = true;
                StartCoroutine(OnComplete());
            }
            else
            {
                //Restart();
            }
        }
        else if (other.gameObject.tag == "StunBlock")
        {
            nstun++;
            //rb.velocity = Vector3.zero;
            pushIcon.SetActive(false);

            if (!wallTouch)
            {
                knockBackTimer = 0.5f;

                StartCoroutine(DamageFlash());
                health--;
                healthred++;

                if (health <= 0)
                {
                    isBossBeat = true;
                    StartCoroutine(OnComplete());
                }
                else
                {
                    rb.velocity = Vector3.zero;
                    st = State.NORMAL;
                    wallTouch = true;
                }
            }
            else
            {
                if (!isBossBeat)
                {
                    Restart();
                }
            }
        }
        else if (other.gameObject.tag == "Block")
        {
            nblock++;
            Restart();
        }
        else if (other.gameObject.tag == "Switch")
        {
            fblock = GameObject.FindGameObjectsWithTag("PushBlock");
            Debug.Log(fblock.Length + "  " + fblock[0].transform.position.x + "   " + fblock[0].transform.position.z);
            StartCoroutine(Post_L4(_sessionID, fblock[0].transform.position));
            Restart();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "StunBlock")
        {
            wallTouch = false;
            shockTimer = 6.0f;
        }
    }

    public IEnumerator Post_tries(string sessionID)
    {
        UnityEngine.Debug.Log("POSTIE");
        WWWForm form = new WWWForm();

        form.AddField("entry.728675539", sessionID);
        form.AddField("entry.1829062957", no_of_tries);
        //for(int t=0;t<no_of_tries+1;t++)
        //{
        form.AddField("entry.1492841611", (temploc.x).ToString());
        form.AddField("entry.475872908", (temploc.y).ToString());
        form.AddField("entry.432244748", (temploc.z).ToString());
        form.AddField("entry.1687831932", PlayerPrefs.GetFloat("pushed", 0).ToString());
        form.AddField("entry.490935050", PlayerPrefs.GetFloat("pulled", 0).ToString());
        //}
        form.AddField("entry.596243283", (scene.buildIndex).ToString());
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form Tries upload complete!");
            }
        }
    }

    public IEnumerator Post_blocks(string sessionID)
    {
        //UnityEngine.Debug.Log("POSTBlocks");
        WWWForm form = new WWWForm();
        blocks_pref = PlayerPrefs.GetString(_sessionID + "blocks");
        print(blocks_pref.Length+"PREFS"+blocks_pref);
        for (int i = 0; i < blocks_pref.Length; i++)
        {
            if (blocks_pref[i] == 'F')
                form.AddField("entry.1986196806", int.Parse(""+blocks_pref[i + 1]));
            else if (blocks_pref[i] == 'P')
            {
                form.AddField("entry.542647140", int.Parse("" + blocks_pref[i + 1]));
            }
            else if (blocks_pref[i] == 'S')
                form.AddField("entry.136227332", int.Parse(""+blocks_pref[i + 1]));
            else if (blocks_pref[i] == 'B')
            {
                form.AddField("entry.511319478", int.Parse("" + blocks_pref[i + 1]));
                //Debug.Log("B here"+int.Parse("" + blocks_pref[i + 1]));
            }
            else if (blocks_pref[i] == 'D')
            {
                form.AddField("entry.657771776", int.Parse("" + blocks_pref[i + 1]));
                //Debug.Log("B here"+int.Parse("" + blocks_pref[i + 1]));
            }
            //else if (blocks_pref[i] == 'D' && blocks_pref[i+2] != 'B')
                //temphits = int.Parse(""+blocks_pref[i + 1] + blocks_pref[i + 2]);
            else if (blocks_pref[i] == 'H')
                form.AddField("entry.188267377", int.Parse(""+blocks_pref[i + 1]));
            else if (blocks_pref[i] == 'L')
            {
                form.AddField("entry.1580530392", sessionID);
                form.AddField("entry.1786672107", int.Parse("" + blocks_pref[i + 1]));
            }
            //UnityEngine.Debug.Log("Player Prefs For loop F"+nforward+"  P"+npushable+"  S"+nstun+"  B"+nblock+"   health"+healthred+"  Level"+scene.buildIndex);
            
            
        }
        //Debug.Log("player prefs"+blocks_pref);
        
        //UnityEngine.Debug.Log("FUNCDIRECT HITTTT"+temphits);
        //UnityEngine.Log("F"+nforward+"  P"+npushable+"  S"+nstun+"  B"+nblock+"   health"+healthred+"  Level"+scene.buildIndex);
        //UnityEngine.Debug.Log(ndirecthits);
        using (UnityWebRequest www = UnityWebRequest.Post(URL_blocks, form))
        {
           
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form Blocks upload complete !!!");
            }
        }
    }

    public IEnumerator Post_L4(string sessionID, Vector3 pos_L4)
    {
        WWWForm form = new WWWForm();

        form.AddField("entry.603090486", sessionID);
        form.AddField("entry.1645207548", (pos_L4.x).ToString());
        form.AddField("entry.974652086", (pos_L4.y).ToString());
        form.AddField("entry.1417563745", (pos_L4.z).ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(URL_L4, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form Level4 upload complete !!!");
            }
        }
    }

    public IEnumerator Post_Level(string sessionID)
    {
        WWWForm form = new WWWForm();

        form.AddField("entry.1950975398", sessionID);
        form.AddField("entry.1281945691", scene.buildIndex);
        using (UnityWebRequest www = UnityWebRequest.Post(URL_Level, form))
        {

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form Post Level upload complete !!!");
            }
        }
    }
}
