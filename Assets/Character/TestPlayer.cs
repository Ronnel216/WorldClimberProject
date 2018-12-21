using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour {

    [SerializeField]
    float powerOfJumping = 0.0f;

    [SerializeField]
    float powerOfClimbing = 0f;

    [SerializeField]
    float powerOfGriping = 0f;

    [SerializeField]
    GameObject core = null;

    [SerializeField]
    GameObject particle = null;

    // 条件が被らないようにすること (例　壁に触れている　と　壁に捕まっている)
    enum StateFlag
    {        
        IsStandingGround    = 1 << 0,       // 地面に立っている
        IsTouchingWall      = 1 << 1,       // 壁に触れている
    }
    StateFlag state;

    Vector3 modelRotationOffset = new Vector3(0, 0, -90);

    Vector3 wallNoraml;
    
    Rigidbody rigid;

    /* ゲームコア
     * カメラワークと移動特性を地形によって変化させるでバリエーションを持たせる
     * 移動を面白く
     * バリエーションが違う場合でもスムーズに操作が繋がるようにする     
         */

    public enum ClimbMode
    {
        KickWall,
        HighSpeedClimb
    }

    public ClimbMode mode = ClimbMode.HighSpeedClimb;

	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody>();
        state = 0;
    }

    private void FixedUpdate()
    {
        //if (!canKickWall) return;        
        //rigid.AddForce(rigid., ForceMode.Acceleration);
        core.GetComponent<Rigidbody>().position = rigid.position;
        core.GetComponent<Rigidbody>().rotation = rigid.rotation * Quaternion.Euler(modelRotationOffset);
        core.GetComponent<Rigidbody>().angularVelocity = rigid.angularVelocity;
        core.GetComponent<Rigidbody>().velocity = rigid.velocity;
    }
    // Update is called once per frame
    void Update () {

        /*
        壁触れ時は常時　壁掴み状態
        特定のボタンで壁から手を放す 
         */

        // 重力を一時的に有効にする
        rigid.useGravity = true;

        // 操作感を合わせるためモード分けしないようにした

        if ((state & StateFlag.IsTouchingWall) == 0) return;

        // 壁に捕まる
        rigid.useGravity = false;
        rigid.AddForce(-wallNoraml * powerOfGriping, ForceMode.Force);

        // 壁の方向を向く

        // 壁に捕まった状態での更新
        UpdateGripingMovement();
        UpdateJumpMovement();

    }

    void UpdateGripingMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rigid.AddForce(Vector3.up * powerOfClimbing, ForceMode.Force);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            rigid.useGravity = true;
        }

    }

    void UpdateJumpMovement()
    {
        Vector3 jumpingVel = Vector3.up;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            jumpingVel += Vector3.right;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            jumpingVel += Vector3.left;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (state & StateFlag.IsTouchingWall) != 0)
        {
            jumpingVel.Normalize();
            // 力積　(瞬間で力を加える)
            rigid.AddForce(jumpingVel * powerOfJumping, ForceMode.Impulse);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        // 指定ビットを立てる
        state |= StateFlag.IsTouchingWall;
        particle.GetComponent<ParticleSystem>().Play();
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 totalVec = Vector3.zero;

        // 法線の平均を求める
        foreach(var contact in collision.contacts)
            totalVec += contact.normal;
        wallNoraml = totalVec /= collision.contacts.Length;  // OnCollisionStayを通る条件として接触していることは確実なので 0除算はあり得ない

        particle.transform.position = collision.contacts[0].point;
        particle.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);

        // 掴んでいる時は落ちない
        //rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.1f, rigid.velocity.z);
    }

    private void OnCollisionExit(Collision collision)
    {
        // 指定ビットをおろす
        state &= ~StateFlag.IsTouchingWall;
        particle.GetComponent<ParticleSystem>().Stop();

    }
}
