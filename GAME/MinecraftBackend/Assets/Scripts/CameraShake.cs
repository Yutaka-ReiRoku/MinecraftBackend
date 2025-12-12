using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    
    private Vector3 _originalPos;
    private Transform _transform;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _transform = GetComponent<Transform>();
        _originalPos = _transform.localPosition;
    }

    
    
    
    
    
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); 
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            
            
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;

            yield return null; 
        }

        
        _transform.localPosition = _originalPos;
    }
}