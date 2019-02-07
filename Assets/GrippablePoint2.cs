using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class GrippablePoint2 : MonoBehaviour
{

    [SerializeField]
    Vector3[] edges = new Vector3[2];

    LineRenderer line;

    static GameObject prefab = null;

    Color baseColor = new Color(0.2f, 0.2f, 1f, 1f);
    Color grippableColor = Color.green;

    static public GrippablePoint2 CreateEdges(Vector3 start, Vector3 end)
    {
        if (prefab == null) prefab = (GameObject)Resources.Load("GripPoint");
        var obj = Instantiate(prefab);

        var grip = obj.GetComponent<GrippablePoint2>();
        grip.edges[0] = start;
        grip.edges[1] = end;

        var line = obj.GetComponent<LineRenderer>();
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        float radius = 0.1f;
        obj.transform.localScale = new Vector3(radius, radius, radius);

        obj.transform.position = (grip.edges[0] + grip.edges[1]) / 2;
        obj.transform.localScale = new Vector3((grip.edges[0] - grip.edges[1]).magnitude, radius, radius); // 仮
        obj.transform.rotation = Quaternion.FromToRotation(Vector3.right, grip.edges[0] - grip.edges[1]);  // 仮

        return grip;
    }

    void Awake()
    {

    }

    
    public void VisualizeGrippable(bool isVisualize)
    {
        GetComponent<Light>().enabled = isVisualize;
    }

    public Quaternion GetWallDirection()
    {
        return Quaternion.FromToRotation(Vector3.left, (edges[1] - edges[0]));
    }

    public Vector3 GetEdgeDirection()
    {
        return edges[1] - edges[0];
    }

    //public Vector3 GetEdgeDirection()
    //{
    //    return Vector3.Cross(Vector3.up, (edges[1] - edges[0]));
    //}

    public int GetEdgeIndexFromDirection(Vector3 direction)
    {
        Vector3 grippingVec = Vector3.zero;

        grippingVec = edges[0] - gameObject.transform.position;
        if (Vector3.Dot(direction, grippingVec) > 0)
            return 0;

        grippingVec = edges[1] - gameObject.transform.position;
        if (Vector3.Dot(direction, grippingVec) > 0)
            return 1;

        return -1;
    }

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
        var indexes = GetEdgesIndexFromPos(handPos);
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

    //public float SetHandsPosition(GameObject forward, GameObject back, Collider selfColi, float lerp = 1.0f)
    //{
    //    GameObject[] hands = new GameObject[2] { forward, back};
    //    var pos = ClimberMethod.GetHandsPosition(hands[0], hands[1]);
    //    Vector3[] target = new Vector3[2];
    //    target[0] = selfColi.ClosestPoint(pos[0]);
    //    target[1] = selfColi.ClosestPoint(pos[1]);
    //    Vector3[] result = new Vector3[2];

    //    for (var i = 0; i < 2; i++)
    //    {
    //        result[i] = Vector3.Lerp(hands[i].transform.position, target[i], lerp);
    //    }

    //    // 位置の設定
    //    //if (result[0] == result[1]) //? 応急処置
    //    //{
    //    //    var indexes = GetEdgesFromPos(result[0]);
    //    //    var offset = (edges[indexes[1]] - edges[indexes[0]]).normalized * 0.2f;
    //    //    result[0] += offset;
    //    //}

    //    hands[0].transform.position = result[0];
    //    hands[1].transform.position = result[1];

    //    // 角度の調整
    //    foreach (var hand in hands)
    //    {
    //        hand.transform.rotation = ClimberMethod.CalcRotation(edges[0], edges[1]);
    //    }

    //    float totalSqr = 0.0f;
    //    for (var i = 0; i < 2; i++)
    //        totalSqr += (hands[i].transform.position - target[i]).sqrMagnitude;

    //    return totalSqr;
    //}

    public float SetHandPosition(GameObject hand, Collider selfColi, float lerp = 1.0f)
    {
        var pos = hand.transform.position;
        var target = selfColi.ClosestPoint(pos);

        Vector3 result = Vector3.Lerp(pos, target, lerp);

        // 位置の設定
        //if (result[0] == result[1]) //? 応急処置
        //{
        //    var indexes = GetEdgesFromPos(result[0]);
        //    var offset = (edges[indexes[1]] - edges[indexes[0]]).normalized * 0.2f;
        //    result[0] += offset;
        //}

        hand.transform.position = result;

        // 角度の調整
        hand.transform.rotation = ClimberMethod.CalcRotation(edges[0], edges[1]);

        float totalSqr = 0.0f;
        totalSqr += (hand.transform.position - target).sqrMagnitude;

        return totalSqr;

    }

    // 近い端を取得する
    public int[] GetEdgesIndexFromPos(Vector3 pos)
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
        int[] indexes = GetEdgesIndexFromPos(GetEdgesCenter() + direction);
        return GetEdge(indexes[0]);
    }

    Vector3 GetEdgesCenter()
    {
        return (edges[0] + edges[1]) / 2;
    }

}
