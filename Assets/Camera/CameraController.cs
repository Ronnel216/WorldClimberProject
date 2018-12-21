using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField]
    GameObject target = null; // クラス名を取得したい

    [SerializeField]
    float distanceToTarget = 5.0f;

    // カメラ制御速度
    [SerializeField]
    float maxCameraSpd = 0.5f;

    //! カメラの位置が直上や真下になるのを防止するため

    // カメラのY軸方向の移動可能範囲 半径
    // カメラ移動できる軌道を単位円した時の値
    [SerializeField]
    [Range(0, 0.99999f)]
    float controllableAreaHalfY = 0.8f;

    // 制限に余裕を与える
    // 制限以上のカメラの制御にバネの様な作用を掛ける
    [SerializeField]
    float limitControlSpringPower = 1.0f;

    // カメラの向き
    public static Quaternion direction;

    //SphereMover spheremover;

    // Use this for initialization
    void Start () {
        //spheremover = gameObject.GetComponent<SphereMover>();
	}
	
	// Update is called once per frame
	void Update () {
        direction = gameObject.transform.rotation;

        float x = .0f;
        float y = .0f;
        if (Input.GetKey(KeyCode.D))
            x = 1.0f;
        else if (Input.GetKey(KeyCode.A))
            x = -1.0f;
        if (Input.GetKey(KeyCode.W))
            y = 1.0f;
        else if (Input.GetKey(KeyCode.S))
            y = -1.0f;
       
        var selfPosOffset = (transform.position - target.transform.position).normalized;
        selfPosOffset *= distanceToTarget;
        selfPosOffset = CalcCameraOffset(selfPosOffset, x, y, maxCameraSpd);

        // カメラが真上と真下に移動しないように
        if (Mathf.Abs(selfPosOffset.y) >= controllableAreaHalfY * distanceToTarget)
        {
            float factor = (Mathf.Abs(selfPosOffset.y) / distanceToTarget - controllableAreaHalfY) / (1.0f - controllableAreaHalfY);
            factor *= limitControlSpringPower;
            factor = Mathf.Min(factor, 1.0f);
            factor *= factor;
            float springSpd = maxCameraSpd * factor;
            selfPosOffset = CalcCameraOffset(selfPosOffset, 0, selfPosOffset.y > 0 ? -1 : 1, springSpd);
        }

        var selfPos = target.transform.position + selfPosOffset;

        // 位置の設定
        transform.position = selfPos;

        transform.LookAt(target.transform.position);

    }

    // カメラの位置を求める
    Vector3 CalcCameraOffset(Vector3 offsetVec, float x, float y, float speed)
    {        
        // 回転
        var rotAxisVertical = Vector3.Cross(offsetVec, Vector3.up);
        var rotQut = Quaternion.AngleAxis(x, Vector3.down) * Quaternion.AngleAxis(y, rotAxisVertical);

        // 補間
        rotQut = Quaternion.Lerp(Quaternion.identity, rotQut, speed);

        // 座標計算
        return rotQut * offsetVec;

    }
}
