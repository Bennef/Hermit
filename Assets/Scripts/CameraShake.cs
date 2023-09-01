using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    float duration = 0.3f;
    float magnitude = 0.3f;

    private void Update() 
    {
        if (Input.GetKey(KeyCode.E)) 
        {
            StartCoroutine(Shake());
        }
    }

    public void CallShake() 
    {
        StartCoroutine(Shake());
    }

    public IEnumerator Shake() 
    {
        Vector3 startPos = transform.position;
        float elapsed = 0.0f;
        
        while (elapsed < duration) 
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = new Vector3(startPos.x + x, startPos.y + y, startPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(0, 1, -11);
    }
}