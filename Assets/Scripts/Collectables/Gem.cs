using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [Tooltip("Cuanto vale la gema. Invalido si es cero o negativo.")]
    [SerializeField] private int _worth = 1;


    // Modelos de las gemas.
    [SerializeField] private GameObject _smallModel;
    [SerializeField] private GameObject _mediumModel;
    [SerializeField] private GameObject _largeModel;
    [SerializeField] private GameObject _superModel;


    //-- Métodos Núcleo --//

    // Start is called before the first frame update
    private void Start()
    {
        Map.AddGemTotal(_worth);
        UpdateModel();
    }


    //-- Métodos Públicos --//

    /// <summary>
    /// Actualiza el modelo según el valor de la gema.
    /// </summary>
    public void UpdateModel()
    {
        if (_worth >= 25)
            SetModel(3);
        else if (_worth >= 10)
            SetModel(2);
        else if (_worth >= 5)
            SetModel(1);
        else if (_worth > 0)
            SetModel(0);
        else
            SetModel(-1);
    }


    //-- Métodos Privados --//

    // Animación que reproduce este objeto antes de ser borrado.
    private IEnumerator Animation()
    {
        float timer = 0.25f;
        while (timer > 0f)
        {
            transform.Translate(Vector3.up * 10.0f * Time.deltaTime);

            timer -= Time.deltaTime;
            yield return null;
        }

        if (Player.TryGetEntity(out PlayerEntity entity))
        {
            timer = 1f;
            while (entity != null && Vector3.Distance(transform.position, entity.transform.position) > 1f)
            {
                timer += Time.deltaTime * 10;
                transform.position = Vector3.Slerp(transform.position, entity.transform.position, 16f * timer * Time.deltaTime);
                yield return null;
            }
        }

        Destroy(gameObject);
    }


    /// <summary>
    /// Activa uno de 4 modelos dependiendo del index.
    /// </summary>
    private void SetModel(int index)
    {
        if (_smallModel != null)
            _smallModel.SetActive(index == 0);
        if (_mediumModel != null)
            _mediumModel.SetActive(index == 1);
        if (_largeModel != null)
            _largeModel.SetActive(index == 2);
        if (_superModel != null)
            _superModel.SetActive(index == 3);
    }


    //-- Eventos --//

    // Cuando se pilla, y un mapa está actualmente cargado, se añade a la cantidad de gemas collecionadas.
    private void OnTriggerEnter(Collider other)
    {
        // Si la entidad es un jugador y el mapa está cargado, colleciona la gema.
        if (_worth > 0 && Map.TryGetMap(out Map map))
        {
            map.Gems += _worth;
            CollectableDisplay.TriggerPickUpState();
            _worth = 0;
            StartCoroutine(Animation());
        }
    }
}
