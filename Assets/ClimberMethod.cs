using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クライマー操作のメソッド群
/// </summary>
static public class ClimberMethod
{
    // 汎用 ---------------
    static void Swap<Type>(ref Type a, ref Type b)
    {
        Type c = a;
        a = b;
        b = c;
    }

    // 専用 ---------------

    static public Vector3 CalcLerpTranslation(Vector3 direction, float length,float step)
    {
        return direction.normalized * length * step;
    }

    static public Vector3 CalcLerpTranslation(Vector3 translation, float step)
    {
        return translation * step;
    }

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

    // 進方向の手をforwardと定義する
    static public void SetHandForwardAndBack(ref GameObject forwardHand, ref GameObject backHand, Vector3 target = new Vector3())
    {
        if ((forwardHand.transform.position - target).sqrMagnitude <
            (backHand.transform.position - target).sqrMagnitude)
            return;

        // 手の参照を入れ替える
        Swap<GameObject>(ref forwardHand, ref backHand);
 
    }

    // 0 : forward  1 : back
    static public Vector3[] GetHandsPosition(GameObject forwardHand, GameObject backHand)
    {
        return new Vector3[]{ forwardHand.transform.position, backHand.transform.position };
    }

    static public void InitgrippingAnchar(CharacterJoint joint, Vector3 connected, Vector3 anchar)
    {
        joint.connectedAnchor = connected;
        joint.anchor = anchar;
        joint.autoConfigureConnectedAnchor = false;
    }

    static public void SetgrippingAnchar(CharacterJoint joint, Vector3 connected)
    {
        // 崖つかまり時の　アンカー設定
        joint.connectedAnchor = connected;        
    }

    static public void ApplygrippingAnchar(Rigidbody rigid, Rigidbody anchar)
    {
        // アンカーの状態を反映する
        var ancharRigid = anchar.GetComponent<Rigidbody>();
        rigid.transform.position = ancharRigid.transform.position;
        rigid.transform.rotation = ancharRigid.transform.rotation;
        rigid.velocity = ancharRigid.velocity;

    }
}
