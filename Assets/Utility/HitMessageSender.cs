using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CallBackFuncType = System.Action<HitMessageSender, UnityEngine.Collider>;

public class HitMessageSender : MonoBehaviour {

	/*
     * コンポーネントの追加と
     * receiverの登録
     */
    public static HitMessageSender AddHitMessageSender(GameObject sender, CallBackFuncType[] callBackFuncs, string[] info)
    {
        //Debug.Assert(receiver != null);
        Debug.Assert(sender != null);

        var comp = sender.AddComponent<HitMessageSender>();

        Debug.Assert(callBackFuncs.Length == comp.callbackFuncs.Length);
        comp.callbackFuncs = callBackFuncs;

        Debug.Assert(info.Length == comp.info.Length);
        comp.info = info;
        comp.rigid = sender.GetComponent<Rigidbody>();
        return comp;
    }

    // あたり判定時のコールバック関数
    public CallBackFuncType[] callbackFuncs = 
        new CallBackFuncType[3];

    public string[] info = new string[1];

    public Rigidbody rigid;

    /*
     * 0 OnTriggerEnter
     * 1 OnTriggerStay
     * 2 OnTriggerExit
     */

    //private void OnCollisionEnter(Collision collision)
    //{
    //    callbackFuncs[0](this, collision);
    //}

    //private void OnCollisionStay(Collision collision)
    //{
    //    callbackFuncs[1](this, collision);
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    callbackFuncs[2](this, collision);
    //}

    private void OnTriggerEnter(Collider collider)
    {
        callbackFuncs[0](this, collider);
    }

    private void OnTriggerStay(Collider collider)
    {
        callbackFuncs[1](this, collider);
    }

    private void OnTriggerExit(Collider collider)
    {
        callbackFuncs[2](this, collider);
    }

}
