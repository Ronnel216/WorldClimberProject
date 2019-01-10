using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrippablePoint : MonoBehaviour {

    [SerializeField]
    Vector3[] edges = new Vector3[2];

    void Awake()
    {
        float radius = 0.1f;
        transform.localScale = new Vector3(radius, radius, radius);

        transform.position = (edges[0] + edges[1]) / 2;
        transform.localScale = new Vector3((edges[0] - edges[1]).magnitude, radius, radius); // 仮
        transform.rotation = Quaternion.FromToRotation(Vector3.right, edges[0] - edges[1]);  // 仮

    }

    public Quaternion GetEdgeDirection()
    {
        return Quaternion.FromToRotation(Vector3.left, (edges[1] - edges[1]));
    }

    //public Vector3 GetEdgeDirection()
    //{
    //    return Vector3.Cross(Vector3.up, (edges[1] - edges[0]));
    //}

    // 移動方向を計算する
    public Vector3 CalcMoveDirction(Vector3 movement)
    {
        Vector3 grippingVec = Vector3.zero;

        grippingVec = edges[0] - gameObject.transform.position;
        if (Vector3.Dot(movement, grippingVec) > 0)
            return grippingVec.normalized;

        grippingVec = edges[1] - gameObject.transform.position;
        if (Vector3.Dot(movement, grippingVec) > 0)
            return grippingVec.normalized;

        return Vector3.zero;
    }

    public Vector3 ClampHandsPosition(Vector3 handPos)
    {
        var indexes = GetEdgeFromPos(handPos);
        var edgeDir = edges[indexes[0]] - edges[indexes[1]];
        var egesToHandVec = edges[indexes[0]] - handPos;

        // グリップポイントの端を超えた位置に手が存在する
        if (Vector3.Dot(edgeDir, egesToHandVec) <= 0)
            return edges[indexes[0]];
        return handPos;

    }

    public Vector3 ClampHandsMovement(Vector3 handPos, Vector3 movement)
    {
        return ClampHandsPosition(handPos + movement) - handPos;
    }

    public float SetHandsPosition(GameObject forward, GameObject back, Collider selfColi, float lerp = 1.0f)
    {
        GameObject[] hands = new GameObject[2] { forward, back};
        var pos = ClimberMethod.GetHandsPosition(hands[0], hands[1]);
        Vector3[] target = new Vector3[2];
        target[0] = selfColi.ClosestPoint(pos[0]);
        target[1] = selfColi.ClosestPoint(pos[1]);
        Vector3[] result = new Vector3[2];

        for (var i = 0; i < 2; i++)
        {
            result[i] = Vector3.Lerp(hands[i].transform.position, target[i], lerp);
        }

        // 位置の設定
        hands[0].transform.position = result[0];
        hands[1].transform.position = result[1];

        // 角度の調整
        foreach (var hand in hands)
        {
            hand.transform.rotation = ClimberMethod.CalcRotation(edges[0], edges[1]);
        }

        float totalSqr = 0.0f;
        for (var i = 0; i < 2; i++)
            totalSqr += (hands[i].transform.position - target[i]).sqrMagnitude;

        return totalSqr;
    }

    // 近い端を取得する
    public int[] GetEdgeFromCenter(Vector3 offset)
    {
        float d0 = ((edges[0] - edges[1]) - offset).sqrMagnitude;
        float d1 = ((edges[1] - edges[0]) - offset).sqrMagnitude;

        if (d0 < d1) return new int[]{ 0, 1};
        return new int[] { 1, 0 };
    }

    public int[] GetEdgeFromPos(Vector3 pos)
    {
        float d0 = (edges[0] - pos).sqrMagnitude;
        float d1 = (edges[1] - pos).sqrMagnitude;

        if (d0 < d1) return new int[] { 0, 1 };
        return new int[] { 1, 0 };
    }

    public Vector3 GetEdge(int index)
    {
        return edges[index];
    }

    public Vector3 GetEdgeFromDirection(Vector3 direction)
    {
        int[] indexes = GetEdgeFromCenter(direction);
        return GetEdge(indexes[0]);
    }

    Vector3 GetEdgesCenter()
    {
        return (edges[0] + edges[1]) / 2;
    }

}
