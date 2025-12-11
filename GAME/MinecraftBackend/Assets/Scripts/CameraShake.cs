using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    // Lưu vị trí gốc của Camera để trả về sau khi rung xong
    private Vector3 _originalPos;
    private Transform _transform;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _transform = GetComponent<Transform>();
        _originalPos = _transform.localPosition;
    }

    /// <summary>
    /// Kích hoạt hiệu ứng rung
    /// </summary>
    /// <param name="duration">Thời gian rung (giây)</param>
    /// <param name="magnitude">Độ mạnh (0.1 - 1.0)</param>
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // Dừng rung cũ nếu có
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Tạo vị trí ngẫu nhiên trong hình cầu bán kính = magnitude
            // Chỉ rung trục X và Y để giữ Z (độ sâu) ổn định cho game 2D/UI
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;

            yield return null; // Chờ frame tiếp theo
        }

        // Trả về vị trí cũ
        _transform.localPosition = _originalPos;
    }
}