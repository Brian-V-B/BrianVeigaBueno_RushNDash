using System.Collections;
using UnityEngine;

public class CollectableDisplay : MonoBehaviour
{
    // Tiempo que tiene que pasar hasta que se enseñe los collecionables de forma pasiva.
    private const float _timeToDisplay = 1.0f;


    private static CollectableDisplay _instance;


    // Modifica como de rápido se enseña el CollectableDisplay.
    [SerializeField] private float _displaySpeed = 2.5f;

    // Tiempo que ha pasado con el jugador estando quieto.
    private float _timer = 0f;

    // Si es verdadero, timer pasa a reducirse con el tiempo incluso cuando se mueve, 
    // solo escondiendo el CollectableDisplay cuando llega a cero.
    private bool _pickUpState = false;



    //-- Métodos Núcleo --//

    private void Start()
    {
        _instance = this;
    }


    // Update is called once per frame
    void Update()
    {
        if (!Map.IsMapLoaded())
        {
            Hide();
            return;
        }

        if (_pickUpState)
        {
            _timer -= Time.deltaTime;
            if (_timer > 0)
            {
                Show();
            }
            else
            {
                _pickUpState = false;
                Hide();
            }
        }
        else if (Player.TryGetEntity(out PlayerEntity entity))
        {
            if (entity.Rigidbody.velocity.sqrMagnitude < 2.0f)
            {
                if (_timer >= _timeToDisplay)
                {
                    Show();
                }
                else
                {
                    _timer += Time.deltaTime;
                    Hide();
                }
            }
            else
            {
                _timer = 0f;
                Hide();
            }
        }
        else
            Hide();
    }


    //-- Show/Hide --//

    /// <summary>
    /// Anima el panel para enseñar los collecionables.
    /// </summary>
    private void Show()
    {
        RectTransform t = GetComponent<RectTransform>();

        Vector3 v = t.localScale;
        v.x = Mathf.MoveTowards(v.x, 1.0f, _displaySpeed * Time.deltaTime);
        t.localScale = v;

        v = t.anchoredPosition3D;
        v.x = Mathf.MoveTowards(v.x, 0.0f, _displaySpeed * 250 * Time.deltaTime);
        t.anchoredPosition3D = v;
    }


    /// <summary>
    /// Anima el panel para esconder los collecionables.
    /// </summary>
    private void Hide()
    {
        RectTransform t = GetComponent<RectTransform>();

        Vector3 v = t.localScale;
        v.x = Mathf.MoveTowards(v.x, 0.0f, _displaySpeed * Time.deltaTime);
        t.localScale = v;

        v = t.anchoredPosition3D;
        v.x = Mathf.MoveTowards(v.x, 250.0f, _displaySpeed * 100 * Time.deltaTime);
        t.anchoredPosition3D = v;
    }


    //-- Métodos Públicos --//

    /// <summary>
    /// Forzar el CollectableDisplay a ser visible.
    /// </summary>
    /// <param name="time">El tiempo que es visible antes de esconderse.</param>
    public static void TriggerPickUpState(float time = 2.0f)
    {
        _instance._pickUpState = true;
        _instance._timer = time;
    }
}
