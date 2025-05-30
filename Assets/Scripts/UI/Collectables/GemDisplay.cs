using System.Collections;
using UnityEngine;
using TMPro;


/// <summary>
/// Componente que enseña la cantidad de gemas coleccionadas, si el mapa está cargado.
/// </summary>
public class GemDisplay : MonoBehaviour
{
    private TextMeshProUGUI _text;



    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }


    // Update is called once per frame
    void Update()
    {
        if (_text != null && Map.TryGetMap(out Map map))
        {
            _text.text = map.Gems + "/" + map.TotalGems;
        }
    }
}
