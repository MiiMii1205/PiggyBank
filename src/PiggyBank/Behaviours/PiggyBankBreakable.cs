using System;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace PiggyBank.Behaviours;

[RequireComponent(typeof(PhotonView))]
public class PiggyBankBreakable : Breakable
{
    private bool m_justThrown;

    public void Awake()
    {
        base.Awake();
        GlobalEvents.OnItemThrown += OnItemThrown;
    }

    private void OnItemThrown(Item obj)
    {
        if (obj == item)
        {
            m_justThrown = true;    
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!item.photonView.IsMine || item.itemState != ItemState.Ground || !breakOnCollision ||
            !item.rig ||
            collision.relativeVelocity.magnitude <= minBreakVelocity)
        {
            m_justThrown = false;
        }
        base.OnCollisionEnter(collision);
    }
    
    public override void Break(Collision coll)
    {
        if (this.item is PiggyBankController && m_justThrown && this.item.lastThrownCharacter.IsLocal)
        {
            // If there's an item in the bank then spawn it

            if (!Plugin.IsBankFree && Plugin.WithdrawFromBank(out var i, false, transform.position + coll.contacts[0].normal))
            {
                i = i ?? throw new NullReferenceException(nameof(i));
                i.rig.linearVelocity = this.item.rig.linearVelocity;
                i.rig.angularVelocity = this.item.rig.linearVelocity;
                i.transform.up = coll.contacts[0].normal;
                Plugin.ClearBank();
            }
        }
        
        base.Break(coll);
    }
    
    [PunRPC]
    public void RPC_NonItemBreak()
    {
        // Stupid Photon being stupid
        base.RPC_NonItemBreak();
    }
    
}