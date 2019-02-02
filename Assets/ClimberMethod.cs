using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クライマー操作のメソッド群
/// </summary>
static public class ClimberMethod
{
    // 汎用 ---------------
    static public void Swap<Type>(ref Type a, ref Type b)
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

    static public Vector3 CalcMixVector(Vector3 a, Vector3 b, float aPerAll)
    {
        return a * aPerAll + b * (1 - aPerAll);
    }

    // 専用 ---------------

    static public Vector3 CalcLerpTranslation(Vector3 direction, float length, float step)
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

    static public Vector3 GetInputMovement3D()
    {
        Vector3 movement = GetInputMovement();
        movement = CameraController.direction * movement;
        movement.Normalize();
        return movement;
    }

    // 移動入力方向の掴める場所を取得する
    static public Collider CheckGripPoint(Vector3 moveDir, SubCollider grippableArea, float ableGripCos)
    {
        float minDistanceSqr = float.MaxValue;
        Collider nearGripColi = null;
        Vector3 basePos = grippableArea.transform.position;
        foreach (var collider in grippableArea.Colliders)
        {
            Vector3 point = collider.ClosestPoint(basePos + moveDir);

            // 移動入力方向に存在しない
            if (Vector3.Dot((point - basePos).normalized, moveDir) <= ableGripCos) continue;

            float distanceSqr = (point - basePos).sqrMagnitude;
            if (minDistanceSqr > distanceSqr)
            {
                minDistanceSqr = distanceSqr;
                nearGripColi = collider;
            }

        }

        return nearGripColi;

    }

    static public Collider ChangeGrippablePoint(ref GrippablePoint2 currentGripping, Collider nextGrippingCollider)
    {        
        // 現在掴んでいる場所から離れる
        if (currentGripping != null)
        {
            currentGripping.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");
        }
        currentGripping = nextGrippingCollider.gameObject.GetComponent<GrippablePoint2>();
        currentGripping.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        return nextGrippingCollider;
    }

    class ReleaseGrippablePointAction : ClimbingSystem.Event
    {
        float time = 3.0f;
        public bool Action(ClimbingSystem system)
        {
            
            var currentGripping = system.grippablePoint;
            currentGripping.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");
            return true;
        }
    }


    static public void ReleaseGrippablePoint(ref GrippablePoint2 grippoint)
    {        

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

    //? 現在の実装だと役立たず
    static public void SetHandForwardAndBack(
        ref GameObject forwardHand, ref GameObject backHand,
        GameObject rightHand, GameObject leftHand, 
        GrippablePoint2 gripping, Vector3 direction)
    {
        var index = gripping.GetEdgeIndexFromDirection(direction);

        switch (index)
        {
            case 0:
                forwardHand = leftHand;
                backHand = rightHand;
                break;

            case 1:
                forwardHand = rightHand;
                backHand = leftHand;
                break;

            default:
                break;
        }

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

    static public float CalcHandToHandMagnitude(GameObject hand0, GameObject hand1)
    {
        return (hand0.transform.position - hand1.transform.position).magnitude;
    }

    static public float CalcJumpPower(float currentTime, float maxTime, float maxJumpPowerFactor, float baseJumpPower)
    {
        return baseJumpPower * (maxJumpPowerFactor * currentTime / maxTime);
    }

    static public Vector3 CalcJumpDir(Vector3 baseJumpDir, Vector3 inputMovement, float shotControlFactor, Vector3 wallDir)
    {
        var shotDir = inputMovement;
        shotDir.z = 0.0f;
        ClimberMethod.Swap(ref shotDir.y, ref shotDir.z);

        // 現在掴んでいる壁の方向に飛べない
        if (Vector3.Dot(wallDir, shotDir) <= 0)
        {
            shotDir = Vector3.zero;
        }

        var result = ClimberMethod.CalcMixVector(shotDir, baseJumpDir, shotControlFactor);
        result.Normalize();

        return result;
    }

    static public void Jump(Vector3 jumpDir, Rigidbody rigid, ref GrippablePoint2 grippablePoint, float jumpPower)
    {
        rigid.AddForce(jumpDir * jumpPower, ForceMode.Impulse);
        ClimberMethod.ReleaseGrippablePoint(ref grippablePoint);

    }
}
