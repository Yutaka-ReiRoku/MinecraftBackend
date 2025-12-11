using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class SplashManager : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument SplashDoc;
    public float SplashDuration = 2.0f; // Thời gian chờ tối thiểu

    private VisualElement _root;
    private Label _tipLabel;
    private VisualElement _background;

    // Danh sách mẹo hiển thị ngẫu nhiên
    private readonly string[] _tips = new string[] {
        "Mẹo: Dùng Cúp (Pickaxe) để khai thác Đá hiệu quả hơn.",
        "Mẹo: Đói bụng? Hãy ăn Táo hoặc Bánh mì để hồi phục.",
        "Mẹo: Nâng cấp vũ khí tại Thợ Rèn để tăng sát thương.",
        "Mẹo: Gửi tiền vào Ngân hàng (Admin) để tránh bị rơi khi chết.",
        "Mẹo: Đăng nhập hàng ngày để nhận quà!",
        "Mẹo: Admin có thể tặng quà cho bạn qua Hòm Thư."
    };

    void Awake()
    {
        // Tối ưu hiệu năng: Khóa FPS ở 60 để tránh ngốn Pin/CPU vô ích
        Application.targetFrameRate = 60;
        // Tắt VSync để targetFrameRate hoạt động chuẩn
        QualitySettings.vSyncCount = 0;
    }

    IEnumerator Start()
    {
        if (SplashDoc == null) SplashDoc = GetComponent<UIDocument>();
        if (SplashDoc != null)
        {
            _root = SplashDoc.rootVisualElement;
            _tipLabel = _root.Q<Label>("LoadingTipLabel");
            _background = _root.Q<VisualElement>("ScreenContainer"); // Container chính để set nền
        }

        // 1. Hiển thị Mẹo ngẫu nhiên
        if (_tipLabel != null)
        {
            _tipLabel.text = _tips[Random.Range(0, _tips.Length)];
        }

        // 2. Tải Con trỏ chuột (Custom Cursor) từ Server
        yield return LoadCustomCursor();

        // 3. Tải Hình nền mặc định (Adventure)
        if (_background != null)
        {
            // Sử dụng Extension LoadBackgroundImage từ ImageLoader
            yield return _background.LoadBackgroundImage("/images/modes/adventure.png");
        }

        // Chờ đủ thời gian splash (hoặc chờ tải xong các tài nguyên thiết yếu khác nếu có)
        yield return new WaitForSeconds(SplashDuration);

        // 4. Kiểm tra điều hướng
        CheckLoginAndRedirect();
    }

    IEnumerator LoadCustomCursor()
    {
        // Tải cursor.png từ Server thay vì dùng mặc định của OS
        // Lưu ý: Cần đảm bảo file này có trên Server tại wwwroot/images/others/cursor.png
        string url = "http://localhost:5000/images/others/cursor.png"; 

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                // Set điểm hotpot (mũi nhọn) là góc trên trái (0,0)
                UnityEngine.Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                // Nếu lỗi thì dùng con trỏ mặc định, không sao cả
                Debug.LogWarning("Không tải được Cursor, dùng mặc định.");
            }
        }
    }

    void CheckLoginAndRedirect()
    {
        // Kiểm tra xem đã có Token đăng nhập chưa
        if (PlayerPrefs.HasKey("JWT"))
        {
            // Đã đăng nhập -> Chuyển sang màn hình Chọn Nhân Vật
            // (Nếu chỉ có 1 nhân vật mặc định, có thể vào thẳng GameScene, 
            // nhưng flow chuẩn là qua CharSelect)
            SceneManager.LoadScene("CharSelectScene");
        }
        else
        {
            // Chưa đăng nhập -> Chuyển sang màn hình Auth (Login/Register)
            SceneManager.LoadScene("AuthScene");
        }
    }
}