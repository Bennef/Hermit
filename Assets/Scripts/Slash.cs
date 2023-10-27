using System.Collections;
using UnityEngine;

public class Slash : MonoBehaviour
{
    //[SerializeField] float fadeSpeed = 1f; 

    void OnEnable () => StartCoroutine(Attack());
 
    IEnumerator Attack() 
    {
        float alpha = 1.0f;
        while (alpha > 0.0f) {
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }
    }
}
