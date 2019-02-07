using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Sprites;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {


    bool isPlaying = false;
    SpriteRenderer render = null;

    [SerializeField]
    GameObject player = null;

    GameObject currentPlayer = null;

    [SerializeField]
    CameraController camera = null;

    [SerializeField]
    Transform spawnPoint = null;

    GameObject canvas = null;

    int numClear = 0;
    [SerializeField]
    Text clearText = null;

    [SerializeField]
    GameObject goal = null;

    float clearStay = -1f;

	// Use this for initialization
	void Start () {
        render = GetComponent<SpriteRenderer>();

        canvas = GameObject.Find("Canvas");
	}
	
	// Update is called once per frame
	void Update () {

        clearText.text = "Clear : " + numClear;

        // ゲームオーバー
        if (currentPlayer)
        {
            if (currentPlayer.GetComponentInChildren<ClimbingSystem>().gameObject.transform.position.y < -200)
            {
                camera.target = gameObject;

                Destroy(currentPlayer);
                currentPlayer = null;

                render.enabled = true;
                canvas.SetActive(true);
                isPlaying = false;

            }
        }

        if (isPlaying == false)
        {
            render.gameObject.transform.localScale = Vector3.one + (Vector3.one * 0.1f * (Mathf.Sin(Time.time) + 1)) * 0.1f;
        }

        if (isPlaying == true)
        {
            if ((goal.transform.position - currentPlayer.GetComponentInChildren<ClimbingSystem>().gameObject.transform.position).sqrMagnitude < 4 * 4)
            {
                clearStay += Time.deltaTime;
            }else if (clearStay >= 0)
            {
                clearStay += Time.deltaTime;                
            }

            if (clearStay > 0)
            {
                goal.GetComponent<Light>().intensity = 1 + clearStay;
                goal.GetComponent<Light>().range = 5 + clearStay * 10;
            }


            if (clearStay >= 8f)
            {
                camera.target = gameObject;

                Destroy(currentPlayer);
                currentPlayer = null;

                render.enabled = true;
                canvas.SetActive(true);
                isPlaying = false;
                clearStay = -1f;
                goal.GetComponent<Light>().intensity = 1;
                goal.GetComponent<Light>().range = 5;
                numClear++;
            }
            return;
        };

        // ゲーム開始
        if (Input.GetKeyDown("joystick button 0"))
        {
            currentPlayer = Instantiate(player);
            currentPlayer.transform.position = spawnPoint.position;

            camera.target = currentPlayer.GetComponentInChildren<ClimbingSystem>().gameObject;

            render.enabled = false;
            canvas.SetActive(false);
            isPlaying = true;
        }
	}
}
