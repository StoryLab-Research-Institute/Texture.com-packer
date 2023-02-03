using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using TMPro;

public class TextureContainer : MonoBehaviour
{
    private static ExtensionFilter[] extensions = new ExtensionFilter[] { new("Accepted image files", "png", "jpg", "jpeg") };

    [SerializeField] private Button loadButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private RawImage thumbnail;
    [SerializeField] private TMP_Text filePath;

    //[SerializeField] private bool sRGB;

    [SerializeField] private Toggle ToggleR;
    [SerializeField] private Toggle ToggleG;
    [SerializeField] private Toggle ToggleB;

    public Texture2D Image { get; private set; }
    public TMP_Text FilePath { get => filePath; set => filePath = value; }

    private void Start()
    {
        loadButton.onClick.AddListener(OnLoadButtonPressed);
        removeButton.onClick.AddListener(Reset);
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
    }

    public TexturePacker.ColorChannel GetChannel()
    {
        if (ToggleR != null && ToggleR.isOn) return TexturePacker.ColorChannel.R;
        if (ToggleG != null && ToggleG.isOn) return TexturePacker.ColorChannel.G;
        if (ToggleB != null && ToggleB.isOn) return TexturePacker.ColorChannel.B;
        return TexturePacker.ColorChannel.A;
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