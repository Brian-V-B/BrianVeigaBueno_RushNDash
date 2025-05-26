using System.Collections;
using UnityEngine;

public class Map : MonoBehaviour
{
    // El mapa actualmente cargado.
    private static Map _currentMap = null;


    //-- Collecionables Totales

    // Las gemas totales que fueron cargadas en el mapa.
    [HideInInspector] public int TotalGems;


    //-- Collecionables Collecionados

    // Las gemas que han sido collecionadas en el mapa.
    [HideInInspector] public int Gems = 0;



    //-- Métodos Núcleo --//

    private void Awake()
    {
        // Cuando se carga un mapa, el último mapa cargado tiene prioridad.
        _currentMap = this;
    }


    //-- Métodos Estáticos --//

    /// <summary>
    /// Comprueba si hay un mapa cargado, si existe, devuelve este mapa.
    /// </summary>
    /// <param name="map">El mapa actualmente cargado.</param>
    /// <returns>Es verdadero si hay un mapa valido cargado.</returns>
    public static bool TryGetMap(out Map map)
    {
        map = _currentMap;
        return map != null;
    }


    public static void AddGemTotal(int amount)
    {
        if (amount > 0 && TryGetMap(out Map map))
            map.TotalGems += amount;
    }


    /// <summary>
    /// Devuelve si hay un mapa cargado.
    /// </summary>
    public static bool IsMapLoaded()
    {
        return _currentMap != null;
    }
}
