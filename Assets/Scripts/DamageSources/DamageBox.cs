using System.Collections;
using UnityEngine;

public class DamageBox : MonoBehaviour
{
    [Tooltip("Si este trigger de daño puede hacer daño al jugador.")]
    [SerializeField] protected bool _damagePlayer = true;

    [Tooltip("Si este trigger de daño puede hacer daño a enemigos.")]
    [SerializeField] protected bool _damageEnemy = false;

    [Tooltip("Si el proyectil puede destruir objetos 'duros', normalmente solo destructibles por la carga del jugador.")]
    [SerializeField] protected bool _damageTough = false;

    [Tooltip("Si es verdadero, este damage box no hace daño al jugador si está cargando.")]
    [SerializeField] protected bool _canHitCharge = false;

    [Tooltip("Si es verdadero, el jugador no puede hacer nada después de recibir el golpe.")]
    [SerializeField] protected bool _stuns = true;

    [Tooltip("Si golpea a un blanco, se aplica este knockback.")]
    [SerializeField] protected float _knockback = 1.5f;

    [Tooltip("El daño que hace este trigger de daño.")]
    [SerializeField] protected int _damage = 1;


    protected void OnTriggerEnter(Collider other)
    {
        if (TargetCheck(other) && other.TryGetComponent(out Entity e))
        {
            e.Damage(_damage);
            e.Knockback((e.transform.position - transform.position + Vector3.up * 3).normalized * _knockback, true, _stuns);
        }
    }


    
    /// <summary>
    /// Se comprueba si el objetivo es jugador y es posible golpearlo por eso.
    /// </summary>
    /// <param name="target">El objetivo a golpear.</param>
    /// <returns>Es verdadero si el objetivo es válido y golpeable.</returns>
    protected bool TargetCheck(Collider target)
    {
        if (target == null)
            return false;

        // Si tiene el tag de duro (Tough) y no se puede golpear cosas duras, el check es falso.
        if (target.CompareTag("Tough") && !_damageTough)
            return false;

        // Comprobar si se golpea a un jugador o un enemigo.
        bool playerTag = target.CompareTag("Player");
        if (playerTag)
            return _damagePlayer && (!Player.Entity.InputDash || (Player.Entity.InputDash && _canHitCharge));
        else
            return _damageEnemy;
    }
}
