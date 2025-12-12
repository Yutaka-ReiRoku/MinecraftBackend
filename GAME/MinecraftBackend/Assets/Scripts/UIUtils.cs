using UnityEngine;
using UnityEngine.UIElements;

public static class UIUtils
{
    
    
    
    public static void FixTextFieldInput(this TextField textField)
    {
        if (textField == null) return;

        
        textField.RegisterCallback<ChangeEvent<string>>(evt => 
        {
            textField.MarkDirtyRepaint();
        });

        
        textField.RegisterCallback<FocusOutEvent>(evt => 
        {
            Input.compositionCursorPos = Vector2.zero;
        });
        
        
        
    }
}