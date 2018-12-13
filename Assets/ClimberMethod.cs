using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クライマー操作のメソッド群
/// </summary>
static public class ClimberMethod
{
    // 移動スティック入力（右スティック）    カメラの向きが反映される
    static public Vector2 GetInputMovement()
    {
        Vector2 result = Vector2.zero;
        if (Input.GetKey(KeyCode.RightArrow)) result += Vector2.right;
        if (Input.GetKey(KeyCode.LeftArrow)) result += Vector2.left;
        if (Input.GetKey(KeyCode.UpArrow)) result += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow)) result += Vector2.down;
        return result;
    }

    // 仮　進方向の手をforwardと定義する
    static public void SetHandForwardAndBack(ref GameObject forwardHand, ref GameObject backHand, Vector3 target = new Vector3())
    {
        forwardHand = forwardHand;
        backHand = backHand;
    }

    // 0 : forward  1 : back
    static public Vector3[] GetHandsPosition(GameObject forwardHand, GameObject backHand)
    {
        return new Vector3[]{ forwardHand.transform.position, backHand.transform.position };
    }

    static public void InitGripingAnchar(CharacterJoint joint, Vector3 connected, Vector3 anchar)
    {
        joint.connectedAnchor = connected;
        joint.anchor = anchar;
        joint.autoConfigureConnectedAnchor = false;
    }

    static public void SetGripingAnchar(CharacterJoint joint, Vector3 connected)
    {
        // 崖つかまり時の　アンカー設定
        joint.connectedAnchor = connected;        
    }

    static public void ApplyGripingAnchar(Rigidbody rigid, Rigidbody anchar)
    {
        // アンカーの状態を反映する
        rigid.transform.position = anchar.GetComponent<Rigidbody>().transform.position;
        rigid.transform.rotation = anchar.GetComponent<Rigidbody>().transform.rotation;
    }
}
