using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class OrbManager : MonoBehaviour
{
    public static OrbManager Instance;

    private VisualElement _root;
    private VisualElement _expBarTarget; // Mục tiêu để hạt bay vào (Thanh EXP)

    [Header("Settings")]
    public float FlySpeed = 1.5f; // Tốc độ bay
    public string OrbImageUrl = "/images/others/exp.png";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            _root = uiDoc.rootVisualElement;
            // Tìm thanh ExpBar để lấy toạ độ đích
            // Lưu ý: Cần đảm bảo trong UXML đã đặt tên là "ExpBar"
            _expBarTarget = _root.Q<VisualElement>("ExpBar");
        }
    }

    /// <summary>
    /// Bắn ra một chùm hạt kinh nghiệm
    /// </summary>
    /// <param name="startPos">Vị trí bắt đầu (thường là vị trí nút bấm hoặc quái chết)</param>
    /// <param name="amount">Số lượng Exp nhận được (để tính số hạt)</param>
    public void SpawnOrbs(Vector2 startPos, int amount)
    {
        if (_root == null || _expBarTarget == null) return;

        // Tính toán số lượng hạt hiển thị (giới hạn để không lag)
        // Ví dụ: 1 hạt cho mỗi 5 exp, tối đa 20 hạt
        int orbCount = Mathf.Clamp(amount / 5, 1, 20);

        for (int i = 0; i < orbCount; i++)
        {
            // Bay lệch pha nhau một chút cho tự nhiên
            StartCoroutine(FlyOrbProcess(startPos, i * 0.1f)); 
        }
    }

    private IEnumerator FlyOrbProcess(Vector2 startPos, float delay)
    {
        // Chờ delay kích hoạt
        yield return new WaitForSeconds(delay);

        // 1. Tạo VisualElement cho hạt
        var orb = new Image();
        orb.style.width = 20; 
        orb.style.height = 20;
        orb.style.position = Position.Absolute;
        
        // Spawn ngẫu nhiên xung quanh điểm gốc một chút (bán kính 50px)
        float offsetX = Random.Range(-50f, 50f);
        float offsetY = Random.Range(-50f, 50f);
        
        // Thiết lập vị trí ban đầu
        // Lưu ý: UI Toolkit toạ độ (0,0) là góc trên-trái
        orb.style.left = startPos.x + offsetX; 
        orb.style.top = startPos.y + offsetY;
        
        // Tắt bắt sự kiện chuột
        orb.pickingMode = PickingMode.Ignore;

        // Tải ảnh từ Server
        StartCoroutine(orb.LoadImage(OrbImageUrl));

        _root.Add(orb);

        // 2. Logic Bay (Animation)
        float t = 0;
        Vector2 p0 = new Vector2(orb.style.left.value.value, orb.style.top.value.value);
        
        while (t < 1)
        {
            t += Time.deltaTime * FlySpeed;

            // Lấy vị trí đích mới nhất (vì thanh Exp có thể di chuyển nếu resize màn hình)
            // worldBound trả về khung hình chữ nhật tuyệt đối trên màn hình
            Vector2 targetPos = _expBarTarget.worldBound.center;

            // Sử dụng Lerp (Nội suy tuyến tính) hoặc SmoothStep để di chuyển
            // Dùng t * t để tạo hiệu ứng "Ease In" (ban đầu chậm, sau nhanh dần như bị hút)
            Vector2 currentPos = Vector2.Lerp(p0, targetPos, t * t);

            orb.style.left = currentPos.x;
            orb.style.top = currentPos.y;

            yield return null;
        }

        // 3. Kết thúc
        if (AudioManager.Instance != null) 
        {
            // Âm thanh 'ding' nhỏ, pitch ngẫu nhiên để nghe vui tai
            // AudioManager.Instance.PlaySFX("orb_pickup", Random.Range(0.8f, 1.2f));
            // Tạm dùng tiếng click nếu chưa có file riêng
            AudioManager.Instance.PlaySFX("click"); 
        }

        _root.Remove(orb); // Xóa khỏi UI
    }
}