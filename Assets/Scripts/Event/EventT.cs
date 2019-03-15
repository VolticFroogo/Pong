using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class EventT : NetworkBehaviour
{
    public enum Events
    {
        None,
        Speed,
        Grow,
        Shrink
    }

    [SyncVar]
    private Events Type;

    private Events CurrentSprite;

    [HideInInspector]
    public SpriteRenderer Renderer;

    public List<Sprite> Sprites;

    void Awake()
    {
        // Get our sprite renderer.
        Renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (isServer)
            // Pick our ability skipping over none.
            Type = (Events)Random.Range(1, Events.GetValues(typeof(Events)).Length);
    }

    void Update()
    {
        // If the sprite isn't set yet:
        if (CurrentSprite != Type)
        {
            // Set our sprite corresponding to the ability we have.
            Renderer.sprite = Sprites[(int)Type - 1];

            // Set our current sprite variable to our ability.
            CurrentSprite = Type;
        }
    }

    [ClientRpc] // Server --> Client
    public void RpcHit(Events eventT)
    {
        HitAction(eventT);
    }

    public void HitAction(Events eventT)
    {
        Debug.Log("Hit action!");

        if (EventSpawner.Instance.ActiveEvent())
            EventSpawner.Instance.EndEvent();

        switch (eventT)
        {
            case Events.Grow:
                EventSpawner.Instance.Active = eventT;
                EventSpawner.Instance.EventEnd = BallSize.Duration + Time.time;

                BallSize.Grow();

                break;

            case Events.Shrink:
                EventSpawner.Instance.Active = eventT;
                EventSpawner.Instance.EventEnd = BallSize.Duration + Time.time;

                BallSize.Shrink();

                break;
        }
    }

    public void Hit()
    {
        Debug.Log("Hit!");

        HitAction(Type);

        RpcHit(Type);

        switch (Type)
        {
            case Events.Speed:
                Speed.Use();

                break;
        }
    }
}
