using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TexturePacker : MonoBehaviour
{
    public TextureContainer baseColorContainer;
    public TextureContainer transparencyContainer;
    public TextureContainer metallicContainer;
    public TextureContainer aoContainer;
    public TextureContainer roughnessContainer;
    public TextureContainer normalContainer;

    public enum ColorChannel
    {
        R,
        G,
        B,
        A
    }

    private bool DoMOS => metallicContainer.Image != null || aoContainer.Image != null || roughnessContainer.Image != null;

    public TMPro.TMP_Text messageText;

    public Button packButton;
    public Button resetButton;

    private void Start()
    {
        packButton.onClick.AddListener(OnPackButtonPressed);
        resetButton.onClick.AddListener(OnResetButtonPressed);
    }

    private void OnResetButtonPressed()
    {
        messageText.text = "";
        baseColorContainer.Reset();
        transparencyContainer.Reset();
        metallicContainer.Reset();
        roughnessContainer.Reset();
        aoContainer.Reset();
    }

    private void OnPackButtonPressed()
    {
        messageText.text = DoPack();
    }

    private string DoPack()
    {
        // validate
        bool doAT = baseColorContainer.Image != null || transparencyContainer.Image != null;
        if (doAT && TexturesExistAndSizesDoNotMatch(baseColorContainer.Image, transparencyContainer.Image)) return "BaseColor and Transparency dimensions must match!";

        bool doMOS = metallicContainer.Image != null || aoContainer.Image != null || roughnessContainer.Image != null;
        if (doMOS && (TexturesExistAndSizesDoNotMatch(metallicContainer.Image, aoContainer.Image)
            || TexturesExistAndSizesDoNotMatch(aoContainer.Image, roughnessContainer.Image)
            || TexturesExistAndSizesDoNotMatch(roughnessContainer.Image, metallicContainer.Image))) return "Metallic, AO and Roughness dimensions must match!";

        bool doN = normalContainer.Image != null;

        if (!doAT && !doMOS && !doN)
        {
            return "No images selected!";
        }

        // get the output path
        var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "File Name Here", "");

        // do the albedo transparency image
        if (doAT)
        {
            byte[] atBytes;

            int width;
            if (baseColorContainer.Image != null) width = baseColorContainer.Image.width;
            else width = transparencyContainer.Image.width;

            int height;
            if (baseColorContainer.Image != null) height = baseColorContainer.Image.height;
            else height = transparencyContainer.Image.height;

            Color32[] atColors = new Color32[width * height];

            Color32[] baseColorColors = GetPixels32(baseColorContainer, width, height);
            Color32[] transparencyColors = GetPixels32(transparencyContainer, width, height);

            ColorChannel transparencyChannel = transparencyContainer.GetChannel();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int t = (y * width) + x;

                    atColors[t] = new Color32(baseColorColors[t].r, baseColorColors[t].g, baseColorColors[t].b, ByteFromColor32Channel(transparencyColors[t], transparencyChannel));
                }
            }

            Texture2D atTex = new(width, height);
            atTex.SetPixels32(atColors);
            atBytes = atTex.EncodeToPNG();

            File.WriteAllBytes(path + "_AlbedoTransparency.png", atBytes);
        }

        // do the metallic-occlusion-smoothness image
        if (DoMOS)
        {
            byte[] mosBytes;

            int width;
            if (metallicContainer.Image != null) width = metallicContainer.Image.width;
            else if (aoContainer.Image != null) width = aoContainer.Image.width;
            else width = roughnessContainer.Image.width;

            int height;
            if (metallicContainer.Image != null) height = metallicContainer.Image.height;
            else if (aoContainer.Image != null) height = aoContainer.Image.height;
            else height = roughnessContainer.Image.height;

            Color32[] metallicColors = GetPixels32(metallicContainer, width, height);
            Color32[] aoColors = GetPixels32(aoContainer, width, height);
            Color32[] roughnessColors = GetPixels32(roughnessContainer, width, height);

            ColorChannel metallicChannel = metallicContainer.GetChannel();
            ColorChannel aoChannel = aoContainer.GetChannel();
            ColorChannel roughnessChannel = aoContainer.GetChannel();

            Color32[] mosColors = new Color32[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int t = (y * width) + x;

                    mosColors[t] = new Color32(
                        ByteFromColor32Channel(metallicColors[t], metallicChannel),
                        ByteFromColor32Channel(aoColors[t], aoChannel),
                        0,
                        (byte)(255 - ByteFromColor32Channel(roughnessColors[t], roughnessChannel))
                        );
                }
            }

            Texture2D mosTex = new(width, height);
            mosTex.SetPixels32(mosColors);
            mosBytes = mosTex.EncodeToPNG();

            File.WriteAllBytes(path + "_MetallicOcclusionSmoothness.png", mosBytes);
        }

        //do the normals image (literally just renaming and moving this one)
        if(doN)
        {
            File.WriteAllBytes(path + "_Normal.png", normalContainer.Image.EncodeToPNG());
        }

        return "Pack complete!";
    }

    private byte ByteFromColor32Channel(Color32 color, ColorChannel channel)
    {
        return channel switch
        {
            ColorChannel.R => color.r,
            ColorChannel.G => color.g,
            ColorChannel.B => color.b,
            _ => color.a,
        };
    }

    private bool TexturesExistAndSizesDoNotMatch(Texture2D texA, Texture2D texB)
    {
        if (texA == null || texB == null) return false;
        return texA.width != texB.width || texA.height != texB.height;
    }

    private Color32[] GetPixels32(TextureContainer container, int expectedWidth, int expectedHeight)
    {
        if (container.Image != null) return container.Image.GetPixels32();

        Color32[] cols = new Color32[expectedWidth * expectedHeight];
        for (int i = 0; i < cols.Length; i++)
        {
            byte val = (byte)Mathf.RoundToInt(container.DefaultValue * 255);
            cols[i] = new Color32(val, val, val, val);
        }
        return cols;
    }
}