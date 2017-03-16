using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    public class EnemyPlayer : BasePlayer
    {
        public void HitEnemy()
        {
            Anim.SetTrigger("Hit");
            if (HealthImage.rectTransform.localScale.y > 0)
            {
                HealthImage.rectTransform.localScale -= new Vector3(0.2f, 0, 0);
            }
        }

        public Fireball InstanceFireball()
        {
            return Instantiate(Fireball, FireballSpawn.position, Quaternion.identity) as Fireball;
        }

        void OnDestroy()
        {
            Destroy(HealthImage.gameObject);
        }

        public void Death()
        {
            Anim.SetTrigger("Death");
        }
    }
}
