using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimberController2 : MonoBehaviour {


    [SerializeField]
    float powerOfJumping;

    [SerializeField]
    float powerOfClimbing;

    [SerializeField]
    float powerOfGriping;

    // 条件が被らないようにすること (例　壁に触れている　と　壁に捕まっている)
    enum StateFlag
    {
        IsStandingGround = 1 << 0,       // 地面に立っている
        IsTouchingWall = 1 << 1,       // 壁に触れている
    }
    StateFlag state;

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
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        state = 0;
    }

    private void FixedUpdate()
    {
        //if (!canKickWall) return;        
        //rigid.AddForce(rigid., ForceMode.Acceleration);
    }
    // Update is called once per frame
    void Update()
    {

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
    }

    private void OnCollisionStay(Collision collision)
    {
        wallNoraml = collision.contacts[0].normal;
        // 掴んでいる時は落ちない
        //rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.1f, rigid.velocity.z);
    }

    private void OnCollisionExit(Collision collision)
    {
        // 指定ビットをおろす
        state &= ~StateFlag.IsTouchingWall;

    }
}
