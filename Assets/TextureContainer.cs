using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using TMPro;
using System;

public class TextureContainer : MonoBehaviour
{
    private static ExtensionFilter[] extensions = new ExtensionFilter[] { new("Accepted image files", "png", "jpg", "jpeg") };

    [SerializeField] private Button loadButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private RawImage thumbnail;
    [SerializeField] private TMP_Text filePath;
    [SerializeField] private TMP_InputField defaultValueField;

    [SerializeField] private float defaultDefaultValue = 1f;

    //[SerializeField] private bool sRGB;

    [SerializeField] private GameObject ToggleGroup;
    [SerializeField] private Toggle ToggleR;
    [SerializeField] private Toggle ToggleG;
    [SerializeField] private Toggle ToggleB;

    public float DefaultValue { get; private set; } = 0f;
    public Texture2D Image { get; private set; }
    public TMP_Text FilePath { get => filePath; set => filePath = value; }

    private void Start()
    {
        loadButton.onClick.AddListener(OnLoadButtonPressed);
        removeButton.onClick.AddListener(Reset);
        defaultValueField.onEndEdit.AddListener(OnEmptyValueEdited);
        OnEmptyValueEdited(defaultDefaultValue.ToString());
    }

    private void OnEmptyValueEdited(string input)
    {
        if (float.TryParse(input, out float result)) DefaultValue = Mathf.Clamp01(result);
        else DefaultValue = 0f;

        defaultValueField.text = DefaultValue.ToString();
        UpdateUI();
    }

    public void OnLoadButtonPressed()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open " + gameObject.name + " File", "", extensions, false);
        if (paths[0] != null)
        {
            byte[] bytes = File.ReadAllBytes(paths[0]);
            Texture2D tex = new(2, 2);
            if (tex.LoadImage(bytes))
            {
                //if (!sRGB) DeSrgbTexture(tex);
                Image = tex;
                thumbnail.texture = tex;
                FilePath.text = paths[0];
                UpdateUI();
            }
            else Reset();
        }
        else Reset();
    }

    public void Reset()
    {
        Image = null;
        thumbnail.texture = null;
        FilePath.text = "No file selected";
        if (ToggleR != null) ToggleR.isOn = true;
        OnEmptyValueEdited(defaultDefaultValue.ToString());
    }

    public TexturePacker.ColorChannel GetChannel()
    {
        if (ToggleR != null && ToggleR.isOn) return TexturePacker.ColorChannel.R;
        if (ToggleG != null && ToggleG.isOn) return TexturePacker.ColorChannel.G;
        if (ToggleB != null && ToggleB.isOn) return TexturePacker.ColorChannel.B;
        return TexturePacker.ColorChannel.A;
    }

    private void UpdateUI()
    {
        bool texLoaded = Image != null;

        removeButton.gameObject.SetActive(texLoaded);
        if (ToggleGroup != null) ToggleGroup.SetActive(texLoaded);

        defaultValueField.gameObject.SetActive(!texLoaded);

        thumbnail.color = texLoaded ? Color.white : new Color(DefaultValue, DefaultValue, DefaultValue, 1f);
    }

    //private void DeSrgbTexture(Texture2D texture)
    //{
    //    Color[] colors = texture.GetPixels();
    //    for (int i = 0; i < colors.Length; i++)
    //    {
    //        colors[i].r = ToSRGB(colors[i].r);
    //        colors[i].g = ToSRGB(colors[i].g);
    //        colors[i].b = ToSRGB(colors[i].b);
    //    }
    //    texture.SetPixels(colors);
    //}

    //private float FromSRGB(float color)
    //{
    //    if (color <= 0.04045) return color / 12.92f; 
    //    else return Mathf.Pow((color + 0.055f) / 1.055f, 2.4f);
    //}

    //private float ToSRGB(float color)
    //{
    //    if (color <= 0.0031308) return 12.92f * color;
    //    else return (1.055f * Mathf.Pow(color, 0.4166667f)) - 0.055f;
    //}
}