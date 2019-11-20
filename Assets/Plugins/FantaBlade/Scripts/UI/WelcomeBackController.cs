using FantaBlade.Internal;
using FantaBlade.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeBackController : MonoBehaviour, IController
{

    public Animator rootAnimator;
    public Text nickName;

    public void Init()
    {
        if(null != rootAnimator)
        {
            rootAnimator.SetTrigger("on_display");
        }
        //nickName.text = SdkManager.Auth.Username + ", " + SdkManager.Localize.GetText("welcome_back_tip");
        nickName.text = SdkManager.Localize.GetText("welcome_back_tip");
    }

    void Update()
    {
        //if(rootAnimator.GetCurrentAnimatorStateInfo(rootAnimator.GetLayerIndex("Base Layer")).IsName("Empty"))
        //{
        //    Finish();
        //}
    }

    /// <summary>
    /// call when animation finish
    /// </summary>
    public void Finish()
    {
        SdkManager.Ui.HideNormalUI((int)NormalUIID.WelcomeBack);
    }

    // Use this for initialization
    void Start () {
		
	}
	
}
