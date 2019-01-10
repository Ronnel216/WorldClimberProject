using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CallBackFuncType = System.Action<HitMessageSender, UnityEngine.Collision>;

public class ClimberController : MonoBehaviour {

    [SerializeField]
    float powerOfJumping = 0.0f;

    [SerializeField]
    float powerOfClimbing = 0.0f;

    [SerializeField]
    float powerOfGriping = 0.0f;

    [SerializeField]
    GameObject particle = null;

    [System.Serializable]
    public struct BodyParts
    {
        public GameObject head;
        public GameObject body;
        public GameObject rightArm;
        public GameObject leftArm;
        public GameObject rightLeg;
        public GameObject leftLeg;
        public const int Num = 6;
    }

    [SerializeField]
    BodyParts actor = new BodyParts();

    // 条件が被らないようにすること (例　壁に触れている　と　壁に捕まっている)
    enum StateFlag
    {
        IsStandingGround = 1 << 0,       // 地面に立っている
        IsTouchingWall = 1 << 1,       // 壁に触れている
    }
    StateFlag state;

    // 接している面平均的な法線
    Vector3 wallNoraml;

    Rigidbody rigid;

    /* ゲームコア
     * 移動特性を地形によって変化させるでバリエーションを持たせる
     * 移動を面白く
    */

    //public enum ClimbMode
    //{
    //    KickWall,
    //    HighSpeedClimb
    //}

    //public ClimbMode mode = ClimbMode.HighSpeedClimb;


    public void TouchWallEnter(HitMessageSender sender, Collision collision)
    {

        if (collision.collider.CompareTag("Wall") == false) return;
        if (sender.tag.CompareTo("LeftArm") != 0 && sender.tag.CompareTo("RIghtArm") != 0) return;


        Debug.Log(sender.info[0] + " : TouchWallEnter");

        Vector3 gripVec = Vector3.zero;

        int i = 0;
        for (; i < collision.contacts.Length; i++)
        {
            gripVec += collision.contacts[i].normal;
        }

        gripVec /= i;
        gripVec.Normalize();

        sender.rigid.AddForce(-gripVec, ForceMode.VelocityChange);

        //wallNoraml += collision.contacts[i].normal;
        //wallNoraml /= i;
        //wallNoraml.Normalize();

        state |= StateFlag.IsTouchingWall;

        //return; //仮
        //// 指定ビットを立てる
        //state |= StateFlag.IsTouchingWall;
        //particle.GetComponent<ParticleSystem>().Play();

    }

    public void TouchWallStay(HitMessageSender sender, Collision collision)
    {

        //return; //仮
        //Debug.Log(sender.info[0] + " : TouchWallStay");

        //particle.transform.position = collision.contacts[0].point;
        //particle.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
        //wallNoraml = collision.contacts[0].normal;
        //// 掴んでいる時は落ちない
        ////rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y * 0.1f, rigid.velocity.z);

    }

    public void TouchWallExit(HitMessageSender sender, Collision collision)
    {

        //return; //仮
        //Debug.Log(sender.info[0] + " : TouchWallExit");

        //// 指定ビットをおろす
        //state &= ~StateFlag.IsTouchingWall;
        //particle.GetComponent<ParticleSystem>().Stop();

    }

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        state = 0;

        // ループで回す用
        GameObject[] parts =
        {
            actor.head, actor.body,
            actor.leftArm, actor.rightArm,
            actor.leftLeg, actor.rightArm
        };

        string[] tags =
        {
            "Head", "Body",
            "LeftArm", "RightArm",
            "LeftLeg", "RightLeg"
        };

        /* HitMessageSenderへ登録する関数 */
        CallBackFuncType[][] callBackFuncs =
        {
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit },
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit },
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit },
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit },
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit },
            new CallBackFuncType[]{ TouchWallEnter, TouchWallStay, TouchWallExit }
        };

        // 各体のパーツの初期化
        for (int i = 0; i < BodyParts.Num; i++)
        {
            //var comp = HitMessageSender.AddHitMessageSender(parts[i], callBackFuncs[i], new string[]{tags[i]});
        }
    }

    private void FixedUpdate()
    {
        //if (!canKickWall) return;        
        //rigid.AddForce(rigid., ForceMode.Acceleration);
    }
    // Update is called once per frame
    void Update()
    {
        //return; // 仮
        ///*
        //壁触れ時は常時　壁掴み状態
        //特定のボタンで壁から手を放す (たぶんなしにする　Aimジャンプできるので)
        // */

        //// 重力を一時的に有効にする
        //rigid.useGravity = true;

        //// 操作感を合わせるためモード分けしないようにした

        //if ((state & StateFlag.IsTouchingWall) == 0) return;

        //// 壁に捕まる
        //rigid.useGravity = false;
        //rigid.AddForce(-wallNoraml * powerOfGriping, ForceMode.Force);

        //// 壁に捕まった状態での更新
        //UpdateGripingMovement();
        //UpdateJumpMovement();

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

    private void OnTriggerStay(Collider other)
    {
        Vector3 targetWall = other.ClosestPoint(actor.body.transform.position);
    }

}
