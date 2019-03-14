using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    public enum Abilities
    {
        None,
        Grow,
        Shrink
    }

    private Abilities Ability;
    
    public List<Sprite> Sprites;

    void Start()
    {
        // Pick our ability skipping over none.
        Ability = (Abilities)Random.Range(1, Abilities.GetValues(typeof(Abilities)).Length);

        // Get our sprite renderer.
        var renderer = GetComponent<SpriteRenderer>();

        // Set our sprite corresponding to the ability we have.
        renderer.sprite = Sprites[(int)Ability - 1];
    }

    [ClientRpc] // Server --> Client
    public void RpcHit(uint netId)
    {
        // Call the hit function.
        // This is split into two functions because the server needs to call hit without an RPC.
        Hit(netId);
    }

    public void Hit(uint netId)
    {
        // Get the paddle that deserves the power up.
        var paddle = Paddle.FromNetId(netId);

        // Set the paddle's ability to this new ability.
        paddle.AbilitySlot = Ability;

        // If we just got a power up:
        if (paddle.isLocalPlayer)
        {
            // Tell our UI we have a new power up.
            PowerUpUI.Instance.Set(Ability);
        }
    }
}