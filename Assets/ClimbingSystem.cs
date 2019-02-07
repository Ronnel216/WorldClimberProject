using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    void OnCollisionEnter()
    {

    }

    [System.Serializable]
    class LevelDesign
    {
        // 手の長さ
        public float armLength = 1f;

        // 手の移動力
        public float handMovement = 0.5f;

        // 手の移動速度係数
        [Range(0f, 1f)]
        public float handMovementSpdFactor = 0.04f;

        // 移動入力の判定領域
        [Range(0f, 1f)]
        public float ableInputMovementLimitCos = 0.2f;

        // 掴み位置移動判定領域
        [Range(0f, 1f)]
        public float ableInputGrippingLimitCos = 0.8f;

        // ベースジャンプ力
        public float baseJumpPower = 1000f;

        [Range(0.001f, 1f - 0.001f)]
        public float shotControlFactor = 0.3f;

        public float airControlVelocity = 0.1f;

        // 最大ジャンプ力の増加量
        [Range(1f, Mathf.Infinity)]
        public float maxJumpPowerFactor = 1.5f;

        // ジャンプ力　最大までのため時間
        public float needTimeByFullJumpPower = 1f;

        // ジャンプ方向
        public Vector3 jumpingDirction = Vector3.forward;

        [Range(0f, 1f)]
        public float groundRadian = 0.5f;

        public Vector3 gripPosOffset = Vector3.back * 0.1f;

        public float wallPower = 0.8f;


        public void Init()
        {
            jumpingDirction.Normalize();

            maxHandToHand = armLength * 2 * 0.7f;         // X
            minHandToHand = maxHandToHand - handMovement; // Y
        }

        public float maxHandToHand { get; set; }
        public float minHandToHand { get; set; }
    }
    [SerializeField]
    LevelDesign level;


    [SerializeField]
    GameObject leftHand;

    [SerializeField]
    GameObject rightHand;

    [SerializeField]
    GameObject gripAncharBase = null;
    [SerializeField]
    GameObject gripAnchar = null;

    [SerializeField]
    SubCollider nearGrippable = null;

    [SerializeField]
    SubCollider farGrippable = null;

    GrippablePoint2 grippablePoint;

    List<Vector3> wallNormals = new List<Vector3>();

    Rigidbody rigid;

    // 掴んでいる地形
    Collider grippingCollider;

    // デバッグ表示用 Gizmos
    List<System.Action> callInOnDrawGizmos;

    [SerializeField]
    AudioClip[] audioClip = new AudioClip[2];

    bool canCatch = true;

    // イベント false:破棄
    public interface Event
    {
        bool Action(ClimbingSystem system);
    }

    public List<Event> events = new List<Event>();

    // クライマーのステートマシーン
    InterfaceClimberState currentState;
    InterfaceClimberState nextState;

    bool isChange = false;  // あとで変数名　実装方法を変える
    // グリップ情報をまとめるクラス作ってもいいかも

    enum HandMovementMode
    {
        None,
        Stay,
        Advance,
        Close,
        Catch
    }
    HandMovementMode handMovementMode;

    float unenableColliderResetTime = -1f;

    AudioSource audio = null;

    // Use this for initialization
    void Start () {
        level.Init();
        rigid = GetComponent<Rigidbody>();
        callInOnDrawGizmos = new List<System.Action>();

        audio = GetComponent<AudioSource>();

        //var funcs = new System.Action < HitMessageSender, UnityEngine.Collider >[3];
        //funcs[0] = (HitMessageSender a, UnityEngine.Collider b) => { };
        //funcs[1] = (HitMessageSender sender, UnityEngine.Collider collider) => 
        //{

        //};
        //funcs[2] = (HitMessageSender a, UnityEngine.Collider b) => { };
        //HitMessageSender.AddHitMessageSender(grippingAbleArea, funcs, new string[] { });



        handMovementMode = HandMovementMode.Close;

        // 進む方向の手を定義する
        ClimberMethod.SetHandForwardAndBack(ref rightHand, ref leftHand, Vector3.right);

        // 仮
        currentState = new AirState();
        nextState = null;

        currentState.Init(this);
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate(this);
    }

    // Update is called once per frame
    void Update ()
    {
        if (unenableColliderResetTime > 0)
        {
            unenableColliderResetTime -= Time.deltaTime;
            if (unenableColliderResetTime <= 0)
                GetComponent<BoxCollider>().enabled = true;
        }

        var removeList = new List<int>();
        int i = 0;
        foreach (var e in events)
        {
            var re = e.Action(this);        // イベントの実行
            if (re == false) removeList.Add(i);
            i++;
        }

        // index関係で逆順に削除
        removeList.Reverse();
        foreach (var index in removeList)
            events.RemoveAt(index);

        currentState.Update(this);

        //? 呼び出すタイミングの考察が必要
        if (nextState != null)
        {
            currentState = nextState;
            nextState = null;

            currentState.Init(this);

        }

        // 衝突中の壁の法線方向をリセット
        wallNormals.Clear();

    }

    void ChangeState(InterfaceClimberState newState)
    {
        nextState = newState;
    }

    void UnenableCollider(float time)
    {
        Debug.Assert(unenableColliderResetTime < 0, "連続して使用できない");
        unenableColliderResetTime = time;
        GetComponent<BoxCollider>().enabled = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        var contacts = collision.contacts;
        
        var normal = Vector3.zero;

        if (collision.gameObject.tag != "Wall") return;
        foreach (var contact in contacts)
            normal += contact.normal;
        normal /= contacts.Length;

        wallNormals.Add(normal);
    }

    void OnDrawGizmos()
    {
        if (rigid == false) return;

        foreach (var func in callInOnDrawGizmos) func();
        callInOnDrawGizmos.Clear();
    }



    // ステートの汎用メソッド
    class StateUtility
    {
        // Use this for initialization
        virtual protected void Start(ClimbingSystem system)
        {

        }

        virtual protected void FixedUpdate(ClimbingSystem system)
        {

        }

        // Update is called once per frame
        virtual protected void Update(ClimbingSystem system)
        {

        }

    }

    // システム側から呼び出す
    protected interface InterfaceClimberState
    {
        void Init(ClimbingSystem system);

        void FixedUpdate(ClimbingSystem system);

        void Update(ClimbingSystem system);

        
    }    

    /// <summary>
    /// 空中で何も手を触れていない状態
    /// </summary>
    class AirState : InterfaceClimberState
    {
        float jumpHoge = 0.0f;  // 仮の滞空中制御

        public void Init(ClimbingSystem system)
        {
            system.canCatch = false;
        }

        public void FixedUpdate(ClimbingSystem system)
        {
            if (jumpHoge >= 0f && jumpHoge <= 1.0f)
            {
                if (Input.GetKey("joystick button 0"))
                {
                    jumpHoge += Time.deltaTime;
                    system.rigid.AddForce(Vector3.up * 5, ForceMode.Impulse);
                }
                else jumpHoge = -1.0f;

            }

            // 空中移動
            var inputMovement = ClimberMethod.GetInputMovement3D();
            inputMovement.z = 0f;
            ClimberMethod.Swap(ref inputMovement.y, ref inputMovement.z);
            if (inputMovement.sqrMagnitude > Mathf.Epsilon)
            {
                system.rigid.AddForce(inputMovement * system.level.airControlVelocity, ForceMode.VelocityChange);
            }

            //{
            //    //! デバッグ用　舞空術
            //    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space))
            //    {
            //        system.rigid.AddForce(Vector3.up * 0.8f, ForceMode.VelocityChange);
            //    }
            //}

            var nearGripColi = ClimberMethod.CheckGripPoint(
                system.rigid.velocity.normalized,
                system.nearGrippable,
                system.level.ableInputGrippingLimitCos);

            if (nearGripColi != null)
            {
                var temp = nearGripColi.gameObject.GetComponent<GrippablePoint2>();
            }

            if (ClimberMethod.GetInputCatch() == false)
            {
                system.canCatch = true;
                return;
            }
            if (system.canCatch == false) return;

            if (nearGripColi != null)
            {
                system.audio.PlayOneShot(system.audioClip[1]);
                //system.grippablePoint = system.nearGrippable.GetComponent<GrippablePoint2>();
                system.grippingCollider = ClimberMethod.ChangeGrippablePoint(ref system.grippablePoint, nearGripColi);
                var ancharRigid = system.gripAncharBase.GetComponent<Rigidbody>();
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, nearGripColi.GetComponent<GrippablePoint2>().GetEdgeDirection());
                ClimberMethod.SetGrippingAnchar(ancharRigid, nearGripColi.ClosestPoint(system.rigid.position), rotation);
                system.ChangeState(new GrippingWallState());
            }else
            {
                system.gripAncharBase.transform.GetComponent<Rigidbody>().position = Vector3.Lerp(system.gripAncharBase.transform.GetComponent<Rigidbody>().position, system.rigid.position, 0.5f);
            }

        }

        public void Update(ClimbingSystem system)
        {
            //var inputMovement = ClimberMethod.GetInputMovement3D();
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    float jumpPower = ClimberMethod.CalcJumpPower(1f, 1f, system.level.maxJumpPowerFactor, system.level.baseJumpPower);
            //    var jumpDir = ClimberMethod.CalcJumpDir(system.level.jumpingDirction, inputMovement, system.level.shotControlFactor, system.grippablePoint.GetWallDirection() * Vector3.forward);
            //    ClimberMethod.Jump(jumpDir, system.rigid, ref system.grippablePoint, jumpPower);
            //    system.ChangeState(new AirState());
            //}

            foreach (var normal in system.wallNormals)
            {
                system.rigid.AddForce(normal * system.level.wallPower, ForceMode.VelocityChange);
            }
        }

    }

    /// <summary>
    /// 壁端つかまり状態
    /// </summary>
    class GrippingWallState : InterfaceClimberState
    {
        GameObject forwardHand; // 進行方向の手
        GameObject backHand;    // 進行方向と逆の手

        CharacterJoint grippingJoint = null;

        float chargeJumppingTime = 0.0f;

        // Use this for initialization
        public void Init(ClimbingSystem system)
        {
            // 掴んでいる地形
            Debug.Assert(system.grippablePoint != null, "掴み位置を保持していない");

            // 体を制御 -----
            grippingJoint = system.gripAnchar.GetComponent<CharacterJoint>();
            forwardHand = system.rightHand; backHand = system.leftHand;

            //Vector3[] handsPos = ClimberMethod.GetHandsPosition(forwardHand, backHand);
            system.grippablePoint.SetHandPosition(forwardHand, system.grippingCollider);　            // 仮
            system.grippablePoint.SetHandPosition(backHand, system.grippingCollider);　            // 仮

        }

        public void FixedUpdate(ClimbingSystem system)
        {
            // 体を制御 -----
            var handsPos = ClimberMethod.GetHandsPosition(system.rightHand, system.leftHand);

            // 崖つかまり時の　アンカー設定
            var ancharRigid = system.gripAncharBase.GetComponent<Rigidbody>();
            Quaternion rotation = ClimberMethod.CalcRotationXZ(handsPos[0], handsPos[1]);
            //ClimberMethod.SetGrippingAnchar(ancharRigid, (handsPos[0] + handsPos[1]) / 2, rotation);
            ClimberMethod.SetGrippingAnchar(ancharRigid
                , ancharRigid.transform.position
                , Quaternion.Lerp(ancharRigid.transform.rotation, rotation, 1f));

            // アンカーの状態を反映する
            ClimberMethod.ApplyGrippingAnchar(system.rigid, system.gripAnchar.GetComponent<Rigidbody>(), system.level.gripPosOffset);

        }

        // Update is called once per frame
        public void Update(ClimbingSystem system)
        {

            if (ClimberMethod.GetInputCatch() == false)
            {
                system.ChangeState(new AirState());
                return;
            }

            /*
             * X = 手の長さ * 2 * 調整値, Y = X - 移動距離
             * それぞれの状態に遷移する距離として近づける距離をXとして進める距離をYとして　X > 移動距離 > Y
             * 進む方向の手を進める   ・・・ 入力直後はこれからスタート または　距離がY以上で遷移
             * 反対の手を近づける    ・・・ 距離がX以上で遷移 または　入力が無いときに遷移
             */
            //canAdvanceStep = magnitudeHandToHand < handMovement;
            var inputMovement = ClimberMethod.GetInputMovement3D();
            var inputMovementMagni = inputMovement.magnitude;
            DebugVisualizeVector3.visualize = inputMovement;

            // 掴んだ状態で移動できる範囲
            system.callInOnDrawGizmos.Add(() =>
            {
                //Gizmos.DrawWireSphere(system.nearGrippable.transform.position);
            });

            if (ClimberMethod.GetInputJumpTrigger())
            {
                float jumpPower = ClimberMethod.CalcJumpPower(1f, 1f, system.level.maxJumpPowerFactor, system.level.baseJumpPower);
                var jumpDir = ClimberMethod.CalcJumpDir(system.level.jumpingDirction, inputMovement, system.level.shotControlFactor, system.grippablePoint.GetWallDirection() * Vector3.forward);
                jumpDir = Vector3.up;
                //var farGripColi = ClimberMethod.CheckGripPoint(inputMovement, system.farGrippable, system.level.ableInputGrippingLimitCos);
                //if (farGripColi != null)
                //{
                //    var closePos = farGripColi.ClosestPoint(system.gripAncharBase.transform.position);
                //    var dir = closePos - system.rigid.transform.position;


                //    dir.y = 1f;
                //    dir.Normalize();

                //    jumpDir = dir;
                //}
                //}

                //jumpDir = 
                ClimberMethod.Jump(jumpDir, system.rigid, ref system.grippablePoint, jumpPower);
                //system.UnenableCollider(0.2f);
                system.audio.PlayOneShot(system.audioClip[0]);
                system.ChangeState(new AirState());

                return;
            }

            Vector3 moveVec = Vector3.zero;
            //GameObject forwardHand = null;

            // 移動方向を求める
            if (inputMovementMagni > Mathf.Epsilon)
            {
                var edge = system.grippablePoint.GetEdgeFromDirection(inputMovement);
                moveVec = system.grippablePoint.CalcMoveDirction(inputMovement);
            }
            bool canMoving = Vector3.Dot(moveVec, inputMovement) > system.level.ableInputMovementLimitCos;
            if (canMoving == false) return;


            var movement = ClimberMethod.CalcLerpTranslation(
                moveVec.normalized,
                1f,
                system.level.handMovementSpdFactor);
                movement = system.grippablePoint.ClampHandsMovement(system.gripAncharBase.transform.position, movement);
            //if (movement.sqrMagnitude <= 0)
            //{
            var nearGripColi = ClimberMethod.CheckGripPoint(inputMovement, system.nearGrippable, system.level.ableInputGrippingLimitCos);

            if (nearGripColi != null)
            {
                var closePos = nearGripColi.ClosestPoint(system.gripAncharBase.transform.position);
                if ((closePos - system.gripAncharBase.transform.position).sqrMagnitude <= 0.04f)
                {
                    system.grippingCollider = ClimberMethod.ChangeGrippablePoint(ref system.grippablePoint, nearGripColi);
                    //system.handMovementMode = HandMovementMode.Catch;
                    //system.isChange = true;

                }
            }
            //}
            system.gripAncharBase.transform.Translate(movement);

            //switch (system.handMovementMode)
            //{
            //    //case HandMovementMode.Catch:
            //    //    float step = system.grippablePoint.SetHandPosition(forwardHand, system.grippingCollider, 0.5f);
            //    //    bool isFinished = 0.000001f > step;
            //    //    if (isFinished)
            //    //    {
            //    //        system.grippablePoint.SetHandPosition(forwardHand, system.grippingCollider);
            //    //        //system.handMovementMode = HandMovementMode.Advance;
            //    //        system.isChange = false;
            //    //    }
            //    //    break;

            //    //case HandMovementMode.Advance:
            //    //    AdvaneHand(inputMovement, system);
            //    //    break;

            //    //case HandMovementMode.Close:
            //    //    CloseHand(inputMovement, system);
            //    //    break;

            //    //default:
            //    //    // 通らないはず
            //    //    Debug.Log("HandMovementMode == None");
            //    //    break;
            //}

        }

        private void AdvaneHand(Vector3 inputMovement, ClimbingSystem system)
        {
            float magnitudeHandToHand = ClimberMethod.CalcHandToHandMagnitude(system.rightHand, system.leftHand);

            var inputMovementMagni = inputMovement.magnitude;
            var moveVec = Vector3.zero;

            var level = system.level;

            if (system.isChange)
            {
                float step = system.grippablePoint.SetHandPosition(forwardHand, system.grippingCollider, 0.5f);
                bool isFinished = 0.000001f > step;
                if (isFinished)
                {
                    system.grippablePoint.SetHandPosition(forwardHand, system.grippingCollider);
                    system.handMovementMode = HandMovementMode.Close;
                    //system.handMovementMode = HandMovementMode.Advance;
                }
            }

            // 移動方向を求める
            if (inputMovementMagni > Mathf.Epsilon)
            {
                var edge = system.grippablePoint.GetEdgeFromDirection(inputMovement);
                ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, edge);
                moveVec = system.grippablePoint.CalcMoveDirction(inputMovement);
            }
            else
            {
                // 入力なしで後方の手を近づける
                system.handMovementMode = HandMovementMode.Close;
                return;
            }

            bool canMoving = Vector3.Dot(moveVec, inputMovement) > level.ableInputMovementLimitCos;
            if (canMoving == false) return;

            // 手同士の距離が最大を超えた時　後方の手を近づける状態に遷移
            if (level.maxHandToHand < magnitudeHandToHand)
            {
                system.handMovementMode = HandMovementMode.Close;
                return;
            }

            // 前方の手を進める
            if (level.maxHandToHand > magnitudeHandToHand)
            {
                var movement = ClimberMethod.CalcLerpTranslation(
                    moveVec.normalized,
                    Mathf.Max(magnitudeHandToHand, 1f),
                    system.level.handMovementSpdFactor);
                movement = system.grippablePoint.ClampHandsMovement(forwardHand.transform.position, movement);
                if (movement.sqrMagnitude <= 0)
                {
                    var nearGripColi = ClimberMethod.CheckGripPoint(inputMovement, system.nearGrippable, system.level.ableInputGrippingLimitCos);

                    if (nearGripColi != null)
                    {
                        system.grippingCollider = ClimberMethod.ChangeGrippablePoint(ref system.grippablePoint, nearGripColi);
                        //system.handMovementMode = HandMovementMode.Catch;
                        system.isChange = true;

                    }
                }
                forwardHand.transform.Translate(movement, Space.World);
            }
            return;
        }

        private void CloseHand(Vector3 inputMovement, ClimbingSystem system)
        {
            float magnitudeHandToHand = ClimberMethod.CalcHandToHandMagnitude(system.rightHand, system.leftHand);

            var inputMovementMagni = inputMovement.magnitude;
            var moveVec = Vector3.zero;

            var level = system.level;

            if (system.isChange)
            {
                float step = system.grippablePoint.SetHandPosition(backHand, system.grippingCollider, 0.01f);
                bool isFinished = 0.000001f > step;
                if (isFinished)
                {
                    system.grippablePoint.SetHandPosition(backHand, system.grippingCollider, 1f);
                    system.isChange = false;
                };
            }

            // 移動入力値が存在する
            bool isInputMovement = inputMovementMagni > Mathf.Epsilon;
            if (isInputMovement)
            {
                // 後ろの手を前の手に限界まで近づけたら遷移
                if (level.minHandToHand > magnitudeHandToHand)
                {
                    system.handMovementMode = HandMovementMode.Advance;
                    return;
                }
            }

            // 後方の手を近づける
            if (level.minHandToHand < magnitudeHandToHand)
            {
                moveVec = forwardHand.transform.position - backHand.transform.position;

                float limit = -level.minHandToHand / 2;
                var result = ClimberMethod.CalcLerpTranslation(
                    moveVec.normalized,
                    magnitudeHandToHand - limit,
                    system.level.handMovementSpdFactor);
                backHand.transform.Translate(result, Space.World);
            }
            return;
        }

    }

}
