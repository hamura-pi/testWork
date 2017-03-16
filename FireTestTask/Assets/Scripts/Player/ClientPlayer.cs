using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    public class ClientPlayer : BasePlayer
    {
        public float SpeedPlayer = 3.0f;
        public float TimeReloadFire = 1f;
        public int health = 100;

        public bool IsMove
        {
            get   { return _isMove; }
            private set { _isMove = value; }
        }

        public bool IsFire { get;  private set; }

        public bool IsDeath { get; private set; }
        
        private bool _isReload;
        private bool _isMove;
        private RaycastHit hitInfo;
        protected override void Update()
        {
            if (!IsDeath)
            {
                MovePlayer();
                TurnAndFirePlayer();
            }

            base.Update();
        }

        private void TurnAndFirePlayer()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    transform.LookAt(new Vector3(hitInfo.point.x, 0, hitInfo.point.z));
                    if (!_isReload)
                    {
                        IsFire = true;
                        _isReload = true;
                        
                        Fire();
                        StartCoroutine(DelayReload());
                    }
                }

            }
            else if (IsFire)
            {
                IsFire = false;
            }
        }

        public override void Fire()
        {
            base.Fire();

            var fireball = Instantiate(Fireball, FireballSpawn.position, Quaternion.identity) as Fireball;

            GameManager.Instance.AddPlayerFireBalls(fireball);
            var point = hitInfo.point;

            Vector3 directionFly = (new Vector3(point.x, FireballSpawn.position.y, point.z) - FireballSpawn.position)
                    .normalized;

            fireball.StartFireball(directionFly);
        }

        private void MovePlayer()
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) 
                || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                IsMove = true;

                var x = Input.GetAxis("Horizontal")*Time.deltaTime*SpeedPlayer;
                var z = Input.GetAxis("Vertical")*Time.deltaTime*SpeedPlayer;
                
                Anim.SetBool("isRun", IsMove);

                transform.Translate(x, 0, z, Space.World);
            }
            else
            {
                IsMove = false;
                Anim.SetBool("isRun", IsMove);
            }
        }

        IEnumerator DelayReload()
        {
            yield return new WaitForSeconds(TimeReloadFire);
            _isReload = false;
        }

        public void DetuctHeals()
        {
            if (health > 0)
            {
                health -= 20;
                if (HealthImage.rectTransform.localScale.y > 0)
                {
                    HealthImage.rectTransform.localScale -= new Vector3(0.2f, 0, 0);
                }

                Anim.SetTrigger("Hit");

                if (health <= 0)
                {
                    IsDeath = true;
                    Anim.SetTrigger("Death");
                }
            }
        }
    } 
}
