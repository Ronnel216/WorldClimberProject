using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMover : MonoBehaviour {

    // 天球 (球体)　の中心
    [SerializeField]
    Vector3 center;

    // 天球の中心からこのオブジェクトまでの距離
    [SerializeField]
    float distance;

    // 一メートルあたりのラジアン(回転角)
    float radianPerMeter;

    public void SetCenter(Vector3 center)
    {
        this.center = center;
    }

    /// <summary>
    /// 球面上を移動する
    /// </summary>
    /// <param name="moveVec">ローカル座標系での移動成分</param>
    public void Move(Vector2 moveVec)
    {
        // 参照用
        Transform transform = gameObject.transform;
        
        // 軸の算出
        Vector3 axis = center - transform.position;
        axis.Normalize();

        //// 姿勢を修正　（仮）
        //// DirectXTKではこの方法では出来ない
        //gameObject.transform.up = axis;

        // ローカル座標系で移動に使用する回転軸を算出
        Vector3 moveVecWorld = new Vector3(moveVec.x , 0f, moveVec.y);
        float spd = moveVecWorld.magnitude;
        Vector3 rotAxis = Vector3.Cross(Vector3.up, moveVecWorld.normalized);
        
        // 回転用のクォータニオン
        Quaternion rotQuat = Quaternion.AngleAxis(spd * radianPerMeter, rotAxis);

        // 現地点を基準に回転移動
        Vector3 targetPosition = rotQuat * transform.position; 

        // 値の反映
        gameObject.transform.position = targetPosition;
    }

	// Use this for initialization
	void Start () {
        // 直径
        float diameter = distance * 2;
        // 円周 (円の周囲の長さ)
        float circumference = diameter * Mathf.PI;

        // 一メートル当たりのラジアン
        radianPerMeter = Mathf.PI / circumference;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
