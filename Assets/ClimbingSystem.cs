using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    [SerializeField]
    Vector3[] edge;

    [SerializeField]
    GameObject leftHand;

    [SerializeField]
    GameObject rightHand;

    [SerializeField]
    float armLength = 1f;

    Rigidbody rigid;

    GameObject activeHand = null;

    float handMovement = 0.1f;

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();
	}

    void FixedUpdate()
    {        
        // 体を制御 -----
        Vector3 leftPos = leftHand.transform.position, rightPos = rightHand.transform.position;

        if ((rigid.transform.position - leftPos).magnitude < armLength && (rigid.transform.position - rightPos).magnitude < armLength) return;

        // 手の方向に力を加える
        rigid.AddForce(leftPos - rigid.transform.position, ForceMode.Impulse);
        rigid.AddForce(rightPos - rigid.transform.position, ForceMode.Impulse);

    }

    // Update is called once per frame
    void Update () {
        float magnitudeHandToHand = (rightHand.transform.position - leftHand.transform.position).magnitude;
        bool canAdvanceStep = magnitudeHandToHand < handMovement  /*次のコメントは無関係*//*手の幅＋余裕を持たせて*/;

        activeHand = null;
        Vector3 moveVec = Vector3.zero;
        // 崖掴む状態 ---
        if (Input.GetKey(KeyCode.RightArrow))
        {
            activeHand = canAdvanceStep ? rightHand : leftHand;
            moveVec = edge[0] - edge[1];
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            activeHand = canAdvanceStep ? leftHand : rightHand;
            moveVec = edge[1] - edge[0];
        }

        if (activeHand == null)
        {
            handMovement = 0.1f;
            return;
        };

        handMovement = armLength + 2;
        activeHand.transform.Translate(moveVec.normalized * 0.1f);

    }
}
