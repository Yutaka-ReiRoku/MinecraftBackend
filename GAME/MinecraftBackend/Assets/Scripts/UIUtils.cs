using UnityEngine;
using UnityEngine.UIElements;

public static class UIUtils
{
    /// <summary>
    /// Hàm mở rộng giúp sửa lỗi ArgumentOutOfRangeException khi nhập liệu trên Unity UI Toolkit.
    /// </summary>
    public static void FixTextFieldInput(this TextField textField)
    {
        if (textField == null) return;

        // 1. Ép buộc vẽ lại khi giá trị thay đổi để tránh lỗi render text sai vị trí
        textField.RegisterCallback<ChangeEvent<string>>(evt => 
        {
            textField.MarkDirtyRepaint();
        });

        // 2. Reset vị trí con trỏ bộ gõ khi mất focus (tránh kẹt IME)
        textField.RegisterCallback<FocusOutEvent>(evt => 
        {
            Input.compositionCursorPos = Vector2.zero;
        });
        
        // 3. (Tùy chọn) Chặn sự kiện sai lệch từ bộ gõ nếu cần thiết
        // textField.RegisterCallback<InputEvent>(evt => { ... });
    }
}