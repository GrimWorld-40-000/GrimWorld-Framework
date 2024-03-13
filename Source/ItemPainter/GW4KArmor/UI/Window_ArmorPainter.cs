namespace GW4KArmor.UI;

public enum PaletteTab
{
    Unique,
    Global
}

/*public class Window_ArmorPainter : Window
{
    private Pawn _pawn;
    private ThingWithComps _item;
    private ThingComp_Paintable _comp;

    private ColorPicker.ColorPicker _simpleColorPicker;

    private List<TabRecord> _paletteTabs;
    
    //Fixed
    private const float Pallete_Option_Size = 65;
    
    //Scrollers & Dynamics
    private Vector2 _paletteScroller;
    private Vector2 _patternScroller;
    private float? _dynamicRectColorPicker;
    
    //Material / Texture
    private Texture2D _texture;
    private Material _material;
    private Color[] _colors = new Color[3];
    
    private Rot4 _curRot = Rot4.North;
    private int _curMaskIndex;
    
    private PaletteTab _paletteTab = PaletteTab.Unique;
    
    public override Vector2 InitialSize
    {
        get
        {
            var size = new Vector2(1500f, 800f);
            return size;
        }
    }
    
    
    public Window_ArmorPainter(ThingComp_Paintable comp)
    {
        _comp = comp;
        _item = comp.parent;
        _pawn = comp.ParentHolder as Pawn;
        
        _simpleColorPicker = new ColorPicker.ColorPicker();
        _paletteTabs = new List<TabRecord>()
        {
            new TabRecord("Unique", () =>
            {
                Log.Message("Clicked Unique");
                _paletteTab = PaletteTab.Unique;
            }, () => _paletteTab == PaletteTab.Unique),
            new TabRecord("Global", () =>
            {
                Log.Message("Clicked Global");
                _paletteTab = PaletteTab.Global;
            }, () => _paletteTab == PaletteTab.Global),
        };
    }

    public override void PreOpen()
    {
        base.PreOpen();
        _material ??= new Material(GW4KContent.MaskMaterial);
    }

    public override void PostClose()
    {
        base.PostClose();
        if (_material != null)
        {
            Object.Destroy(_material);
            _material = null;
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        //Layout
        var leftPart = inRect.LeftPartPixels(256);
        var rightPart = inRect.RightPartPixels(384);
        var center = new Rect(leftPart.xMax, leftPart.y, rightPart.x - leftPart.xMax, leftPart.height);
        
        //
        ItemPreview(center);
        
        //Pattern selection
        DrawPatternSelection(leftPart);
        
        //Color Group
        var pixels = _dynamicRectColorPicker ?? 384;
        var colorPickerRect = rightPart.TopPartPixels(pixels).Rounded();
        var paletteRect = rightPart.BottomPartPixels((rightPart.height - pixels) - 10).Rounded();
        
        var res = TriColorPicker(colorPickerRect);
        PaletteSelection(paletteRect);
        
        _dynamicRectColorPicker ??= res.height + 10;
    }
    
    /// <summary>
    /// When opened on a pawn, full equipment selection is available. (to choose between painted part)
    /// </summary>
    private void EquippedSelection(Rect inRect)
    {
        
    }
    
    private Rect TriColorPicker(Rect inRect)
    {
        Widgets.DrawMenuSection(inRect);
        
        return _simpleColorPicker.Draw(inRect.ContractedBy(5));
    }

    private static List<Palette> testPalets = new List<Palette>()
    {
        new Palette("Test Palette", Color.red, Color.green, Color.blue),
        new Palette("Test Palette 2", Color.cyan, Color.magenta, Color.cyan),
    };

    private void PaletteSelection(Rect inRect)
    {
        //Palette Data
        Widgets.DrawMenuSection(inRect);
        
        //Title
        DrawTitle(inRect, "Palette Selection", out var topRect, false);
        Rect tabRect = new Rect(topRect.x, topRect.yMax+topRect.height, topRect.width, 5);
        Rect scrollRect = inRect.BottomPartPixels(inRect.yMax - tabRect.y+5).ContractedBy(5);
        TabDrawer.DrawTabs(tabRect, _paletteTabs);
        
        switch (_paletteTab)
        {
            case PaletteTab.Unique:
                DrawPaletteGroup(scrollRect, testPalets);
                break;
            case PaletteTab.Global:
                break;
        }
    }

    private void DrawPaletteGroup(Rect inRect, List<Palette> palettes)
    {
        var scrollableRect = new Rect(inRect.position, new Vector2(inRect.width, palettes.Count * Pallete_Option_Size));
        
        //Guide-Lines
        GUI.color = Widgets.MenuSectionBGBorderColor;
        Widgets.DrawLineHorizontal(inRect.x-5, inRect.y, inRect.width+10);
        Widgets.DrawLineVertical(inRect.x, inRect.y, inRect.height);
        Widgets.DrawLineVertical(inRect.xMax, inRect.y, inRect.height);
        Widgets.DrawLineHorizontal(inRect.x-5, inRect.yMax, inRect.width+10);
        GUI.color = Color.white;
  
        //Scroll
        Widgets.BeginScrollView(inRect, ref _paletteScroller, scrollableRect, false);
        
        for(int i = 0; i < palettes.Count; i++)
        {
            var rect = new Rect(inRect.x, inRect.y + (i * Pallete_Option_Size), inRect.width, Pallete_Option_Size);
            DrawPaletteRow(rect, i, palettes[i]);
        }
        
        Widgets.EndScrollView();
    }

    private void DrawPaletteRow(Rect rect, int i, Palette palette)
    {
        Text.Font = GameFont.Small;
        var size = Text.CalcSize(palette.Label);
        var titleRect = rect.TopPartPixels(size.y);
        titleRect.x += 5;

        var paletteRect = rect.BottomPartPixels(rect.height - size.y).ContractedBy(5).Rounded();
        var iconRect = paletteRect.LeftPartPixels(paletteRect.height).Rounded();
        var paletteDataRect = paletteRect.RightPartPixels(paletteRect.width - iconRect.width - 10).Rounded();
        var labelsRect = paletteDataRect.TopHalf().Rounded();
        var colorsRect = paletteDataRect.BottomHalf().Rounded();
        
        Widgets.DrawHighlightIfMouseover(rect);
        if(i % 2 == 0)
            Widgets.DrawHighlight(rect);
        Widgets.Label(titleRect, palette.Label);
        
        //Show colored texture
        Widgets.DrawHighlight(iconRect);
        //Widgets.DrawTextureFitted();

        var colorARect = paletteDataRect.LeftPartPixels(100);
        var colorBRect = paletteDataRect.LeftPartPixels(200).RightPartPixels(100);
        var colorCRect = paletteDataRect.LeftPartPixels(300).RightPartPixels(100);
        
        GUI.color = Color.white;
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(colorARect.TopHalf(), "Color A");
        Widgets.Label(colorBRect.TopHalf(), "Color B");
        Widgets.Label(colorCRect.TopHalf(), "Color C");
        Widgets.DrawBoxSolid(colorARect.BottomHalf(), palette.ColorA);
        Widgets.DrawBoxSolid(colorBRect.BottomHalf(), palette.ColorB);
        Widgets.DrawBoxSolid(colorCRect.BottomHalf(), palette.ColorC);
        
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
    }

    private void ItemPreview(Rect inRect)
    {
        var mainTexture = _item.Graphic.MatAt(_curRot, _item).mainTexture;
        var maskVariantID = GetCurrentVariantID();
        var maskTexture = _comp.Props.Masks.GetTexture(maskVariantID);
        var texCoords = new Rect(0f, 0f, 1f, 1f); //TODO? Rotation adjust?: previewRotation == 1 ? new Rect(1f, 0f, -1f, 1f) : new Rect(0f, 0f, 1f, 1f);
        _material.SetColor("_Color", _colors[0]);
        _material.SetColor("_ColorTwo", _colors[1]);
        _material.SetColor("_ColorThree", _colors[2]);
        _material.SetTexture("_MaskTex", maskTexture);

        var textureRect = inRect.ContractedBy(64);
        GUI.BeginGroup(textureRect);
        Graphics.DrawTexture(new Rect(0f, 0f, textureRect.width, textureRect.height), mainTexture, texCoords, 0, 0, 0, 0, _material);
        GUI.EndGroup();
        
        //Rotation Controls
    }

    private VariantID GetCurrentVariantID()
    {
        return new VariantID
        {
            BodyType = CurrentBodyType(),
            Index = _curMaskIndex,
            Rotation = _curRot
        };
    }

    private BodyTypeDef CurrentBodyType()
    {
        if (!MaskUtils.NeedsBodyType(_item)) return null;
        return MaskUtils.BodyTypDef(_comp.ParentHolder);
    }

    #region Masked Patterns

    private List<Texture2D> MaskTextures => null; //TODO
    
    private IEnumerable<Texture2D> MaskTexturesEnum
    {
        get
        {
            //TODO: Collect them from def.modextension.customCache
            yield break;
        }
    }

    #endregion
    
    private void DrawPatternSelection(Rect inRect)
    {
        Widgets.DrawMenuSection(inRect);
        DrawTitle(inRect, "Pattern Selection", out var topRect);
        
        //Rect tabRect = new Rect(topRect.x, topRect.yMax+topRect.height, topRect.width, 5);
        /*
        Rect scrollRect = inRect.BottomPartPixels(inRect.yMax - topRect.y+5).ContractedBy(5);
        var scrollableRect = new Rect(inRect.position, new Vector2(inRect.width, palettes.Count * Pallete_Option_Size));
        
        //
        Widgets.BeginScrollView(scrollRect, ref _patternScroller,
            scrollableRect, true);
        var previewMainTexture = GetPreviewMainTexture(Rot4.South);
        var num2 = 0f;
        var num3 = rect.width - 20f - (num * 120f - 20f);
        var num4 = -1;
        foreach (var value in MaskTextures)
        {
            num4++;
            var num5 = num4 % num;
            var num6 = num4 / num;
            var x = num3 * 0.5f + num5 * 120f;
            var num7 = num6 * 120f + 20f;
            var rect2 = new Rect(x, num7, 100f, 100f);
            var flag = num4 == previewMaskIndex;
            var flag2 = flag;
            if (flag2)
            {
                GUI.color = Color.yellow;
                Widgets.DrawBox(rect2.ExpandedBy(4f, 4f), 2);
                GUI.color = Color.white;
            }

            Material material = GWMaterialPool.Get();
            tempMaterials.Add(material);
            material.SetColor("_Color", colors[0]);
            material.SetColor("_ColorTwo", colors[1]);
            material.SetColor("_ColorThree", colors[2]);
            material.SetTexture("_MaskTex", value);
            GUI.BeginGroup(rect2);
            UnityEngine.Graphics.DrawTexture(new Rect(0f, 0f, rect2.width, rect2.height), previewMainTexture,
                new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, material);
            GUI.EndGroup();
            var flag3 = Mouse.IsOver(rect2);
            if (flag3)
            {
                GUI.color = Color.white * 0.7f;
                Widgets.DrawBox(rect2.ExpandedBy(4f, 4f));
                GUI.color = Color.white;
            }

            var flag4 = Widgets.ButtonInvisible(rect2);
            if (flag4) previewMaskIndex = num4;

            var flag5 = num7 + 100f > num2;
            if (flag5) num2 = num7 + 100f;
        }

        foreach (var mat in tempMaterials) GWMaterialPool.Return(mat);

        tempMaterials.Clear();
        Widgets.EndScrollView();
        lastMaskHeight = num2 + 20f;
        #1#
        
    }

    private void DrawTitle(Rect inRect, string title, out Rect topRect, bool addDivider = true)
    {
        topRect = inRect.TopPartPixels(30);
        var titleRect = topRect;
        titleRect.x += 5;
        
        GUI.color = Color.gray;
        Text.Font = GameFont.Medium;
        Widgets.Label(titleRect, title);
        Text.Font = GameFont.Small;
        GUI.color = Widgets.MenuSectionBGBorderColor;
        if (addDivider)
        {
            Widgets.DrawLineHorizontal(topRect.x, topRect.yMax, topRect.width * 0.75f);
        }
        GUI.color = Color.white;
    }
    
}*/