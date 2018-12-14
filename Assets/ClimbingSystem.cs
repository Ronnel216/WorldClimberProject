using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    [SerializeField]
    Vector3[] edgeTop;

    [SerializeField]
    Vector3[] edgeBottom;

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

    [SerializeField]
    float handMovementSpdFactor = 0.03f;

    GameObject forwardHand; // 進行方向の手
    GameObject backHand;    // 進行方向と逆の手

    Vector3[] gripPoint;

    CharacterJoint grippingJoint;

    //[SerializeField]
    //GameObject grippingAbleArea;

    //bool isGrip;    // 仮のフラグ    ステイトにあとで変えます
    // グリップ情報をまとめるクラス作ってもいいかも

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
        //isGrip = true;

        // 掴んでいる地形
        gripPoint = edgeTop;

        //var funcs = new System.Action < HitMessageSender, UnityEngine.Collider >[3];
        //funcs[0] = (HitMessageSender a, UnityEngine.Collider b) => { };
        //funcs[1] = (HitMessageSender sender, UnityEngine.Collider collider) => 
        //{

        //};
        //funcs[2] = (HitMessageSender a, UnityEngine.Collider b) => { };
        //HitMessageSender.AddHitMessageSender(grippingAbleArea, funcs, new string[] { });



        handMovementMode = HandMovementMode.Close;

        // 進む方向の手を定義する
        ClimberMethod.SetHandForwardAndBack(ref rightHand, ref leftHand);

        // 体を制御 -----
        grippingJoint = gripAnchar.GetComponent<CharacterJoint>();
        forwardHand = rightHand; backHand = leftHand;
        Vector3[] handsPos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
        ClimberMethod.InitgrippingAnchar(grippingJoint, (handsPos[0] + handsPos[1]) / 2, -Vector3.down * 2);
    }

    void FixedUpdate()
    {
        //if (isGrip == false) return;

        // 体を制御 -----
        var handsPos = ClimberMethod.GetHandsPosition(rightHand, leftHand);

        // 崖つかまり時の　アンカー設定
        ClimberMethod.SetgrippingAnchar(grippingJoint, (handsPos[0] + handsPos[1]) / 2);

        // アンカーの状態を反映する
        ClimberMethod.ApplygrippingAnchar(rigid, gripAnchar.GetComponent<Rigidbody>());

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Display");
            //isGrip = false;
        }

        return;
        
        //if ((rigid.transform.position - handsPos[1]).magnitude < armLength && (rigid.transform.position - handsPos[0]).magnitude < armLength) return;

        //rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.0f, rigid.velocity.z);

        //// 手の方向に力を加える
        //rigid.AddForceAtPosition(handsPos[1] - rigid.transform.position, leftArmRoot.transform.position, ForceMode.Impulse);
        //rigid.AddForceAtPosition(handsPos[0] - rigid.transform.position, rightArmRoot.transform.position, ForceMode.Impulse);

    }

    // Update is called once per frame
    void Update () {

        //if (isGrip == false) return;

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

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            gripPoint = edgeTop;
            var pos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
            forwardHand.transform.position = new Vector3(pos[0].x, gripPoint[0].y, pos[0].z);
            backHand.transform.position = new Vector3(pos[1].x, gripPoint[0].y, pos[1].z);
            Vector3[] handsPos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
            ClimberMethod.InitgrippingAnchar(grippingJoint, (handsPos[0] + handsPos[1]) / 2, -Vector3.down * 2);

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            gripPoint = edgeBottom;
            var pos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
            forwardHand.transform.position = new Vector3(pos[0].x, gripPoint[0].y, pos[0].z);
            backHand.transform.position = new Vector3(pos[1].x, gripPoint[0].y, pos[1].z);
            Vector3[] handsPos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
            ClimberMethod.InitgrippingAnchar(grippingJoint, (handsPos[0] + handsPos[1]) / 2, -Vector3.down * 2);

        }

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
                    ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, gripPoint[0]);
                    moveVec = gripPoint[0] - gripPoint[1];

                }
                else if (inputMovement.x < 0)
                {
                    ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, gripPoint[1]);
                    moveVec = gripPoint[1] - gripPoint[0];

                }
                else
                {
                    // 入力なしで後方の手を近づける
                    handMovementMode = HandMovementMode.Close;
                    break;
                }

                // 手同士の距離が最大を超えた時　後方の手を近づける状態に遷移
                if (maxHandToHand < magnitudeHandToHand)
                {
                    handMovementMode = HandMovementMode.Close;
                    break;
                }

                // 前方の手を進める
                if (maxHandToHand > magnitudeHandToHand)
                {
                    var result = ClimberMethod.CalcLerpTranslation(
                        moveVec.normalized,
                        magnitudeHandToHand, 
                        handMovementSpdFactor);
                    forwardHand.transform.Translate(result);
                }
                break;

            case HandMovementMode.Close:
                // 前方の手を進める状態に遷移
                bool isInputMovement = inputMovement.x > 0 || inputMovement.x < 0;
                if (isInputMovement)
                {
                    if (minHandToHand > magnitudeHandToHand)
                    {
                        handMovementMode = HandMovementMode.Advance;
                        break;
                    }
                }

                // 後方の手を近づける
                if (minHandToHand < magnitudeHandToHand)
                {
                    moveVec = forwardHand.transform.position - backHand.transform.position;

                    float limit = -minHandToHand / 2;
                    var result = ClimberMethod.CalcLerpTranslation(
                        moveVec.normalized, 
                        magnitudeHandToHand - limit, 
                        handMovementSpdFactor);
                    backHand.transform.Translate(result);
                }
                break;

            default:
                // 通らないはず
                Debug.Log("HandMovementMode == None");
                break;
        }

    }
}
