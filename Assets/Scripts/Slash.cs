using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    Renderer gridSquareRenderer;
    public float fadeSpeed = 1f;    // How fast alpha value decreases.
    private Material m_Material;    // Used to store material reference.
    private Color m_Color;   

    void Awake() {
        gridSquareRenderer = gameObject.GetComponent<Renderer>();    
        m_Material = GetComponent <Renderer>().material;
    }

    void OnEnable () {
        StartCoroutine(Attack());
    }
    

    IEnumerator Attack() {
        float alpha = 1.0f;
        while (alpha > 0.0f) {
            //alpha -= fadeSpeed * Time.deltaTime;
            //m_Material.color = new Color (m_Color.r, m_Color.g, m_Color.b, alpha);
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }
    }
}
