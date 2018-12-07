using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerFollow : MonoBehaviour {

    [SerializeField]
    GameObject player = null;

    public enum CameraMode
    {
        LookAndSlideAxixY,
        HighSpeedFollow
    }

    public CameraMode mode = CameraMode.LookAndSlideAxixY;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pos = transform.position;
        float forwardLevel = 0.001f;

        switch (mode)
        {
            case CameraMode.LookAndSlideAxixY:
                forwardLevel = 0.001f;
                transform.position = new Vector3(pos.x, player.transform.position.y, pos.z);
                transform.LookAt(Vector3.Lerp(transform.position + transform.forward, player.transform.position, forwardLevel));
                break;

            case CameraMode.HighSpeedFollow:
                float followLevel = 0.1f;
                const float distanceFromPlayer = 10.0f;
                const float distanceFromWall = 0.5f;
                forwardLevel = 0.1f;
                Vector3 cameraToPlayer = player.transform.position - transform.position;
                pos = player.transform.position 
                    - cameraToPlayer.normalized * (distanceFromPlayer + (cameraToPlayer.magnitude - distanceFromPlayer) * followLevel) 
                    + Vector3.back * distanceFromWall;
                transform.position = pos;
                transform.LookAt(Vector3.Lerp(transform.position + transform.forward, player.transform.position, forwardLevel));
                break;

            default:
                break;
        }	
	}
}
