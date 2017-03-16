using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    public abstract class BasePlayer : MonoBehaviour
    {
        public Transform FireballSpawn;
        public Fireball Fireball;
        public Animator Anim;
        public Image HealthImage;

        private AudioSource _fireAudioPlay;

        protected virtual void Start()
        {
            HealthImage.transform.parent = GameManager.Instance.Canvas.transform;
            _fireAudioPlay = GetComponent<AudioSource>();
        }

        protected virtual void Update()
        {
            var posHealthImage = Camera.main.WorldToScreenPoint(transform.position);
            HealthImage.rectTransform.position = posHealthImage + new Vector3(0, 45, 0);
        }

        public virtual void Fire()
        {
            Anim.SetTrigger("Fire");
            _fireAudioPlay.Play();
        }

        void OnDestroy()
        {
            Destroy(HealthImage.gameObject);
        }
    }
}
