using UnityEngine;

namespace ColorPicker;

public static class ColorUtils
{
    public static Texture2D CreateColourPickerBG(int height, int width, float pixelUnits, float Hue, float Alpha)
    {
        var texture2D = new Texture2D(width, height);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var s = x * pixelUnits;
            var v = y * pixelUnits;
            texture2D.SetPixel(x, y, HSVAToRGB(Hue, s, v, Alpha));
        }

        texture2D.Apply();
        return texture2D;
    }

    public static Texture2D CreateHuePickerBG(int height)
    {
        var texture2D = new Texture2D(1, height);
        var num = 1f / height;
        for (var i = 0; i < height; i++)
        {
            texture2D.SetPixel(0, i, Color.HSVToRGB(num * i, 1f, 1f));
        }
        texture2D.Apply();
        return texture2D;
    }

    public static Texture2D CreateAlphaPickerBG(int height, Color color)
    {
        var texture2D = new Texture2D(1, height);
        var num = 1f / height;
        for (var i = 0; i < height; i++)
        {
            texture2D.SetPixel(0, i, new Color(color.r, color.g, color.b, i * num));
        }
        texture2D.Apply();
        return texture2D;
    }
    
    /*private void CreateAlphaBG(ref Texture2D bg, int width, int height)
    {
        Texture2D texture2D = new Texture2D(width, height);
        Color[] array = new Color[this._alphaBGBlockSize * this._alphaBGBlockSize];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = this._alphaBGColorA;
        }
        Color[] array2 = new Color[this._alphaBGBlockSize * this._alphaBGBlockSize];
        for (int j = 0; j < array2.Length; j++)
        {
            array2[j] = this._alphaBGColorB;
        }
        int num = 0;
        for (int k = 0; k < width; k += this._alphaBGBlockSize)
        {
            int num2 = num;
            for (int l = 0; l < height; l += this._alphaBGBlockSize)
            {
                texture2D.SetPixels(k, l, this._alphaBGBlockSize, this._alphaBGBlockSize, (num2 % 2 == 0) ? array : array2);
                num2++;
            }
            num++;
        }
        texture2D.Apply();
        this.SwapTexture(ref bg, texture2D);
    }*/
    
    public static Color HSVAToRGB(float H, float S, float V, float A)
    {
        var result = Color.HSVToRGB(H, S, V);
        result.a = A;
        return result;
    }
}