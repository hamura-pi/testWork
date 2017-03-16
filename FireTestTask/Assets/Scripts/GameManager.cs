using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using Assets.Scripts.Player;
using TestNetwork;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Canvas Canvas;
        public RectTransform[] element;

        public ClientPlayer Player;
        public EnemyPlayer Enemy;

        public List<Fireball> PlayerFireballs;
        public List<Fireball> EnemyFireballs;
        public bool IsHitEnemy { get; set; }

        public static GameManager Instance { get; private set; }

        private ClientPlayer currentPlayer;
        private EnemyPlayer currentEnemy;
    
        private int indexDestroyFireball;
        private bool isDestroyedFireball;
    

        private GameObject enemyFireParent;
        private GameObject playerFireParent;

        void Start ()
        {
            Instance = this;

            enemyFireParent = GameObject.Find("EnemyFireBalls");
            playerFireParent = GameObject.Find("PlayerFireBalls");

            PlayerFireballs = new List<Fireball>();
            EnemyFireballs = new List<Fireball>();

            NetworkLLAPI.Instance.DoneConnected += PlayerInstance;
            NetworkLLAPI.Instance.ConnectedEvent += EnemyInstance;
            NetworkLLAPI.Instance.OnGameDataReceived += GameDataRecive;
            NetworkLLAPI.Instance.Disconnected += Disconnected;
        }
	
	
        void Update ()
        {
            if (currentPlayer != null)
            {
                var sendData = PackagingSendData();

                if (IsHitEnemy)
                {
                    if (currentEnemy != null)
                    {
                        currentEnemy.HitEnemy();
                    }
                }

                NetworkLLAPI.Instance.SendPositionPlayer(sendData);

                IsHitEnemy = false;
                isDestroyedFireball = false;
            }

            
        }

        private GameData PackagingSendData()
        {
            GameData sendData;
            sendData.position = currentPlayer.transform.position;
            sendData.rotation = currentPlayer.transform.rotation.eulerAngles;
            sendData.isMove = currentPlayer.IsMove;
            sendData.isFire = currentPlayer.IsFire;
            sendData.fireBallCount = PlayerFireballs.Count;

            sendData.fireBallPosition = new List<Vector3>();
        
            foreach (var pFireballs in PlayerFireballs)
            {
                sendData.fireBallPosition.Add(pFireballs.transform.position);
            }

            sendData.isDestroyFireball = isDestroyedFireball;
            sendData.indexDestroyFireball = indexDestroyFireball;
            sendData.isHit = IsHitEnemy;
            sendData.isDeath = currentPlayer.IsDeath;
            return sendData;
        }

        private void PlayerInstance()
        {
            foreach (var e in element)
            {
                e.gameObject.SetActive(false);
            }

            currentPlayer = Instantiate(Player, Vector3.zero, Quaternion.identity);
            CameraController.Instance.player = currentPlayer.gameObject;
        }

        private void EnemyInstance()
        {
            if (currentEnemy == null)
            {
                currentEnemy = Instantiate(Enemy, Vector3.zero, Quaternion.identity);
            }
        }

        private void GameDataRecive(BinaryReader reader)
        {
            if (currentEnemy != null)
            {
                Vector3 pos;
                Vector3 rot;

                pos.x = reader.ReadSingle();
                pos.y = reader.ReadSingle();
                pos.z = reader.ReadSingle();

                rot.x = reader.ReadSingle();
                rot.y = reader.ReadSingle();
                rot.z = reader.ReadSingle();

                var isRun = reader.ReadBoolean();
                var isFire = reader.ReadBoolean();

                if (isFire)
                {
                    currentEnemy.Fire();
                    var fireball = currentEnemy.InstanceFireball();
                    fireball.transform.parent = enemyFireParent.transform;

                    EnemyFireballs.Add(fireball);
                }

                int countFireballs = reader.ReadInt32();
                for (int i = 0; i < countFireballs; i++)
                {
                    Vector3 posFire;

                    posFire.x = reader.ReadSingle();
                    posFire.y = reader.ReadSingle();
                    posFire.z = reader.ReadSingle();

                    if (EnemyFireballs[i].transform != null)
                    {
                        EnemyFireballs[i].transform.position = posFire;
                    }
                }
            
                bool isDestroyFireball = reader.ReadBoolean();
                if (isDestroyFireball)
                {
                    int index = reader.ReadInt32();

                    var fireball = EnemyFireballs[index];
                    Destroy(fireball.gameObject);
                    EnemyFireballs.RemoveAt(index);
                    Debug.Log("EnemyFireDestroy");
                }

                bool isHit = reader.ReadBoolean();

                if (isHit)
                {
                    currentPlayer.DetuctHeals();
                }

                bool isDeath = reader.ReadBoolean();

                if (isDeath)
                {
                    currentEnemy.Death();
                }

                currentEnemy.Anim.SetBool("isRun", isRun);
            
                currentEnemy.transform.position = pos;
                currentEnemy.transform.rotation = Quaternion.Euler(rot);
            }
        }

        public void DestroyPlayerFireBall(Fireball fireball)
        {
            isDestroyedFireball = true;
            indexDestroyFireball = PlayerFireballs.IndexOf(fireball);
            PlayerFireballs.Remove(fireball);
            Destroy(fireball.gameObject);
        }

        public void AddPlayerFireBalls(Fireball fireball)
        {
            PlayerFireballs.Add(fireball);
            fireball.transform.parent = playerFireParent.transform;
        }

        public void Disconnected()
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }
    }

    public struct GameData
    {
        public Vector3 position;
        public Vector3 rotation;

        public bool isMove;
        public bool isFire;

        public bool isDestroyFireball;
        public int indexDestroyFireball;

        public int fireBallCount;
        public List<Vector3> fireBallPosition;
        public bool isHit;
        public bool isDeath;
    }
}