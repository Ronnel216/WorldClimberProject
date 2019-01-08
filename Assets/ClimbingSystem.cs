using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    [SerializeField]
    GameObject gripTop = null;
    [SerializeField]
    GameObject gripBottom;

    [SerializeField]
    GameObject leftHand;

    [SerializeField]
    GameObject rightHand;

    [SerializeField]
    GameObject garpAncharBase = null;
    [SerializeField]
    GameObject gripAnchar = null;

    [SerializeField]
    SubCollider nearGrippable = null;

    [SerializeField]
    float armLength = 1f;

    Rigidbody rigid;

    [SerializeField]
    float handMovement = 0.1f;

    [SerializeField]
    float handMovementSpdFactor = 0.03f;

    [SerializeField]
    float ableInputAreaForMovement = 0.0f;  // 移動入力の判定領域

    // 掴んでいる地形
    Collider grippingCollider;

    // デバッグ表示用 Gizmos
    List<System.Action> callInOnDrawGizmos;

    // クライマーのステートマシーン
    InterfaceClimberState state;

    //bool isGrip;    // 仮のフラグ    ステイトにあとで変えます
    // グリップ情報をまとめるクラス作ってもいいかも

    enum HandMovementMode
    {
        None,
        Stay,
        Advance,
        Close,
    }
    HandMovementMode handMovementMode;

    class HandTrasControlCmd
    {
        GameObject hand = null;
        Vector3 target = Vector3.zero;
        float safeErrorDistance = Mathf.Epsilon;    // 許容誤差

        bool Execute(float step)
        {
            Vector3 moveVec = target - hand.transform.position;
            hand.transform.Translate(moveVec.normalized * step);
            return moveVec.sqrMagnitude <= safeErrorDistance * safeErrorDistance;
        }
    }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();
        callInOnDrawGizmos = new List<System.Action>();

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
        state = new GrippingWallState();

        state.Init(this);
    }

    void FixedUpdate()
    {
        state.FixedUpdate(this);
    }

    // Update is called once per frame
    void Update ()
    {
        state.Update(this);

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
    /// 壁端つかまり状態
    /// </summary>
    class GrippingWallState : InterfaceClimberState
    {
        GameObject forwardHand; // 進行方向の手
        GameObject backHand;    // 進行方向と逆の手

        GrippablePoint grippablePoint;

        CharacterJoint grippingJoint;
        private bool isChangeGripPoint;

        // Use this for initialization
        public void Init(ClimbingSystem system)
        {
            // 掴んでいる地形
            grippablePoint = system.gripTop.GetComponent<GrippablePoint>();

            // 体を制御 -----
            grippingJoint = system.gripAnchar.GetComponent<CharacterJoint>();
            forwardHand = system.rightHand; backHand = system.leftHand;
            Vector3[] handsPos = ClimberMethod.GetHandsPosition(forwardHand, backHand);

            isChangeGripPoint = false;
        }

        public void FixedUpdate(ClimbingSystem system)
        {
            // 体を制御 -----
            var handsPos = ClimberMethod.GetHandsPosition(system.rightHand, system.leftHand);

            // 崖つかまり時の　アンカー設定
            var ancharRigid = system.garpAncharBase.GetComponent<Rigidbody>();            
            Quaternion rotation = ClimberMethod.CalcRotationXZ(handsPos[0], handsPos[1]);
            ClimberMethod.SetGrippingAnchar(ancharRigid, (handsPos[0] + handsPos[1]) / 2, rotation);

            // アンカーの状態を反映する
            ClimberMethod.ApplyGrippingAnchar(system.rigid, system.gripAnchar.GetComponent<Rigidbody>());

        }

        // Update is called once per frame
        public void Update(ClimbingSystem system)
        {      
            if (isChangeGripPoint)
            {
                float step = grippablePoint.SetHandsPosition(forwardHand, backHand, system.grippingCollider, 0.5f);
                bool isFinished = 0.001f > step;
                if (isFinished)
                {
                    isChangeGripPoint = false;
                    grippablePoint.SetHandsPosition(forwardHand, backHand, system.grippingCollider);
                }
                return;
            }

            float magnitudeHandToHand = (system.rightHand.transform.position - system.leftHand.transform.position).magnitude;
            /*
             * X = 手の長さ * 2 * 調整値, Y = X - 移動距離
             * それぞれの状態に遷移する距離として近づける距離をXとして進める距離をYとして　X > 移動距離 > Y
             * 進む方向の手を進める   ・・・ 入力直後はこれからスタート または　距離がY以上で遷移
             * 反対の手を近づける    ・・・ 距離がX以上で遷移 または　入力が無いときに遷移
             */
            //canAdvanceStep = magnitudeHandToHand < handMovement;
            float maxHandToHand = system.armLength * 2 * 0.7f;         // X
            float minHandToHand = maxHandToHand - system.handMovement; // Y

            var inputMovement = ClimberMethod.GetInputMovement3D();

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                var nearGripColi = ClimberMethod.CheckGripPoint(Vector3.up, system.nearGrippable);             

                if (nearGripColi != null)
                {
                    Debug.Log("Ok Grip");
                    grippablePoint.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");

                    system.grippingCollider = nearGripColi;
                    grippablePoint = nearGripColi.gameObject.GetComponent<GrippablePoint>();
                    grippablePoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    isChangeGripPoint = true;
                }
            }

            // 掴んだ状態で移動できる範囲
            system.callInOnDrawGizmos.Add(() =>
            {
                //Gizmos.DrawWireSphere(system.nearGrippable.transform.position);
            });

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                var nearGripColi = ClimberMethod.CheckGripPoint(Vector3.down, system.nearGrippable);

                if (nearGripColi != null)
                {
                    Debug.Log("Ok Grip");
                    grippablePoint.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");

                    system.grippingCollider = nearGripColi;
                    grippablePoint = nearGripColi.gameObject.GetComponent<GrippablePoint>();
                    grippablePoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    isChangeGripPoint = true;
                }

            }

            Vector3 moveVec = Vector3.zero;
            //GameObject forwardHand = null;
            switch (system.handMovementMode)
            {
                case HandMovementMode.Advance:
                    // 移動方向を求める
                    if (inputMovement.x != 0)
                    {
                        Vector3 inputMovementXZ = ClimberMethod.ConvertVec2ToVec3XZ(inputMovement);
                        var edge = grippablePoint.GetEdgeFromDirection(inputMovementXZ);
                        ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, edge);
                        moveVec = grippablePoint.CalcMoveDirction(inputMovementXZ);
                    }
                    else
                    {
                        // 入力なしで後方の手を近づける
                        system.handMovementMode = HandMovementMode.Close;
                        break;
                    }

                    // 手同士の距離が最大を超えた時　後方の手を近づける状態に遷移
                    if (maxHandToHand < magnitudeHandToHand)
                    {
                        system.handMovementMode = HandMovementMode.Close;
                        break;
                    }

                    // 前方の手を進める
                    if (maxHandToHand > magnitudeHandToHand)
                    {
                        var movement = ClimberMethod.CalcLerpTranslation(
                            moveVec.normalized,
                            magnitudeHandToHand,
                            system.handMovementSpdFactor);
                        movement = grippablePoint.ClampHandsMovement(forwardHand.transform.position, movement);

                        forwardHand.transform.Translate(movement, Space.World);
                    }
                    break;

                case HandMovementMode.Close:
                    // 移動入力値が存在する
                    bool isInputMovement = inputMovement.x > system.ableInputAreaForMovement || 
                        inputMovement.x < -system.ableInputAreaForMovement;

                    if (isInputMovement)
                    {
                        // 後ろの手を前の手に限界まで近づけたら遷移
                        if (minHandToHand > magnitudeHandToHand)
                        {
                            system.handMovementMode = HandMovementMode.Advance;
                            break;
                        }
                    }

                    // 後方の手を近づける
                    if (minHandToHand < magnitudeHandToHand)
                    {
                        moveVec = forwardHand.transform.position - backHand.transform.position;

                        float limit = -minHandToHand / 2;
                        var result = ClimberMethod.CalcLerpTranslation(
                            moveVec.normalized,
                            magnitudeHandToHand - limit,
                            system.handMovementSpdFactor);
                        backHand.transform.Translate(result, Space.World);
                    }
                    break;

                default:
                    // 通らないはず
                    Debug.Log("HandMovementMode == None");
                    break;
            }

        }

    }
}
