using UnityEngine;
using TMPro;

public class TextWaveAnimation : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 8f;
    [SerializeField] private float waveHeight = 0.15f;
    [SerializeField] private float waveFrequency = 0.5f;

    private TMP_Text textComponent;
    private TMP_TextInfo textInfo;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

            float offsetY = Mathf.Sin(Time.time * waveSpeed + i * waveFrequency) * waveHeight;
            Vector3 offset = new Vector3(0, offsetY, 0);

            // Shift position vertices
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;

            // Preserve alpha tint from textComponent.color during fade out
            byte currentAlpha = (byte)(textComponent.color.a * 255);
            vertexColors[vertexIndex + 0].a = currentAlpha;
            vertexColors[vertexIndex + 1].a = currentAlpha;
            vertexColors[vertexIndex + 2].a = currentAlpha;
            vertexColors[vertexIndex + 3].a = currentAlpha;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}