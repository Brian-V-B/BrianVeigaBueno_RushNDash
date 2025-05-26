using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Entity
{
    private NavMeshAgent _navMeshAgent;

    protected override void Start()
    {
        base.Start();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }


    // Update is called once per frame
    private void Update()
    {
        if (Player.TryGetEntity(out PlayerEntity player))
        {
            NavMeshPath path = new NavMeshPath();
            if (Vector3.Distance(transform.position, player.transform.position) < 70f && !_navMeshAgent.Raycast(player.CameraAnchor.position, out NavMeshHit hit))
                if (_navMeshAgent.CalculatePath(player.transform.position, path))
                    _navMeshAgent.SetPath(path);
        }
    }
}
