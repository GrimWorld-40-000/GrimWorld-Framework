using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

// Original Author: Epicguru
// Commissioned Edit: Telefonmast

namespace ColorPicker;

public class ColorPickerTool
{
    private Color _color;
    private ColorEditTextFieldGroup _textFields;

    //
    private Texture2D _colorPickerBG;
    private Texture2D _huePickerBG;
    private Texture2D _alphaPickerBG;
    private Texture2D _tempPreviewBG;
    
    public Color Color => _color;
    
    public Action<Color> OnColorChanged { get; set; }

    private const int PickerSize = 200;
    private const float UnitsPerPixel = 1f / PickerSize;

    public Color TempColor => _textFields.Color;
    public (float H, float S, float V) HSV => _textFields.HSV;
    
    public Texture2D ColorPickerBG
    {
        get
        {
            if (_colorPickerBG == null)
            {
                UpdateColorPickerBG();
            }
            return _colorPickerBG;
        }
    }
    
    public Texture2D HuePickerBG
    {
        get
        {
            if (_huePickerBG == null)
            {
                UpdateHuePickerBG();
            }
            return _huePickerBG;
        }
    }
    
    public Texture2D AlphaPickerBG
    {
        get
        {
            if (_alphaPickerBG == null)
            {
                UpdateAlphaPickerBG();
            }
            return _alphaPickerBG;
        }
    }

    private void UpdateColorPickerBG()
    {
        Object.Destroy(_colorPickerBG);
        _colorPickerBG = ColorUtils.CreateColourPickerBG(PickerSize, PickerSize, UnitsPerPixel, HSV.H, TempColor.a);
    }

    private void UpdateHuePickerBG()
    {
        Object.Destroy(_huePickerBG);
        _huePickerBG = ColorUtils.CreateHuePickerBG(PickerSize);
    }

    private void UpdateAlphaPickerBG()
    {
        Object.Destroy(_alphaPickerBG);
        _alphaPickerBG = ColorUtils.CreateAlphaPickerBG(PickerSize, TempColor);
    }
    
    public ColorPickerTool()
    {
        _color = Color.white;
        _textFields = new ColorEditTextFieldGroup();
        _textFields.SetColor(_color);
    }

    public Rect Draw(Rect inRect)
    {
        // Given values
        float _margin = 5;
        float _sliderWidth = 15;
        var initialSize = inRect.size;

        // Calculation
        //pickerSize + margin + sliderWidth + margin + sliderWidth + margin + previewSize

        var _pickerSize = (initialSize.x - _sliderWidth * 2 - _margin * 3) / 2f;
        var _previewSize = _pickerSize;

        var colorPickerRect = new Rect(inRect.x, inRect.y, _pickerSize, _pickerSize);
        var hueSliderRect = new Rect(colorPickerRect.xMax + _margin, colorPickerRect.y, _sliderWidth, _pickerSize);
        var alphaSliderRect = new Rect(hueSliderRect.xMax + _margin, colorPickerRect.y, _sliderWidth, _pickerSize);

        var rightRect = new Rect(alphaSliderRect.xMax + _margin, colorPickerRect.y, _previewSize, _previewSize);
        var previewRect = rightRect.TopHalf();
        var colorEditRect = rightRect.BottomHalf();
        colorEditRect = new Rect(colorEditRect.x, colorEditRect.y + 5, colorEditRect.width, colorEditRect.height - 5);

        //
        Widgets.DrawHighlight(colorPickerRect);
        GUI.DrawTexture(colorPickerRect, ColorPickerBG);
        
        Widgets.DrawHighlight(hueSliderRect);
        GUI.DrawTexture(hueSliderRect, HuePickerBG);
        
        Widgets.DrawHighlight(alphaSliderRect);
        GUI.DrawTexture(alphaSliderRect, AlphaPickerBG);
        
        Widgets.DrawHighlight(previewRect);

        _textFields.Draw(colorEditRect);

        return new Rect(inRect.position, new Vector2(inRect.width, colorPickerRect.yMax - inRect.y));
    }

    public void SetColorDirect(Color color)
    {
        
    }
}

public class DynamicColorSelection
{
    private Texture2D _pickAreaBG;
    private Texture2D _hueSliderBG;
    private Texture2D _alphaSliderBG;
    private Texture2D _previewColorTex;

    private void RegeneratePickArea()
    {
        
    }
}