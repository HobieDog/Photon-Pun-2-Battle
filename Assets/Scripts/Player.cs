using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Cinemachine;

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

        if (photonV.IsMine)
        {
            // Follow Cinema Camera
            var CMcamera = GameObject.Find("CMcamera").GetComponent<CinemachineVirtualCamera>();
            CMcamera.Follow = transform;
            CMcamera.LookAt = transform;
        }
    }

    void Update()
    {
        if (photonV.IsMine)
        {
            //← → : Move
            float h = Input.GetAxisRaw("Horizontal");
            rigid.velocity = new Vector2(4 * h, rigid.velocity.y);

            if (h != 0)
            {
                anim.SetBool("walk", true);
                photonV.RPC("FlipXRPC", RpcTarget.AllBuffered, h); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered
            }
            else anim.SetBool("walk", false);

            // ↑ : Jump, Ground Check
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            anim.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGround)
                photonV.RPC("JumpRPC", RpcTarget.All);

            // Space : Shot
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(spriteRenderer.flipX ? -0.35f : 0.35f, -0.05f, 0), Quaternion.identity)
                    .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, spriteRenderer.flipX ? -1 : 1);
                anim.SetTrigger("shot");
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    public void Hit()
    {
        if (health.fillAmount <= 0)
            return;

        health.fillAmount -= 0.1f;
        if (health.fillAmount <= 0)
        {
            anim.SetTrigger("died");
            Invoke("Respawn", 3);
        }
    }

    void Respawn()
    {
        GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
        photonV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    //Move
    [PunRPC]
    void FlipXRPC(float h)
    {
        spriteRenderer.flipX = h == -1;
    }

    //Jump
    [PunRPC]
    void JumpRPC()
    {
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 700);
    }

    //Destroy
    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
