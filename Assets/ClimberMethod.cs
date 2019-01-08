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

    static public Vector3 ConvertVec2ToVec3XZ(Vector2 vec)
    {
        Vector3 result = vec;
        Swap(ref result.y, ref result.z);
        return result;
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

    static public Vector2 GetInputMovement3D()
    {
        Vector3 movement = GetInputMovement();
        movement = CameraController.direction * movement;
        return movement;
    }

    // 移動入力方向の掴める場所を取得する
    static public Collider CheckGripPoint(Vector3 movement, SubCollider grippable)
    {
        float minDistanceSqr = float.MaxValue;
        Collider nearGripColi = null;
        Vector3 basePos = grippable.transform.position;
        foreach (var collider in grippable.Colliders)
        {
            Vector3 point = collider.ClosestPoint(basePos + movement);

            // 移動入力方向に存在しない
            if (Vector3.Dot(point - basePos, movement) <= 0) continue;

            float distanceSqr = (point - basePos).sqrMagnitude;
            if (minDistanceSqr > distanceSqr)
            {
                minDistanceSqr = distanceSqr;
                nearGripColi = collider;
            }

        }

        return nearGripColi;

    }

    // 二つの座標から回転姿勢を求める
    static public Quaternion CalcRotationXZ(Vector3 start, Vector3 end)
    {
        var vec = end - start;
        vec.y = 0f;
        return Quaternion.FromToRotation(Vector3.left, vec);
    }

    static public Quaternion CalcRotation(Vector3 start, Vector3 end)
    {
        var vec = end - start;
        return Quaternion.FromToRotation(Vector3.left, vec);
    }

    // 進方向の手をforwardと定義する
    static public void SetHandForwardAndBack(ref GameObject forwardHand, ref GameObject backHand, Vector3 target)
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

    static public void SetGrippingAnchar(Rigidbody connectedRigid, Vector3 position, Quaternion rotation)
    {
        connectedRigid.transform.position = position;
        connectedRigid.transform.rotation = rotation;
    }

    static public void ApplyGrippingAnchar(Rigidbody rigid, Rigidbody anchar)
    {
        // アンカーの状態を反映する
        var ancharRigid = anchar.GetComponent<Rigidbody>();
        rigid.transform.position = ancharRigid.transform.position;
        rigid.transform.rotation = ancharRigid.transform.rotation;
        rigid.velocity = ancharRigid.velocity;

    }
}
