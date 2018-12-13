using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    [SerializeField]
    Vector3[] edge;

    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject leftArmRoot;

    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject rightArmRoot;

    [SerializeField]
    GameObject gripAnchar;

    [SerializeField]
    float armLength = 1f;

    Rigidbody rigid;

    [SerializeField]
    float handMovement = 0.1f;

    GameObject forwardHand; // 進行方向の手
    GameObject backHand;    // 進行方向と逆の手

    CharacterJoint gripingJoint;

    enum HandMovementMode
    {
        None,
        Stay,
        Advance,
        Close,
    }
    HandMovementMode handMovementMode;

    class HandTrasControlCmd
    {
        GameObject hand;
        Vector3 target;
        float safeErrorDistance = Mathf.Epsilon;    // 許容誤差

        bool Execute(float step)
        {
            Vector3 moveVec = target - hand.transform.position;
            hand.transform.Translate(moveVec.normalized * step);
            return moveVec.sqrMagnitude <= safeErrorDistance * safeErrorDistance;
        }
    }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();

        handMovementMode = HandMovementMode.Close;

        // 進む方向の手を定義する
        ClimberMethod.SetHandForwardAndBack(ref rightHand, ref leftHand);

        // 体を制御 -----
        gripingJoint = gripAnchar.GetComponent<CharacterJoint>();
        Vector3[] handsPos = ClimberMethod.GetHandsPosition(rightHand, leftHand);
        ClimberMethod.InitGripingAnchar(gripingJoint, (handsPos[0] + handsPos[1]) / 2, -Vector3.down * 2);
    }

    void FixedUpdate()
    {
        // 体を制御 -----
        var handsPos = ClimberMethod.GetHandsPosition(rightHand, leftHand);

        // 崖つかまり時の　アンカー設定
        ClimberMethod.SetGripingAnchar(gripingJoint, (handsPos[0] + handsPos[1]) / 2);

        // アンカーの状態を反映する
        ClimberMethod.ApplyGripingAnchar(rigid, gripAnchar.GetComponent<Rigidbody>());

        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Display");
            rigid.AddForce(Vector3.up, ForceMode.VelocityChange);
        }

        return;
        
        if ((rigid.transform.position - handsPos[1]).magnitude < armLength && (rigid.transform.position - handsPos[0]).magnitude < armLength) return;

        rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.0f, rigid.velocity.z);

        // 手の方向に力を加える
        rigid.AddForceAtPosition(handsPos[1] - rigid.transform.position, leftArmRoot.transform.position, ForceMode.Impulse);
        rigid.AddForceAtPosition(handsPos[0] - rigid.transform.position, rightArmRoot.transform.position, ForceMode.Impulse);

    }

    // Update is called once per frame
    void Update () {
        float magnitudeHandToHand = (rightHand.transform.position - leftHand.transform.position).magnitude;
        /*
         * X = 手の長さ * 2 * 調整値, Y = X - 移動距離
         * それぞれの状態に遷移する距離として近づける距離をXとして進める距離をYとして　X > 移動距離 > Y
         * 進む方向の手を進める   ・・・ 入力直後はこれからスタート または　距離がY以上で遷移
         * 反対の手を近づける    ・・・ 距離がX以上で遷移 または　入力が無いときに遷移
         */
        //canAdvanceStep = magnitudeHandToHand < handMovement;
        float maxHandToHand = armLength * 2 * 0.7f;         // X
        float minHandToHand = maxHandToHand - handMovement; // Y

        var inputMovement = ClimberMethod.GetInputMovement();


        Vector3 moveVec = Vector3.zero;
        //GameObject forwardHand = null;
        switch (handMovementMode)
        {
            //case HandMovementMode.Stay:
            //    if (minHandToHand < magnitudeHandToHand)
            //    {
            //        float distance = magnitudeHandToHand - minHandToHand;
            //        distance *= 0.1f;
            //        leftHand.transform.Translate(moveVec.normalized * distance);
            //    }
            //    break;

            case HandMovementMode.Advance:
                if (inputMovement.x > 0)
                {
                    forwardHand = rightHand;
                    backHand = leftHand;
                    moveVec = edge[0] - edge[1];

                }
                else if (inputMovement.x < 0)
                {
                    forwardHand = leftHand;
                    backHand = rightHand;
                    moveVec = edge[1] - edge[0];

                }
                else
                {
                    handMovementMode = HandMovementMode.Close;
                    break;
                }
          
                if (maxHandToHand - 0.05f < magnitudeHandToHand)
                    handMovementMode = HandMovementMode.Close;


                if (maxHandToHand > magnitudeHandToHand)
                {

                    float distance = maxHandToHand - magnitudeHandToHand;
                    distance *= 0.1f;
                    forwardHand.transform.Translate(moveVec.normalized * distance);
                }
                break;

            case HandMovementMode.Close:
                if (inputMovement.x > 0)
                {
                    if (minHandToHand + 0.05f > magnitudeHandToHand)
                    {
                        handMovementMode = HandMovementMode.Advance;
                        break;
                    }
                }
                else if (inputMovement.x < 0)
                {
                    if (minHandToHand + 0.05f > magnitudeHandToHand)
                    {
                        handMovementMode = HandMovementMode.Advance;
                        break;
                    }
                }
                else
                {
                }


                if (minHandToHand < magnitudeHandToHand)
                {
                    //Vector3 moveVec = Vector3.zero;
                    moveVec = forwardHand.transform.position - backHand.transform.position;

                    float distance = magnitudeHandToHand - minHandToHand;
                    distance *= 0.1f;
                    backHand.transform.Translate(moveVec.normalized * distance);
                }
                break;

            default:
                // 通らないはず
                Debug.Log("HandMovementMode == None");
                break;
        }

    }
}
