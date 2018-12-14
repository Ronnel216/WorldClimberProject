using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripingWallState : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void FixedUpdate()
    {
        //// 体を制御 -----
        //var handsPos = ClimberMethod.GetHandsPosition(rightHand, leftHand);

        //// 崖つかまり時の　アンカー設定
        //ClimberMethod.SetGripingAnchar(gripingJoint, (handsPos[0] + handsPos[1]) / 2);

        //// アンカーの状態を反映する
        //ClimberMethod.ApplyGripingAnchar(rigid, gripAnchar.GetComponent<Rigidbody>());

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    Debug.Log("Display");
        //    rigid.AddForce(Vector3.up, ForceMode.VelocityChange);
        //}

        //return;

        //if ((rigid.transform.position - handsPos[1]).magnitude < armLength && (rigid.transform.position - handsPos[0]).magnitude < armLength) return;

        //rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.0f, rigid.velocity.z);

        //// 手の方向に力を加える
        //rigid.AddForceAtPosition(handsPos[1] - rigid.transform.position, leftArmRoot.transform.position, ForceMode.Impulse);
        //rigid.AddForceAtPosition(handsPos[0] - rigid.transform.position, rightArmRoot.transform.position, ForceMode.Impulse);

    }

    // Update is called once per frame
    void Update () {
        //float magnitudeHandToHand = (rightHand.transform.position - leftHand.transform.position).magnitude;
        ///*
        // * X = 手の長さ * 2 * 調整値, Y = X - 移動距離
        // * それぞれの状態に遷移する距離として近づける距離をXとして進める距離をYとして　X > 移動距離 > Y
        // * 進む方向の手を進める   ・・・ 入力直後はこれからスタート または　距離がY以上で遷移
        // * 反対の手を近づける    ・・・ 距離がX以上で遷移 または　入力が無いときに遷移
        // */
        ////canAdvanceStep = magnitudeHandToHand < handMovement;
        //float maxHandToHand = armLength * 2 * 0.7f;         // X
        //float minHandToHand = maxHandToHand - handMovement; // Y

        //var inputMovement = ClimberMethod.GetInputMovement();


        //Vector3 moveVec = Vector3.zero;
        ////GameObject forwardHand = null;
        //switch (handMovementMode)
        //{
        //    //case HandMovementMode.Stay:
        //    //    if (minHandToHand < magnitudeHandToHand)
        //    //    {
        //    //        float distance = magnitudeHandToHand - minHandToHand;
        //    //        distance *= 0.1f;
        //    //        leftHand.transform.Translate(moveVec.normalized * distance);
        //    //    }
        //    //    break;

        //    case HandMovementMode.Advance:
        //        if (inputMovement.x > 0)
        //        {
        //            ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, edge[0]);
        //            moveVec = edge[0] - edge[1];

        //        }
        //        else if (inputMovement.x < 0)
        //        {
        //            ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, edge[1]);
        //            moveVec = edge[1] - edge[0];

        //        }
        //        else
        //        {
        //            // 入力なしで後方の手を近づける
        //            handMovementMode = HandMovementMode.Close;
        //            break;
        //        }

        //        // 手同士の距離が最大を超えた時　後方の手を近づける状態に遷移
        //        if (maxHandToHand < magnitudeHandToHand)
        //        {
        //            handMovementMode = HandMovementMode.Close;
        //            break;
        //        }

        //        // 前方の手を進める
        //        if (maxHandToHand > magnitudeHandToHand)
        //        {
        //            var result = ClimberMethod.CalcLerpTranslation(
        //                moveVec.normalized,
        //                magnitudeHandToHand,
        //                handMovementSpdFactor);
        //            forwardHand.transform.Translate(result);
        //        }
        //        break;

        //    case HandMovementMode.Close:
        //        // 前方の手を進める状態に遷移
        //        bool isInputMovement = inputMovement.x > 0 || inputMovement.x < 0;
        //        if (isInputMovement)
        //        {
        //            if (minHandToHand > magnitudeHandToHand)
        //            {
        //                handMovementMode = HandMovementMode.Advance;
        //                break;
        //            }
        //        }

        //        // 後方の手を近づける
        //        if (minHandToHand < magnitudeHandToHand)
        //        {
        //            moveVec = forwardHand.transform.position - backHand.transform.position;

        //            float limit = -minHandToHand / 2;
        //            var result = ClimberMethod.CalcLerpTranslation(
        //                moveVec.normalized,
        //                magnitudeHandToHand - limit,
        //                handMovementSpdFactor);
        //            backHand.transform.Translate(result);
        //        }
        //        break;

        //    default:
        //        // 通らないはず
        //        Debug.Log("HandMovementMode == None");
        //        break;
        //}
    }
}
