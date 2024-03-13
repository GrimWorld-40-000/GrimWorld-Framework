using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace ColorPicker;

public class ColorEditTextFieldGroup
{
    //Buffers
    private string _hueBuffer;
    private string _saturationBuffer;
    private string _valueBuffer;
    private string _alphaBuffer;
    private string _redBuffer;
    private string _greenBuffer;
    private string _blueBuffer;
    private string _hexBuffer;
    
    //HSV
    private float _hue;
    private float _saturation;
    private float _value;
    
    //ARGB
    private Color _color;
    private float _alpha;
    private float _red;
    private float _green;
    private float _blue;

    public Color Color => new Color(_red, _green, _blue, _alpha);
    public (float H, float S, float V) HSV => (Hue, Saturation, Value);


    #region RGBA
    
    public float Red
    {
        get => _red;
        set
        {
            if(_red == value) return;
            Log.Message("Setting red to:" + value);
            _red = value;
            _redBuffer = value.ToString();
            RecalculateHSV();
        }
    }
    
    public float Green
    {
        get => _green;
        set
        {
            if(_green == value) return;
            _green = value;
            _greenBuffer = value.ToString();
            RecalculateHSV();
        }
    }
    
    public float Blue
    {
        get => _blue;
        set
        {
            if(_blue == value) return;
            _blue = value;
            _blueBuffer = value.ToString();
            RecalculateHSV();
        }
    }
    
    public float Alpha
    {
        get => _alpha;
        set
        {
            if(_alpha == value) return;
            _alpha = value;
            _alphaBuffer = value.ToString();
            RecalculateHSV();
        }
    }    
    
    public event Action<Color> ColorChanged;
    public event Action<(float, float, float)> HSVChanged;

    #endregion

    #region HSV
    
    public float Hue
    {
        get => _hue;
        set
        {
            if(_hue == value) return;
            Log.Message("Setting hue to:" + value);
            _hue = value;
            _hueBuffer = value.ToString();
            RecalculateArgb();
        }
    }
    
    public float Saturation
    {
        get => _saturation;
        set
        {
            if(_saturation == value) return;
            Log.Message("Setting saturation to:" + value);
            _saturation = value;
            _saturationBuffer = value.ToString();
            RecalculateArgb();
        }
    }
    
    public float Value
    {
        get => _value;
        set
        {
            if(_value == value) return;
            Log.Message("Setting HSV.Value to:" + value);
            _value = value;
            _valueBuffer = value.ToString();
            RecalculateArgb();
        }
    }

    #endregion
    
    //Validators
    private const string reg = @"^#(?:[0-9a-fA-F]{2}){1,2,4}$";
    private Regex _hexValidator = new Regex(reg);
    
    public ColorEditTextFieldGroup()
    {
        _color = Color.white;
    }

    public void SetColor(Color color)
    {
        Alpha = color.a;
        Red = color.r;
        Green = color.g;
        Blue = color.b;
    }

    public void Draw(Rect inRect)
    {
        var partWidth = inRect.width / 5f;
        var partHeight = (inRect.height / 3f) - 4; //2px padding

        var hsvRect = new Rect(inRect.x, inRect.y, partWidth, partHeight);
        var argbRect = new Rect(inRect.x, hsvRect.yMax + 4, partWidth, partHeight);
        var hexRect = new Rect(inRect.x, argbRect.yMax + 4, partWidth, partHeight);

        GUI.color = Color.grey;
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(hsvRect, "HSV");
        Widgets.Label(argbRect, "RGBA");
        Widgets.Label(hexRect, "Hex");
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        
        //HSV Fields
        var hue = Hue;
        var hueFieldRect = ValueTextField(hsvRect, partWidth, partHeight, "gw4k.colorpicker.hsv.h", ref hue, ref _hueBuffer);
        Hue = hue;

        var sat = Saturation;
        var saturationFieldRect = ValueTextField(hueFieldRect, partWidth, partHeight, "gw4k.colorpicker.hsv.s", ref sat, ref _saturationBuffer);
        Saturation = sat;
        
        var val = Value;
        var valueFieldRect = ValueTextField(saturationFieldRect, partWidth, partHeight, "gw4k.colorpicker.hsv.v", ref val, ref _valueBuffer);
        Value = val;

        //ARGB Fields
        //TODO: RGBA and HEX value doesnt change when editing field
        var r = Red;
        var redFieldRect = ValueTextField(argbRect, partWidth, partHeight, "gw4k.colorpicker.argb.r", ref r, ref _redBuffer);
        Red = r;
        
        var g = Green;
        var greenFieldRect = ValueTextField(redFieldRect, partWidth, partHeight, "gw4k.colorpicker.argb.g", ref g, ref _greenBuffer);
        Green = g;
        
        var b = Blue;
        var blueFieldRect = ValueTextField(greenFieldRect, partWidth, partHeight, "gw4k.colorpicker.argb.b", ref b, ref _blueBuffer);
        Blue = b;
        
        var a = Alpha;
        var alphaFieldRect = ValueTextField(blueFieldRect, partWidth, partHeight, "gw4k.colorpicker.argb.a", ref a, ref _alphaBuffer);
        Alpha = a;

        //Hex Field
        var hexFieldRect = new Rect(hexRect.xMax, hexRect.y, partWidth * 4, partHeight);
        var hexValue = _hexBuffer;
        var hexResult = Widgets.TextField(hexFieldRect, hexValue, 9, _hexValidator);
        _hexBuffer = hexResult;
    }

    private Rect ValueTextField(Rect refRect, float width, float height, string id, ref float value, ref string buffer)
    {
        var rect = new Rect(refRect.xMax, refRect.y, width, height);
        GUI.SetNextControlName(id);
        Widgets.TextFieldNumeric(rect, ref value, ref buffer, 0, 1);
        return rect;
    }
    
    private void RecalculateHSV()
    {
        //Set HSV
        Color.RGBToHSV(Color, out var hue, out var saturation, out var value);
        Hue = Mathf.Clamp(hue, 0, 1);
        Saturation = Mathf.Clamp(saturation, 0, 1);
        Value = Mathf.Clamp(value, 0, 1);
        
        //Set Hex
        _hexBuffer = ColorUtility.ToHtmlStringRGBA(Color);
        ColorChanged?.Invoke(Color);
    }

    private void RecalculateArgb()
    {
        //Set ARGB
        _color = Color.HSVToRGB(_hue, _saturation, _value);
        Red = Mathf.Clamp(_color.r, 0, 1);
        Green = Mathf.Clamp(_color.g, 0, 1);
        Blue = Mathf.Clamp(_color.b, 0, 1);
        Alpha = Mathf.Clamp(_color.a, 0, 1);
        
        //Set Hex
        _hexBuffer = ColorUtility.ToHtmlStringRGBA(Color);
        ColorChanged?.Invoke(Color);
    }
    
}