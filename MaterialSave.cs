using PaintIn3D;
using System.IO;
using UnityEngine;

public class MaterialSaver : MonoBehaviour
{
    private P3dPaintableTexture savingTexture;
    [field: SerializeField] public string CurrentMaterial { get; set; }
    private string _materialsName;

    private void Start()
    {
        savingTexture = GetComponent<P3dPaintableTexture>();
    }

    public void SaveMaterial(string name)
    {
        //SaveBytes(savingTexture.GetPngData());
        _materialsName = name;

        byte[] byteArray = savingTexture.GetPngData();
        var assetPath = Application.persistentDataPath + "/Materials/" + _materialsName + ".png";
        Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
        File.WriteAllBytes(assetPath, byteArray);
    }

    public void LoadMaterial()
    {
        if (savingTexture.Activated == true)
        {
            savingTexture.LoadFromData(LoadBytes());
        }
    }

    private byte[] LoadBytes()
    {
        var base64 = CurrentMaterial;

        if (string.IsNullOrEmpty(base64) == false)
        {
            return System.Convert.FromBase64String(base64);
        }

        return null;
    }

    private void SaveBytes(byte[] data, bool save = true)
    {
        var base64 = default(string);

        if (data != null)
        {
            base64 = System.Convert.ToBase64String(data);
        }

        print(base64);
        CurrentMaterial = base64;

        if (save == true)
        {
            CurrentMaterial = base64;
        }
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    
    public Texture2D GetReadableCopy(bool convertBack = false)
    {
        var copy = default(Texture2D);

        if (savingTexture.Activated == true)
        {
            copy = P3dHelper.GetReadableCopy(savingTexture.Current);
        }
        else
        {
            var desc = new RenderTextureDescriptor(savingTexture.Width, savingTexture.Height, savingTexture.Format, 0);
            var temp = P3dHelper.GetRenderTexture(desc);
            var finalTexture = savingTexture.Texture;

            if (finalTexture == null && savingTexture.Existing != P3dPaintableTexture.ExistingType.Ignore)
            {
                UpdateMaterial();

                finalTexture = savingTexture.Material.GetTexture(savingTexture.Slot.Name);
            }

            P3dCommandReplace.Blit(temp, finalTexture, savingTexture.Color);

            copy = P3dHelper.GetReadableCopy(temp);

            P3dHelper.ReleaseRenderTexture(temp);
        }

        if (convertBack == true && savingTexture.Conversion == P3dPaintableTexture.ConversionType.Normal)
        {
            for (var y = 0; y < copy.height; y++)
            {
                for (var x = 0; x < copy.width; x++)
                {
                    var color = copy.GetPixel(x, y);

                    copy.SetPixel(x, y, new Color(color.r, color.g, color.b, 1.0f));
                }
            }

            copy.Apply();
        }

        return copy;
    }

    public void UpdateMaterial()
    {
        if (savingTexture.Paintable != null)
        {
            savingTexture.material = P3dHelper.GetMaterial(savingTexture.Paintable.CachedRenderer, savingTexture.Slot.Index);
            savingTexture.materialSet = true;
        }
        else
        {
            savingTexture.materialSet = false;
        }
    }

    public byte[] GetPngData(bool convertBack = false)
    {
        var tempTexture = GetReadableCopy(convertBack);

        if (tempTexture != null)
        {
            var data = tempTexture.EncodeToPNG();

            P3dHelper.Destroy(tempTexture);

            return data;
        }

        return null;
    }
}
