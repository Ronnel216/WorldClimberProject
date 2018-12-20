using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingSystem : MonoBehaviour {

    [SerializeField]
    GameObject gripTop;
    [SerializeField]
    GameObject gripBottom;

    [SerializeField]
    GameObject leftHand;

    [SerializeField]
    GameObject rightHand;

    [SerializeField]
    GameObject garpAncharBase;
    [SerializeField]
    GameObject gripAnchar;

    [SerializeField]
    SubCollider nearGrippable;

    [SerializeField]
    float armLength = 1f;

    Rigidbody rigid;

    [SerializeField]
    float handMovement = 0.1f;

    [SerializeField]
    float handMovementSpdFactor = 0.03f;

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
        GameObject hand;
        Vector3 target;
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
        ClimberMethod.SetHandForwardAndBack(ref rightHand, ref leftHand);

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

        GripPoint gripPoint;

        CharacterJoint grippingJoint;
        private bool isChangeGripPoint;

        // Use this for initialization
        public void Init(ClimbingSystem system)
        {
            // 掴んでいる地形
            gripPoint = system.gripTop.GetComponent<GripPoint>();

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
            ClimberMethod.SetGrippingAnchar(system.garpAncharBase.GetComponent<Rigidbody>(), (handsPos[0] + handsPos[1]) / 2);

            // アンカーの状態を反映する
            ClimberMethod.ApplyGrippingAnchar(system.rigid, system.gripAnchar.GetComponent<Rigidbody>());

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Display");
            }
        }

        // Update is called once per frame
        public void Update(ClimbingSystem system)
        {
            //if (isGrip == false) return;
            
            if (isChangeGripPoint)
            {
                float step = gripPoint.SetHandsPosition(forwardHand, backHand, system.grippingCollider, 0.5f);
                bool isFinished = 0.01f > step;
                if (isFinished)
                {
                    isChangeGripPoint = false;
                    gripPoint.SetHandsPosition(forwardHand, backHand, system.grippingCollider);
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

            var inputMovement = ClimberMethod.GetInputMovement();

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector3 movement = Vector3.up;

                float minDistanceSqr = float.MaxValue;
                Collider nearGripColi = null;
                Vector3 basePos = system.nearGrippable.transform.position;
                foreach (var collider in system.nearGrippable.Colliders)
                {
                    Vector3 point = collider.ClosestPoint(basePos + movement);

                    // 移動入力方向に存在しない
                    if (Vector3.Dot(point - basePos, movement) <= 0) continue;

                    float distanceSqr = (point - basePos).sqrMagnitude;
                    if (minDistanceSqr > distanceSqr)
                    {
                        minDistanceSqr = distanceSqr;
                        nearGripColi = collider;
                    }

                }

                if (nearGripColi != null)
                {
                    Debug.Log("Ok Grip");
                    gripPoint.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");

                    system.grippingCollider = nearGripColi;
                    gripPoint = nearGripColi.gameObject.GetComponent<GripPoint>();
                    gripPoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
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
                Vector3 movement = Vector3.down;
                float minDistanceSqr = float.MaxValue;
                Collider nearGripColi = null;
                Vector3 basePos = system.nearGrippable.transform.position;
                foreach (var collider in system.nearGrippable.Colliders)
                {
                    Vector3 point = collider.ClosestPoint(basePos + movement);

                    // 移動入力方向に存在しない
                    if (Vector3.Dot(point - basePos, movement) <= 0) continue;

                    float distanceSqr = (point - basePos).sqrMagnitude;
                    if (minDistanceSqr > distanceSqr)
                    {
                        minDistanceSqr = distanceSqr;
                        nearGripColi = collider;
                    }
                                        
                }

                if (nearGripColi != null)
                {
                    Debug.Log("Ok Grip");
                    gripPoint.gameObject.layer = LayerMask.NameToLayer("GrippingPoint");

                    system.grippingCollider = nearGripColi;
                    gripPoint = nearGripColi.gameObject.GetComponent<GripPoint>();
                    gripPoint.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    isChangeGripPoint = true;
                }

            }

            Vector3 moveVec = Vector3.zero;
            //GameObject forwardHand = null;
            switch (system.handMovementMode)
            {
                case HandMovementMode.Advance:
                    if (inputMovement.x > 0)
                    {

                        ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, gripPoint.GetEdge(Vector3.right));
                        moveVec = gripPoint.CalcMovement(Vector3.right);

                    }
                    else if (inputMovement.x < 0)
                    {
                        ClimberMethod.SetHandForwardAndBack(ref forwardHand, ref backHand, gripPoint.GetEdge(Vector3.left));
                        moveVec = gripPoint.CalcMovement(Vector3.left);
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
                        var result = ClimberMethod.CalcLerpTranslation(
                            moveVec.normalized,
                            magnitudeHandToHand,
                            system.handMovementSpdFactor);
                        forwardHand.transform.Translate(result);
                    }
                    break;

                case HandMovementMode.Close:
                    // 前方の手を進める状態に遷移
                    bool isInputMovement = inputMovement.x > 0 || inputMovement.x < 0;
                    if (isInputMovement)
                    {
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
                        backHand.transform.Translate(result);
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
