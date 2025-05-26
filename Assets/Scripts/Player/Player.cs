using System.Collections;
using UnityEngine;

public class Player : Object
{
    // La referencia a la entidad que controla el jugador.
    public static PlayerEntity Entity = null;

    // La posición (Global) donde reaparecerá el jugador si muere.
    public static Vector3 RespawnPosition;

    // La dirección (Global) que el jugador mirará a cuando reaparece.
    public static Vector3 RespawnDirection;


    /// <summary>
    /// Comprueba si existe un PlayerEntity unido al jugador, si existe, devuelve esta entidad.
    /// </summary>
    /// <param name="playerEntity">La entidad del jugador</param>
    /// <returns>Es verdadero si existe PlayerEntity.</returns>
    public static bool TryGetEntity(out PlayerEntity playerEntity)
    {
        playerEntity = Entity;
        return playerEntity != null;
    }



    public static void RespawnEntity(PlayerEntity prefab)
    {
        if (Entity == null)
        {
            PlayerEntity e = Instantiate(prefab, RespawnPosition, Quaternion.LookRotation(Vector3.forward));
            e.SetLook(RespawnDirection);
        }
    }
}
