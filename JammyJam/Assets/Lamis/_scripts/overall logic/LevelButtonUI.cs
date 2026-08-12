using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class LevelButtonUI : MonoBehaviour
{
    [Header("Direct UI References (Drag & Drop from Prefab)")]
    [Tooltip("The GameObject containing your Text or TextMeshPro")]
    public GameObject labelObject;

    [Tooltip("The GameObject containing your Lock Image")]
    public GameObject lockIconObject;

    [Tooltip("Drag Star1, Star2, and Star3 Image components here")]
    public Image[] starImages;

    [Tooltip("The Image component on the Root of this prefab (for tinting)")]
    public Image backgroundImage;

    public void SetLabel(string value)
    {
        if (labelObject == null)
            return;

        // Try normal Unity UI Text first
        Text uiText = labelObject.GetComponent<Text>();

        if (uiText != null)
        {
            uiText.text = value;
            uiText.enabled = true;
            return;
        }

        // If using TextMeshPro, find it by component type name
        foreach (Component comp in labelObject.GetComponents<Component>())
        {
            if (comp == null)
                continue;

            string typeName = comp.GetType().Name;

            if (typeName == "TextMeshProUGUI" || typeName == "TextMeshPro")
            {
                PropertyInfo textProperty = comp.GetType().GetProperty("text");

                if (textProperty != null && textProperty.CanWrite)
                {
                    textProperty.SetValue(comp, value);

                    if (comp is Behaviour behaviour)
                        behaviour.enabled = true;

                    return;
                }
            }
        }
    }
}