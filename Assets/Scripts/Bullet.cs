using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView photonV;
    int dir;

    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Ground")
            photonV.RPC("DestroyRPC", RpcTarget.AllBuffered);

        if (!photonV.IsMine && collider.tag == "Player" && collider.GetComponent<PhotonView>().IsMine)
        {
            collider.GetComponent<Player>().Hit();
            photonV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(int dir)
    {
        this.dir = dir;
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}