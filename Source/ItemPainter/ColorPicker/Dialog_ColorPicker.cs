using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace ColorPicker
{
    [Obsolete]
    public class Dialog_ColorPicker : Window
    {
        private enum Controls
        {
            colourPicker,
            huePicker,
            alphaPicker,
            none
        }

        private Controls _activeControl = Controls.none;
        private Texture2D _colourPickerBG;
        private Texture2D _huePickerBG;
        private Texture2D _alphaPickerBG;
        private Texture2D _tempPreviewBG;
        private Texture2D _previewBG;
        private Texture2D _pickerAlphaBG;
        private Texture2D _sliderAlphaBG;
        private Texture2D _previewAlphaBG;

        private Color _alphaBGColorA = Color.white;
        private Color _alphaBGColorB = new Color(0.85f, 0.85f, 0.85f);

        private readonly int _pickerSize = 200;
        private readonly int _sliderWidth = 15;
        private int _alphaBGBlockSize = 10;
        private readonly int _previewSize = 90;
        private readonly int _handleSize = 10;
        private readonly int _recentSize = 20;
        private readonly float _margin = 6f;
        private readonly float _buttonHeight = 30f;
        private readonly float _fieldHeight = 24f;
        private float _huePosition;
        private float _alphaPosition;
        private float _unitsPerPixel;
        private float _h;
        private float _s;
        private float _v;

        private List<string> textFieldIds;

        public bool autoApply = false;
        public bool minimalistic = false;

        private TextField<float> RedField;
        private TextField<float> GreenField;
        private TextField<float> BlueField;
        private TextField<float> HueField;
        private TextField<float> SaturationField;
        private TextField<float> ValueField;
        private TextField<float> Alpha1Field;
        private TextField<float> Alpha2Field;
        private TextField<string> HexField;

        private string _hex;

        private RecentColors _recentColours = new RecentColors();

        private Vector2 _position = Vector2.zero;
        private Action<Color> _callback;
        public Color curColour;
        private Color _tempColour;
        private Vector2? _initialPosition;

        public string Hex
        {
            get => "#" + ColorUtility.ToHtmlStringRGBA(TempColour);
            set
            {
                _hex = value;
                NotifyHexUpdated();
            }
        }

        public Color TempColour
        {
            get => _tempColour;
            set
            {
                _tempColour = value;
                if (autoApply || minimalistic)
                    SetColor();
            }
        }

        public Vector2 InitialPosition => _initialPosition ??
                                          new Vector2(UI.screenWidth - InitialSize.x,
                                              UI.screenHeight - this.InitialSize.y) / 2f;

        public float UnitsPerPixel
        {
            get
            {
                if (_unitsPerPixel == 0f) _unitsPerPixel = 1f / _pickerSize;
                return _unitsPerPixel;
            }
        }

        public float H
        {
            get => _h;
            set
            {
                _h = Mathf.Clamp(value, 0f, 1f);
                NotifyHSVUpdated();
                CreateColourPickerBG();
                CreateAlphaPickerBG();
            }
        }

        public float S
        {
            get => _s;
            set
            {
                _s = Mathf.Clamp(value, 0f, 1f);
                NotifyHSVUpdated();
                CreateAlphaPickerBG();
            }
        }

        public float V
        {
            get => _v;
            set
            {
                _v = Mathf.Clamp(value, 0f, 1f);
                NotifyHSVUpdated();
                CreateAlphaPickerBG();
            }
        }

        public float A
        {
            get => TempColour.a;
            set
            {
                Color val = TempColour;
                val.a = Mathf.Clamp(value, 0f, 1f);
                TempColour = val;
                NotifyRGBUpdated();
            }
        }

        public float R
        {
            get => TempColour.r;
            set
            {
                Color val = TempColour;
                val.r = Mathf.Clamp(value, 0f, 1f);
                TempColour = val;
                NotifyRGBUpdated();
            }
        }

        public float G
        {
            get => TempColour.g;
            set
            {
                Color val = TempColour;
                val.g = Mathf.Clamp(value, 0f, 1f);
                TempColour = val;
                NotifyRGBUpdated();
            }
        }

        public float B
        {
            get => TempColour.b;
            set
            {
                Color val = TempColour;
                val.b = Mathf.Clamp(value, 0f, 1f);
                TempColour = val;
                NotifyRGBUpdated();
            }
        }

        public Texture2D ColourPickerBG
        {
            get
            {
                if (_colourPickerBG == null)
                {
                    CreateColourPickerBG();
                }

                return _colourPickerBG;
            }
        }

        public Texture2D HuePickerBG
        {
            get
            {
                if (_huePickerBG == null)
                {
                    CreateHuePickerBG();
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
                    CreateAlphaPickerBG();
                }

                return _alphaPickerBG;
            }
        }

        public Texture2D TempPreviewBG
        {
            get
            {
                if (_tempPreviewBG == null)
                {
                    CreatePreviewBG(ref _tempPreviewBG, TempColour);
                }

                return _tempPreviewBG;
            }
        }

        public Texture2D PreviewBG
        {
            get
            {
                if (_previewBG == null)
                {
                    CreatePreviewBG(ref _previewBG, curColour);
                }

                return _previewBG;
            }
        }

        public Texture2D PickerAlphaBG
        {
            get
            {
                if (_pickerAlphaBG == null)
                {
                    CreateAlphaBG(ref _pickerAlphaBG, _pickerSize, _pickerSize);
                }

                return _pickerAlphaBG;
            }
        }

        public Texture2D SliderAlphaBG
        {
            get
            {
                if (_sliderAlphaBG == null)
                {
                    CreateAlphaBG(ref _sliderAlphaBG, _sliderWidth, _pickerSize);
                }

                return _sliderAlphaBG;
            }
        }

        public Texture2D PreviewAlphaBG
        {
            get
            {
                if (_previewAlphaBG == null)
                {
                    CreateAlphaBG(ref _previewAlphaBG, _previewSize, _previewSize);
                }

                return _previewAlphaBG;
            }
        }

        public override Vector2 InitialSize =>
            new Vector2(_pickerSize + 3f * _margin + 2 * _sliderWidth + 2 * _previewSize + 36f, _pickerSize + 36f);

        public Dialog_ColorPicker(Color color, Action<Color> callback = null, Vector2? position = null)
        {
            base.absorbInputAroundWindow = true;
            base.closeOnClickedOutside = true;
            _callback = callback;
            _initialPosition = position;
            curColour = color;
            TempColour = color;
            HueField = TextField<float>.Float01(H, "Hue", delegate (float h) { H = h; });
            SaturationField = TextField<float>.Float01(S, "Saturation", delegate (float s) { S = s; });
            ValueField = TextField<float>.Float01(V, "Value", delegate (float v) { V = v; });
            Alpha1Field = TextField<float>.Float01(A, "Alpha1", delegate (float a) { A = a; });
            RedField = TextField<float>.Float01(color.r, "Red", delegate (float r) { R = r; });
            GreenField = TextField<float>.Float01(color.r, "Green", delegate (float g) { G = g; });
            BlueField = TextField<float>.Float01(color.r, "Blue", delegate (float b) { B = b; });
            Alpha2Field = TextField<float>.Float01(A, "Alpha2", delegate (float a) { A = a; });
            HexField = TextField<string>.Hex(Hex, "Hex", delegate (string hex) { Hex = hex; });
            textFieldIds = new List<string>(new string[9] { "Hue", "Saturation", "Value", "Alpha1", "Red", "Green", "Blue", "Alpha2", "Hex" });
            NotifyRGBUpdated();
        }

        protected override void SetInitialSizeAndPosition()
        {
            var val = new Vector2(Mathf.Min(InitialSize.x, UI.screenWidth),
                Mathf.Min(InitialSize.y, UI.screenHeight - 35f));
            var val2 = new Vector2(Mathf.Max(0f, Mathf.Min(InitialPosition.x, UI.screenWidth - val.x)),
                Mathf.Max(0f, Mathf.Min(InitialPosition.y, UI.screenHeight - val.y)));
            windowRect = new Rect(val2.x, val2.y, val.x, val.y);
        }

        public override void PreOpen()
        {
            ((Window)this).PreOpen();
            NotifyHSVUpdated();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var val = new Rect(inRect.xMin, inRect.yMin, _pickerSize, _pickerSize);
            var val2 = new Rect(val.xMax + _margin, inRect.yMin, _sliderWidth, _pickerSize);
            var val3 = new Rect(val2.xMax + _margin, inRect.yMin, _sliderWidth, _pickerSize);
            var val4 = new Rect(val3.xMax + _margin, inRect.yMin, _previewSize, _previewSize);
            var val5 = new Rect(val4.xMax, inRect.yMin, _previewSize, _previewSize);
            var val6 = new Rect(val3.xMax + _margin, inRect.yMax - _buttonHeight, _previewSize * 2, _buttonHeight);
            var val7 = new Rect(val3.xMax + _margin, inRect.yMax - 2f * _buttonHeight - _margin,
                _previewSize - _margin / 2f, _buttonHeight);
            var val8 = new Rect(val7.xMax + _margin, val7.yMin, _previewSize - _margin / 2f, _buttonHeight);
            var hsvFieldRect = new Rect(val3.xMax + _margin,
                inRect.yMax - 2f * _buttonHeight - 3f * _fieldHeight - 4f * _margin, _previewSize * 2, _fieldHeight);
            var rgbFieldRect = new Rect(val3.xMax + _margin,
                inRect.yMax - 2f * _buttonHeight - 2f * _fieldHeight - 3f * _margin, _previewSize * 2, _fieldHeight);
            var hexRect = new Rect(val3.xMax + _margin, inRect.yMax - 2f * _buttonHeight - _fieldHeight - 2f * _margin,
                _previewSize * 2, _fieldHeight);
            var val9 = new Rect(val4.xMin, val4.yMax + _margin, _previewSize * 2, _recentSize * 2);

            GUI.DrawTexture(val, PickerAlphaBG);
            GUI.DrawTexture(val3, SliderAlphaBG);
            GUI.DrawTexture(val4, PreviewAlphaBG);
            GUI.DrawTexture(val5, PreviewAlphaBG);
            GUI.DrawTexture(val, ColourPickerBG);
            GUI.DrawTexture(val2, HuePickerBG);
            GUI.DrawTexture(val3, AlphaPickerBG);
            GUI.DrawTexture(val4, TempPreviewBG);
            GUI.DrawTexture(val5, PreviewBG);
            if (Widgets.ButtonInvisible(val5))
            {
                TempColour = curColour;
                NotifyRGBUpdated();
            }

            var val10 = new Rect(val2.xMin - 3f, val2.yMin + _huePosition - _handleSize / 2f, _sliderWidth + 6f,
                _handleSize);
            var val11 = new Rect(val3.xMin - 3f, val3.yMin + _alphaPosition - _handleSize / 2f, _sliderWidth + 6f,
                _handleSize);
            var val12 = new Rect(val.xMin + _position.x - _handleSize / 2f,
                val.yMin + _position.y - _handleSize / 2f, _handleSize, _handleSize);
            GUI.DrawTexture(val10, TempPreviewBG);
            GUI.DrawTexture(val11, TempPreviewBG);
            GUI.DrawTexture(val12, TempPreviewBG);
            GUI.color = Color.gray;
            Widgets.DrawBox(val10);
            Widgets.DrawBox(val11);
            Widgets.DrawBox(val12);
            GUI.color = Color.white;
            if (Input.GetMouseButtonUp(0)) _activeControl = Controls.none;

            DrawColourPicker(val);
            DrawHuePicker(val2);
            DrawAlphaPicker(val3);
            DrawFields(hsvFieldRect, rgbFieldRect, hexRect);
            GUI.color = Color.white;
        }

        #region ColorPicker

        public void SetPickerPositions()
        {
            _huePosition = (1f - H) / UnitsPerPixel;
            _position.x = S / UnitsPerPixel;
            _position.y = (1f - V) / UnitsPerPixel;
            _alphaPosition = (1f - A) / UnitsPerPixel;
        }

        public void NotifyHSVUpdated()
        {
            //Debug($"HSV updated: ({_h}, {_s}, {_v})");
            Color val = Color.HSVToRGB(H, S, V);
            val.a = A;
            TempColour = val;
            CreatePreviewBG(ref _tempPreviewBG, TempColour);
            SetPickerPositions();
            RedField.Value = TempColour.r;
            GreenField.Value = TempColour.g;
            BlueField.Value = TempColour.b;
            HueField.Value = H;
            SaturationField.Value = S;
            ValueField.Value = V;
            Alpha1Field.Value = A;
            Alpha2Field.Value = A;
            HexField.Value = Hex;
        }

        public void NotifyRGBUpdated()
        {
            //Debug($"RGB updated: ({R}, {G}, {B})");
            Color.RGBToHSV(TempColour, out _h, out _s, out _v);
            CreateColourPickerBG();
            CreateHuePickerBG();
            CreateAlphaPickerBG();
            CreatePreviewBG(ref _tempPreviewBG, TempColour);
            SetPickerPositions();
            HueField.Value = H;
            SaturationField.Value = S;
            ValueField.Value = V;
            Alpha1Field.Value = A;
            Alpha2Field.Value = A;
            HexField.Value = Hex;
        }

        public void NotifyHexUpdated()
        {
            //Debug("HEX updated (" + Hex + ")");
            if (ColorUtility.TryParseHtmlString(_hex, out var col))
            {
                TempColour = col;
                NotifyRGBUpdated();
                RedField.Value = TempColour.r;
                GreenField.Value = TempColour.g;
                BlueField.Value = TempColour.b;
            }
        }

        public void SetColor()
        {
            curColour = TempColour;
            _recentColours.Add(TempColour);
            _callback?.Invoke(curColour);
            CreatePreviewBG(ref _previewBG, TempColour);
        }

        private void SwapTexture(ref Texture2D tex, Texture2D newTex)
        {
            Object.Destroy(tex);
            tex = newTex;
        }

        private void CreateColourPickerBG()
        {
            var unitsPerPixel = UnitsPerPixel;
            var unitsPerPixel2 = UnitsPerPixel;
            var val = new Texture2D(_pickerSize, _pickerSize);
            for (var x = 0; x < _pickerSize; x++)
            {
                for (var y = 0; y < _pickerSize; y++)
                {
                    var s = x * unitsPerPixel;
                    var v = y * unitsPerPixel2;
                    val.SetPixel(x, y, HSVAToRGB(H, s, v, A));
                }
            }

            val.Apply();
            SwapTexture(ref _colourPickerBG, val);
        }

        private void CreateHuePickerBG()
        {
            Texture2D val = new Texture2D(1, _pickerSize);
            int pickerSize = _pickerSize;
            float num = 1f / (float)pickerSize;
            for (int i = 0; i < pickerSize; i++)
            {
                val.SetPixel(0, i, Color.HSVToRGB(num * (float)i, 1f, 1f));
            }

            val.Apply();
            SwapTexture(ref _huePickerBG, val);
        }

        private void CreateAlphaPickerBG()
        {
            Texture2D val = new Texture2D(1, _pickerSize);
            int pickerSize = _pickerSize;
            float num = 1f / (float)pickerSize;
            for (int i = 0; i < pickerSize; i++)
            {
                val.SetPixel(0, i, new Color(TempColour.r, TempColour.g, TempColour.b, (float)i * num));
            }

            val.Apply();
            SwapTexture(ref _alphaPickerBG, val);
        }

        private void CreateAlphaBG(ref Texture2D bg, int width, int height)
        {
            Texture2D val = new Texture2D(width, height);
            Color[] array = (Color[])(object)new Color[_alphaBGBlockSize * _alphaBGBlockSize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _alphaBGColorA;
            }

            Color[] array2 = (Color[])(object)new Color[_alphaBGBlockSize * _alphaBGBlockSize];
            for (int j = 0; j < array2.Length; j++)
            {
                array2[j] = _alphaBGColorB;
            }

            int num = 0;
            for (int k = 0; k < width; k += _alphaBGBlockSize)
            {
                int num2 = num;
                for (int l = 0; l < height; l += _alphaBGBlockSize)
                {
                    val.SetPixels(k, l, _alphaBGBlockSize, _alphaBGBlockSize, (num2 % 2 == 0) ? array : array2);
                    num2++;
                }

                num++;
            }

            val.Apply();
            SwapTexture(ref bg, val);
        }

        public void CreatePreviewBG(ref Texture2D bg, Color col)
        {
            SwapTexture(ref bg, SolidColorMaterials.NewSolidColorTexture(col));
        }

        public void PickerAction(Vector2 pos)
        {
            _s = UnitsPerPixel * pos.x;
            _v = 1f - UnitsPerPixel * pos.y;
            CreateAlphaPickerBG();
            NotifyHSVUpdated();
            _position = pos;
        }

        public void HueAction(float pos)
        {
            H = 1f - UnitsPerPixel * pos;
            _huePosition = pos;
        }

        public void AlphaAction(float pos)
        {
            A = 1f - UnitsPerPixel * pos;
            _alphaPosition = pos;
        }

        #endregion

        private void DrawRecent(Rect canvas)
        {
            var num = (int)(canvas.width / _recentSize);
            var num2 = (int)(canvas.height / _recentSize);
            var num3 = Math.Min(num * num2, _recentColours.Count);
            GUI.BeginGroup(canvas);
            var val2 = default(Rect);
            for (var i = 0; i < num3; i++)
            {
                var num4 = i % num;
                var num5 = i / num;
                var val = _recentColours[i];
                val2 = new Rect(num4 * _recentSize, num5 * _recentSize, _recentSize,
                    _recentSize);
                Widgets.DrawBoxSolid(val2, val);
                if (Mouse.IsOver(val2)) Widgets.DrawBox(val2);

                if (Widgets.ButtonInvisible(val2))
                {
                    TempColour = val;
                    NotifyRGBUpdated();
                }
            }

            GUI.EndGroup();
        }

        private void DrawAlphaPicker(Rect alphaRect)
        {
            if (Mouse.IsOver(alphaRect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _activeControl = Controls.alphaPicker;
                }

                if ((int)Event.current.type == 6)
                {
                    A -= Event.current.delta.y * UnitsPerPixel;
                    _alphaPosition = Mathf.Clamp(_alphaPosition + Event.current.delta.y, 0f, (float)_pickerSize);
                    Event.current.Use();
                }

                if (_activeControl == Controls.alphaPicker)
                {
                    float y = Event.current.mousePosition.y;
                    float pos = y - alphaRect.yMin;
                    AlphaAction(pos);
                }
            }
        }

        private void DrawHuePicker(Rect hueRect)
        {
            if (Mouse.IsOver(hueRect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _activeControl = Controls.huePicker;
                }

                if ((int)Event.current.type == 6)
                {
                    H -= Event.current.delta.y * UnitsPerPixel;
                    _huePosition = Mathf.Clamp(_huePosition + Event.current.delta.y, 0f, (float)_pickerSize);
                    Event.current.Use();
                }

                if (_activeControl == Controls.huePicker)
                {
                    float y = Event.current.mousePosition.y;
                    float pos = y - hueRect.yMin;
                    HueAction(pos);
                }
            }
        }

        private void DrawColourPicker(Rect pickerRect)
        {
            if (Mouse.IsOver(pickerRect))
            {
                if (Input.GetMouseButtonDown(0))
                    _activeControl = Controls.colourPicker;

                if (_activeControl == Controls.colourPicker)
                {
                    var mousePosition = Event.current.mousePosition;
                    var pos = mousePosition - new Vector2(pickerRect.xMin, pickerRect.yMin);
                    PickerAction(pos);
                }
            }
        }

        private void DrawButtons(Rect doneRect, Rect setRect, Rect cancelRect)
        {
            if (Widgets.ButtonText(doneRect, "OK"))
            {
                SetColor();
                Close();
            }

            if (Widgets.ButtonText(setRect, "Apply"))
                SetColor();

            if (Widgets.ButtonText(cancelRect, "Cancel"))
                Close();
        }

        private void DrawFields(Rect hsvFieldRect, Rect rgbFieldRect, Rect hexRect)
        {
            Text.Font = GameFont.Small;
            var val = hsvFieldRect;
            val.width = val.width / 5f;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.grey;
            Widgets.Label(val, "HSV");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            val.x = val.x + val.width;
            HueField.Draw(val);
            val.x = val.x + val.width;
            SaturationField.Draw(val);
            val.x = val.x + val.width;
            ValueField.Draw(val);
            val.x = val.x + val.width;
            Alpha1Field.Draw(val);

            val = rgbFieldRect;
            val.width = val.width / 5f;
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(val, "RGB");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            val.x = val.x + val.width;
            RedField.Draw(val);
            val.x = val.x + val.width;
            GreenField.Draw(val);
            val.x = val.x + val.width;
            BlueField.Draw(val);
            val.x = val.x + val.width;
            Alpha2Field.Draw(val);

            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(new Rect(hexRect.xMin, hexRect.yMin, val.width, hexRect.height), "HEX");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            hexRect.xMin = hexRect.xMin + val.width;
            HexField.Draw(hexRect);

            Text.Anchor = TextAnchor.UpperLeft;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                var nameOfFocusedControl = GUI.GetNameOfFocusedControl();
                var num = textFieldIds.IndexOf(nameOfFocusedControl);
                GUI.FocusControl(
                    textFieldIds[GenMath.PositiveMod(num + (Event.current.shift ? -1 : 1), textFieldIds.Count)]);
            }
        }

        public static Color HSVAToRGB(float H, float S, float V, float A)
        {
            Color result = Color.HSVToRGB(H, S, V);
            result.a = A;
            return result;
        }

        public override void OnAcceptKeyPressed()
        {
            OnAcceptKeyPressed();
            SetColor();
        }
    }
}