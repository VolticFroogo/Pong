using UnityEngine;
using Mirror;

namespace MirrorTest
{
    class MirrorTestPlayer : NetworkBehaviour
    {
        [SyncVar(hook = "OnChangeHealth")]
        int health;

        public void TakeDamage(int amount)
        {
            if (!isServer)
                return;

            health -= amount;
        }

        void OnChangeHealth()
        {
            // do things with your health bar
        }
    }
}
