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
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(health.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            health.fillAmount = (float)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        //NickName
        nickNameText.text = photonV.IsMine ? PhotonNetwork.NickName : photonV.Owner.NickName;
        nickNameText.color = photonV.IsMine ? Color.green : Color.red;
    }

    void Update()
    {
        if (photonV.IsMine)
        {
            //← → Move
            float h = Input.GetAxisRaw("Horizontal");
            rigid.velocity = new Vector2(4 * h, rigid.velocity.y);

            if (h != 0)
            {
                anim.SetBool("walk", true);
                photonV.RPC("FlipXRPC", RpcTarget.AllBuffered, h); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered
            }
            else anim.SetBool("walk", false);

            // ↑ 점프, 바닥체크
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            anim.SetBool("jump", !isGround);
            if ((Input.GetKeyDown(KeyCode.UpArrow) && isGround) || (Input.GetKeyDown(KeyCode.Space) && isGround))
                photonV.RPC("JumpRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void FlipXRPC(float h) => spriteRenderer.flipX = h == -1;

    [PunRPC]
    void JumpRPC()
    {
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 600);
    }
}
