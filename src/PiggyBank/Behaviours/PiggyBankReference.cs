using Photon.Pun;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace PiggyBank.Behaviours;

public struct PiggyBankReference: IBinarySerializable
{
    public Transform locationTransform;

    public void Serialize(BinarySerializer serializer)
    {
        serializer.WriteInt(view.ViewID);
    }

    public void Deserialize(BinaryDeserializer deserializer)
    {
        view = PhotonView.Find(deserializer.ReadInt());
    }

    public static PiggyBankReference GetFromBackpackItem(Item item)
    {
        PiggyBankReference result = default(PiggyBankReference);
        result.view = item.GetComponent<PhotonView>();
        result.locationTransform = item.transform;
        return result;
    }
    
    public PhotonView view;
    
    public bool TryGetPiggyBankItem(out PiggyBankController piggyBank)
    {
        piggyBank = view.GetComponent<PiggyBankController>();
        return true;
    }

}