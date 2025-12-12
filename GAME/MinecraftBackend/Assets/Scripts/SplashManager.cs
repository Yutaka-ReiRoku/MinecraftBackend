using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class SplashManager : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument SplashDoc;
    public float SplashDuration = 2.0f;

    private VisualElement _root;
    private Label _tipLabel;
    private VisualElement _background;

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
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    IEnumerator Start()
    {
        if (SplashDoc == null) SplashDoc = GetComponent<UIDocument>();
        if (SplashDoc != null)
        {
            _root = SplashDoc.rootVisualElement;
            _tipLabel = _root.Q<Label>("LoadingTipLabel");
            _background = _root.Q<VisualElement>("ScreenContainer");
        }
        if (_tipLabel != null)
        {
            _tipLabel.text = _tips[Random.Range(0, _tips.Length)];
        }
        yield return LoadCustomCursor();
        if (_background != null)
        {
            yield return _background.LoadBackgroundImage("/images/modes/adventure.png");
        }
        yield return new WaitForSeconds(SplashDuration);
        CheckLoginAndRedirect();
    }

    IEnumerator LoadCustomCursor()
    {
        string url = "http://localhost:5000/images/others/cursor.png"; 

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                UnityEngine.Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Debug.LogWarning("Không tải được Cursor, dùng mặc định.");
            }
        }
    }

    void CheckLoginAndRedirect()
    {
        if (PlayerPrefs.HasKey("JWT"))
        {
            SceneManager.LoadScene("CharSelectScene");
        }
        else
        {
            SceneManager.LoadScene("AuthScene");
        }
    }
}