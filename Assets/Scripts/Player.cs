using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rigid;
    public Animator anim;
    public PhotonView photonV;
    public Text nickNameText;
    public Image health;

    bool isGround;
    Vector3 curPos;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }

    void Awake()
    {
        //NickName
        nickNameText.text = photonV.IsMine ? PhotonNetwork.NickName : photonV.Owner.NickName;
        nickNameText.color = photonV.IsMine ? Color.green : Color.red;
    }

    void Update()
    {
        
    }
}
