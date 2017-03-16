using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float Speed = 0.3f;
    public float LifeTime = 1.5f;

    private bool isStartFire;
    private Vector3 directionFly;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (isStartFire)
	    {
	        transform.Translate(directionFly*Speed);
	    }
    }

    public void StartFireball(Vector3 direction)
    {
        isStartFire = true;
        directionFly = direction;
        StartCoroutine(DelayDestroy());
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Obstructions"))
        {
            FireballCollision();
        }
        else if (collider.CompareTag("Enemy"))
        {
            GameManager.Instance.IsHitEnemy = true;
            FireballCollision();
        }
    }

    private void FireballCollision()
    {
        isStartFire = false;
        StopCoroutine(DelayDestroy());
        GameManager.Instance.DestroyPlayerFireBall(this);
    }

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(LifeTime);
        GameManager.Instance.DestroyPlayerFireBall(this);
    }
}
