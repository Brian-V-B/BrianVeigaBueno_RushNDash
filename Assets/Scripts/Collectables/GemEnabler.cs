using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemEnabler : MonoBehaviour
{
    [SerializeField] private Gem gem;


    // Start is called before the first frame update
    void Start()
    {
        if (gem != null)
            gem.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (gem != null)
        {
            gem.gameObject.SetActive(true);
            gem.transform.parent = null;
            gem.UpdateModel();
        }
    }
}
