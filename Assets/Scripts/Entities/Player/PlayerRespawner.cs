using System.Collections;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    [SerializeField] private bool _triggerOnStart = false;



    private void Start()
    {
        if (_triggerOnStart)
            SetSpawn();
    }


    private void SetSpawn()
    {
        Player.RespawnPosition = transform.position;
        Player.RespawnDirection = transform.forward;
    }


    private void OnTriggerEnter(Collider other)
    {
        SetSpawn();
    }
}
