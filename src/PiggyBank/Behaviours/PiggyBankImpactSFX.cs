using UnityEngine;

namespace PiggyBank.Behaviours;

public class PiggyBankImpactSFX : MonoBehaviour
{
    public float vel;

    private Rigidbody rig;

    private Item item;
    public float velMult = 1f;

    public bool disallowInHands;


    public SFX_Instance[] impact;
    public SFX_Instance[] impact_empty;
    public SFX_Instance[] impact_full;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        item = GetComponent<Item>();
    }

    private void Update()
    {
        if ((bool) rig && !rig.isKinematic)
        {
            vel = Mathf.Lerp(vel, Vector3.SqrMagnitude(rig.linearVelocity) * velMult, 10f * Time.deltaTime);
        }
    }

    private void playEmptySFX()
    {
        for (int i = 0; i < impact_empty.Length; i++)
        {
            impact_empty[i].Play(base.transform.position);
        }
    }

    private void playFullSFX()
    {
        for (int i = 0; i < impact_full.Length; i++)
        {
            impact_full[i].Play(base.transform.position);
        }
    }

    private void PlayAdditionalSfx()
    {
        if (Plugin.IsBankFree)
        {
            playEmptySFX();
        }
        else
        {
            playFullSFX();
        }
    }

    private void PlayImpactSFX()
    {
        foreach (var sfx in impact)
        {
            sfx.Play(base.transform.position);
        }

        PlayAdditionalSfx();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!rig)
        {
            return;
        }

        if ((bool) item)
        {
            if (!item.holderCharacter)
            {
                if (vel > 4f)
                {
                    PlayImpactSFX();
                }
            }
            else if (vel > 36f && !disallowInHands)
            {
                PlayImpactSFX();
            }
        }

        if (!item && !collision.rigidbody && vel > 64f)
        {
            PlayImpactSFX();
        }

        vel = 0f;
    }
}