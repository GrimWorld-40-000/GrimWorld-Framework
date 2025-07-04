using System;
using System.Collections.Generic;
using System.Linq;
using ColorPicker;
using GW4KArmor.Data;
using GW4KArmor.Debugging;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace GW4KArmor.UI
{
    public class Window_ItemPainter : Window
    {
        [TweakValue("_GW", 0f, 700f)]
        private static readonly float colorsWidthPx = 448f;

        [TweakValue("_GW", 0f, 600f)]
        private static readonly float colorsHeightPx = 330f;

        [TweakValue("_GW", 0f, 100f)]
        private static readonly float tabsHeightPx = 70f;

        [TweakValue("_GW", 0f, 500f)]
        private static readonly float paletteWidthPx = 350f;

        [TweakValue("_GW", 0f, 400f)]
        private static readonly float mainButtonsHeightPx = 70f;

        private readonly Color[] colors =
        [
            Color.red,
            Color.green,
            Color.blue
        ];
        
        private readonly ThingWithComps target;
        private readonly Comp_TriColorMask comp;
        private Material materialInstance;
        private float lastPaletteHeight;
        private float lastMaskHeight;

        private readonly ColorPickerWidget _colorPicker;
        //private readonly ColorPicker.ColorPickerTool _pickerTool;
        private readonly List<TabRecord> colorTabs;

        private readonly Color[] originalColors = new Color[3];
        private readonly int originalMask;
        private readonly List<Material> tempMaterials = [];
        private int previewMaskIndex;
        private int previewRotation = 2;
        private string paletteName = "My Palette";

        //New
        private const float Pallete_Option_Size = 65;
        private Vector2 _paletteScroller;

        public static Window_ItemPainter OpenWindowFor(ThingWithComps thing)
        {
            Window_ItemPainter windowItemPainter = new(thing);
            Find.WindowStack.Add(windowItemPainter);
            return windowItemPainter;
        }

        public override Vector2 InitialSize => new(1500f, 800f);

        private Window_ItemPainter(ThingWithComps thing)
        {
            target = thing;
            Comp_TriColorMask triColorMaskComp = thing.GetComp<Comp_TriColorMask>();
            comp = triColorMaskComp ?? throw new Exception($"Missing TriColorMaskComp on {thing} ({thing.def.defName})");
            
            previewMaskIndex = comp.MaskIndex;
            colors[0] = comp.ColorOne;
            colors[1] = comp.ColorTwo;
            colors[2] = comp.ColorThree;
            originalColors[0] = colors[0];
            originalColors[1] = colors[1];
            originalColors[2] = colors[2];
            originalMask = comp.MaskIndex;
            closeOnClickedOutside = false;
            preventCameraMotion = false;
            forcePause = false;
            resizeable = true;
            draggable = true;
            doCloseX = true;
            colorTabs =
            [
                new TabRecord("ColA", delegate { OnColorTabClicked(0); }, true),
                new TabRecord("ColB", delegate { OnColorTabClicked(1); }, false),
                new TabRecord("ColC", delegate { OnColorTabClicked(2); }, false)
            ];

            _colorPicker = new ColorPickerWidget(colors[0], OnColorChanged);
            // _pickerTool = new ColorPickerTool();
            // _pickerTool.OnColorChanged += OnColorChanged;

            OnColorTabClicked(2);
            OnColorTabClicked(1);
            OnColorTabClicked(0);
        }

        private void OnColorTabClicked(int index)
        {
            for (int i = 0; i < colorTabs.Count; i++)
            {
                TabRecord tabRecord = colorTabs[i];
                tabRecord.selected = i == index;
                tabRecord.label = $"<color=#{ColorUtility.ToHtmlStringRGB(colors[i])}>{'█'}{'█'}</color> Color {i + 1}";

                if (!tabRecord.selected) 
                    continue;
                
                _colorPicker.R = colors[i].r;
                _colorPicker.G = colors[i].g;
                _colorPicker.B = colors[i].b;
                _colorPicker.A = colors[i].a;

                //_pickerTool.SetColorDirect(colors[i]);
            }
        }

        private int GetSelectedColorIndex()
        {
            for (int i = 0; i < colorTabs.Count; i++)
            {
                bool selected = colorTabs[i].selected;
                if (selected) return i;
            }
            return -1;
        }

        private void OnColorChanged(Color newColor)
        {
            int selectedColorIndex = GetSelectedColorIndex();
            TabRecord tabRecord = colorTabs[selectedColorIndex];
            tabRecord.label =
                $"<color=#{ColorUtility.ToHtmlStringRGB(newColor)}>{'█'}{'█'}</color> Color {selectedColorIndex + 1}";
        }

        private bool Guard_BadInstance()
        {
            ThingWithComps thingWithComps = target;

            if (thingWithComps != null && !target.Destroyed)
                return false;
            
            GW4KLog.Error("[Guard_BadInstance] Bad item/clothes instance, closing window.");
            Close();
            return true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Guard_BadInstance()) 
                return;
            
            //Layout
            Rect leftPart = inRect.LeftPartPixels(256);
            Rect rightPart = inRect.RightPartPixels(colorsWidthPx); //384
            Rect center = new(leftPart.xMax, leftPart.y, rightPart.x - leftPart.xMax, leftPart.height);
            
            //
            ItemPreview(center);
            
            //Pattern selection
            DrawPatternSelection(leftPart);
            
            //Color Group
            float pixels = TabDrawer.TabHeight + 210 + 30 + 10; //_dynamicRectColorPicker ?? 384;
            Rect colorPickerRect = rightPart.TopPartPixels(pixels).Rounded();
            Rect paletteRect = rightPart.BottomPartPixels((rightPart.height - pixels) - 10).Rounded();
            
            TriColorPicker(colorPickerRect);
            PaletteSelection(paletteRect);
        }

        #region New

        private void ItemPreview(Rect inRect)
        {
            DrawMainButtons(inRect.BottomPartPixels(mainButtonsHeightPx));
            DrawPreview(inRect.TopPartPixels(inRect.height - mainButtonsHeightPx));
            return;

            /*
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
            GUI.EndGroup();*/

            //Rotation Controls
        }

        private void DrawPatternSelection(Rect inRect)
        {
            Widgets.DrawMenuSection(inRect);
            DrawTitle(inRect, "Pattern Selection", out Rect topRect);

            const int selectionsPerRow = 2;
            const int spacing = 5;
            inRect = new Rect(topRect.xMin, topRect.yMax, topRect.width, inRect.height - topRect.height);
            int selectionBoxSize = Mathf.RoundToInt((inRect.width - ((selectionsPerRow + 1) * spacing)) / selectionsPerRow);
            Rect viewRect = new(inRect.x, inRect.y, inRect.width - 20f, lastMaskHeight);
            lastMaskHeight = 0;

            Widgets.BeginScrollView(inRect, ref ScrollRects.Get("Masks"), viewRect, true);
            Texture2D previewMainTexture = GetPreviewMainTexture(Rot4.South);

            float num2 = 0f;
            int curIndex = -1;
            int gridX, gridY = 0;
            
            foreach (Texture2D value in EnumerateMaskTextures)
            {
                curIndex++;
                gridX = curIndex % selectionsPerRow;
                gridY = curIndex / selectionsPerRow;
                int x = gridX * selectionBoxSize + (spacing * (gridX + 1));
                int y = gridY * selectionBoxSize + (spacing * (gridY + 1)); ;
                Rect rect2 = new Rect(inRect.x + x, inRect.y + y, selectionBoxSize, selectionBoxSize);
                
                if (curIndex == previewMaskIndex)
                {
                    GUI.color = Color.yellow;
                    Widgets.DrawBox(rect2, 2);
                    GUI.color = Color.white;
                }

                // the actual custom shader they originally had??...
                Material material = MaterialPool.Get();
                tempMaterials.Add(material);
                material.SetColor("_Color", colors[0]);
                material.SetColor("_ColorTwo", colors[1]);
                material.SetColor("_ColorThree", colors[2]);
                material.SetTexture("_MaskTex", value);
                
                GUI.BeginClip(rect2);
                {
                    Graphics.DrawTexture(rect2.AtZero(), previewMainTexture, 
                        new Rect(0f, 0f, 1f, 1f),
                        0, 0, 0, 0, material);
                }
                GUI.EndClip();
                
                if (Mouse.IsOver(rect2))
                {
                    GUI.color = Color.white * 0.7f;
                    Widgets.DrawBox(rect2);
                    GUI.color = Color.white;
                }

                if (Widgets.ButtonInvisible(rect2))
                {
                    previewMaskIndex = curIndex;
                }

                if (y + 100f > num2)
                {
                    num2 = y + 100f;
                }
            }

            lastMaskHeight = gridY * selectionBoxSize + (spacing * (gridY + 1)) + selectionBoxSize;
            Widgets.EndScrollView();
        }

        private void TriColorPicker(Rect inRect)
        {
            Rect tabRect = inRect.TopPartPixels(TabDrawer.TabHeight);
            Rect pickerRect = inRect.BottomPartPixels(inRect.height - TabDrawer.TabHeight);
            tabRect.y = tabRect.yMax;

            Widgets.DrawMenuSection(pickerRect);
            TabDrawer.DrawTabs(tabRect, colorTabs);
            DrawColorPicker(pickerRect.ContractedBy(5));
        }

        private void PaletteSelection(Rect inRect)
        {
            //Palette Data
            Widgets.DrawMenuSection(inRect);

            //Title
            DrawTitle(inRect, "Palette Selection", out Rect topRect, false);
            //Rect tabRect = new Rect(topRect.x, topRect.yMax+topRect.height, topRect.width, 5);
            Rect scrollRect = new Rect(topRect.x, topRect.yMax, topRect.width, inRect.height - topRect.height).ContractedBy(5); //.BottomPartPixels(inRect.yMax - tabRect.y+5).ContractedBy(5);
            
            // TabDrawer.DrawTabs(tabRect, _paletteTabs);
            //       
            // switch (_paletteTab)
            // {
            // 	case PaletteTab.Unique:
            // 		DrawPaletteGroup(scrollRect, testPalets);
            // 		break;
            // 	case PaletteTab.Global:
            // 		break;
            // }

            List<Palette> palettes = CurrentPalettes.ToList();
            DrawPaletteGroup(scrollRect, palettes);
        }
        
        private void DrawPaletteGroup(Rect inRect, IReadOnlyList<Palette> palettes)
        {
            //Guide-Lines
            GUI.color = new ColorInt(135, 135, 135).ToColor;
            Widgets.DrawLineHorizontal(inRect.x - 5, inRect.y, inRect.width + 10);
            Widgets.DrawLineVertical(inRect.x, inRect.y, inRect.height);
            Widgets.DrawLineVertical(inRect.xMax, inRect.y, inRect.height);
            Widgets.DrawLineHorizontal(inRect.x - 5, inRect.yMax, inRect.width + 10);
            GUI.color = Color.white;

            float rowOptionHeight = inRect.height / 6;
            Rect scrollableRect = new(inRect.position, new Vector2(inRect.width, palettes.Count * rowOptionHeight));

            //Scroll
            Widgets.BeginGroup(inRect);
            inRect = inRect.AtZero();
            scrollableRect = scrollableRect.AtZero();
            _paletteScroller = new Vector2(_paletteScroller.x, Mathf.RoundToInt(_paletteScroller.y / rowOptionHeight) * rowOptionHeight);
            Widgets.BeginScrollView(inRect, ref _paletteScroller, scrollableRect, false);

            for (int i = 0; i < palettes.Count; i++)
            {
                Rect rect = new(inRect.x, inRect.y + (i * rowOptionHeight), inRect.width, rowOptionHeight);
                DrawPaletteRow(rect, i, palettes[i], scrollableRect);
            }

            Widgets.EndScrollView();
            Widgets.EndGroup();
        }
        
        private void DrawPaletteRow(Rect rect, int i, Palette palette, Rect containingRect)
        {
            Text.Font = GameFont.Small;
            Vector2 size = Text.CalcSize(palette.name);
            Rect titleRect = rect.TopPartPixels(size.y);
            titleRect.x += 5;

            Rect paletteRect = rect.BottomPartPixels(rect.height - size.y).ContractedBy(5).Rounded();
            Rect iconRect = paletteRect.LeftPartPixels(paletteRect.height).Rounded();
            Rect paletteDataRect = paletteRect.RightPartPixels(paletteRect.width - iconRect.width - 10).Rounded();
            Rect labelsRect = paletteDataRect.TopHalf().Rounded();
            Rect colorsRect = paletteDataRect.BottomHalf().Rounded();

            Widgets.DrawHighlightIfMouseover(rect);
            
            if (i % 2 == 0)
            {
                Widgets.DrawHighlight(rect);
            }

            Widgets.Label(titleRect, palette.name);

            //Show colored texture
            Widgets.DrawHighlight(iconRect);

            //MainTex
            Texture2D previewMainTexture = GetPreviewMainTexture(Rot4.South);

            //MaskTex
            MaskTextureStorage masks = comp.Masks;
            TextureID textureID = default;
            textureID.BodyType = NeedsBodyType() ? BodyTypeDefOf.Male : null;
            textureID.Index = previewMaskIndex;
            textureID.Rotation = Rot4.South.AsByte;
            Texture2D maskTex = masks.GetTexture(textureID);

            Material material = MaterialPool.StaticMask;
            material.SetColor("_Color", palette.colorA);
            material.SetColor("_ColorTwo", palette.colorB);
            material.SetColor("_ColorThree", palette.colorC);
            material.SetTexture("_MaskTex", maskTex);

            Widgets.BeginGroup(iconRect);
            Rect iconRect2 = iconRect.AtZero().ExpandedBy(16).Rounded();
            Graphics.DrawTexture(iconRect2, previewMainTexture, material);
            Widgets.EndGroup();

            if (Widgets.ButtonInvisible(rect))
            {
                EnablePalette(palette);
            }

            //Widgets.DrawTextureFitted();
            Rect colorARect = paletteDataRect.LeftPartPixels(100);
            Rect colorBRect = paletteDataRect.LeftPartPixels(200).RightPartPixels(100);
            Rect colorCRect = paletteDataRect.LeftPartPixels(300).RightPartPixels(100);

            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(colorARect.TopHalf(), "Color A");
            Widgets.Label(colorBRect.TopHalf(), "Color B");
            Widgets.Label(colorCRect.TopHalf(), "Color C");
            Widgets.DrawBoxSolid(colorARect.BottomHalf(), palette.colorA);
            Widgets.DrawBoxSolid(colorBRect.BottomHalf(), palette.colorB);
            Widgets.DrawBoxSolid(colorCRect.BottomHalf(), palette.colorC);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private static void DrawTitle(Rect inRect, string title, out Rect topRect, bool addDivider = true)
        {
            topRect = inRect.TopPartPixels(30);
            Rect titleRect = topRect;
            titleRect.x += 5;

            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;
            GUI.color = new ColorInt(135, 135, 135).ToColor;

            if (addDivider)
            {
                Widgets.DrawLineHorizontal(topRect.x, topRect.yMax, topRect.width * 0.75f);
            }
            GUI.color = Color.white;
        }

        #endregion

        #region Legacy

        private void LegacyDoWindowContents(Rect rect)
        {
            if (materialInstance != null)
            {
                Text.Font = GameFont.Medium;
                var rect2 = rect.LeftPartPixels(colorsWidthPx);
                var rect3 = rect.RightPartPixels(paletteWidthPx);
                var rect4 = rect;
                rect4.width = rect.width - colorsWidthPx - paletteWidthPx;
                rect4.x = colorsWidthPx;
                var rect5 = rect4;
                var rect6 = rect2.TopPartPixels(colorsHeightPx);
                var rect7 = rect2.BottomPartPixels(rect2.height - colorsHeightPx);
                var rect8 = rect6.TopPartPixels(tabsHeightPx);
                var rect9 = rect6.BottomPartPixels(rect6.height - tabsHeightPx);
                DrawPart(rect8, DrawColorPickerTabs);
                DrawPart(rect9, DrawColorPicker);
                var rect12 = rect5.TopPartPixels(rect5.height - mainButtonsHeightPx);
                var rect13 = rect5.BottomPartPixels(mainButtonsHeightPx);
                var rect14 = rect12;
                DrawPart(rect14, DrawPreview);
                var rect15 = rect13;
                DrawPart(rect15, DrawMainButtons);
                var rect16 = rect7;
                DrawPart(rect16, DrawMasks);
                var rect17 = rect3;
                DrawPart(rect17, DrawPalettes);
            }
        }

        private void DrawColorPickerTabs(Rect rect)
        {
            rect.y = rect.yMax;
            TabDrawer.DrawTabs(rect, colorTabs, 1);
        }

        private void DrawColorPicker(Rect rect)
        {
            var colorPickerRect = rect.TopPartPixels(210);
            var saveDataRect = rect.BottomPartPixels(rect.height - 210);
            _colorPicker.Draw(colorPickerRect);

            //_pickerTool.Draw(inRect);

            for (var i = 0; i < colors.Length; i++)
            {
                var tabRecord = colorTabs[i];
                if (tabRecord.selected)
                {
                    //pickerTool.color;
                    colors[i] = _colorPicker.curColour;
                }
            }

            var rect2 = saveDataRect.RightPartPixels(100f);
            var rect3 = saveDataRect.LeftPartPixels(rect.width - 110f);
            paletteName = Widgets.TextField(rect3, paletteName, 32);
            if (Widgets.ButtonText(rect2, "Save Palette"))
                TrySavePalette();
        }

        #endregion


        public override void PreOpen()
        {
            base.PreOpen();
            materialInstance = new Material(Core.MaskMaterial);
        }

        public override void PostClose()
        {
            base.PostClose();
            
            if (materialInstance == null) 
                return;
            
            Object.Destroy(materialInstance);
            materialInstance = null;
        }

        private void DrawPart(Rect rect, in Action<Rect> drawAction)
        {
            Rect obj = rect.ExpandedBy(-8f, -8f);
            drawAction(obj);
        }

        private bool NeedsBodyType()
        {
            Apparel apparel = target as Apparel;
            bool flag = apparel == null;
            bool result;
            
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
                             apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover ||
                             apparel.RenderAsPack() ||
                             apparel.WornGraphicPath == BaseContent.PlaceholderImagePath ||
                             apparel.WornGraphicPath == BaseContent.PlaceholderGearImagePath;
                result = !flag2;
            }
            return result;
        }

        private bool NeedsRotation()
        {
            return target is Apparel;
        }

        private void DrawPreview(Rect rect)
        {
            using (new TempAnchor(TextAnchor.UpperCenter))
            {
                Widgets.Label(rect, "<size=28><b>" + target.LabelNoParenthesisCap + "</b></size>");
            }

            rect.yMin += 40f;
            float num = Mathf.Min(rect.width, rect.height);
            Rect rect2 = new Rect(0f, rect.y + 60f, num - 120f, num - 120f).CenteredOnXIn(rect);
            Comp_TriColorMask triColorMaskComp = comp;
            
            if (triColorMaskComp == null || materialInstance == null)
            {
                return;
            }

            MaskTextureStorage masks = triColorMaskComp.Masks;
            TextureID textureID = default(TextureID);
            textureID.BodyType = NeedsBodyType() ? BodyTypeDefOf.Male : null;
            textureID.Index = previewMaskIndex;
            textureID.Rotation = (byte)(NeedsRotation() ? (byte)previewRotation : 4);
            Texture2D texture2D = masks.GetTexture(textureID);

            Texture2D previewMainTexture = GetPreviewMainTexture();
            Rect sourceRect = previewRotation == 1 
                ? new Rect(1f, 0f, -1f, 1f) 
                : new Rect(0f, 0f, 1f, 1f);
            materialInstance.SetColor("_Color", colors[0]);
            materialInstance.SetColor("_ColorTwo", colors[1]);
            materialInstance.SetColor("_ColorThree", colors[2]);
            materialInstance.SetTexture("_MaskTex", texture2D);
            Widgets.BeginGroup(rect2);
            Graphics.DrawTexture(new Rect(0f, 0f, rect2.width, rect2.height), 
                previewMainTexture, sourceRect, 0, 0, 0, 0, materialInstance);
            Widgets.EndGroup();

            Rect rect3 = rect2.LeftPartPixels(50f);
            rect3.x -= 55f;
            Rect rect4 = rect2.RightPartPixels(50f);
            rect4.x += 55f;
            bool flag = NeedsRotation();
            
            if (flag)
            {
                Widgets.DrawHighlightIfMouseover(rect3);
                using (new TempAnchor(TextAnchor.MiddleCenter))
                {
                    Widgets.Label(rect3, "<size=42><b><</b></size>");
                }

                if (Widgets.ButtonInvisible(rect3))
                {
                    int num2 = previewRotation - 1;
                    previewRotation = num2;
                    previewRotation = num2 % 4;
                    bool flag3 = previewRotation < 0;
                    
                    if (flag3)
                    {
                        previewRotation += 4;
                    }
                }

                Widgets.DrawHighlightIfMouseover(rect4);
                using (new TempAnchor(TextAnchor.MiddleCenter))
                {
                    Widgets.Label(rect4, "<size=42><b>></b></size>");
                }

                if (Widgets.ButtonInvisible(rect4))
                {
                    int num2 = previewRotation + 1;
                    previewRotation = num2;
                    previewRotation = num2 % 4;
                }
            }
            else
            {
                previewRotation = 0;
            }
        }

        private Texture2D GetPreviewMainTexture(Rot4? overrideRot = null)
        {
            Apparel apparel = target as Apparel;
            bool flag = apparel != null;
            Texture2D result;
            
            if (flag)
            {
                ApparelGraphicRecord apparelGraphicRecord;
                bool flag2 =
                    !ApparelGraphicRecordGetter
                        .TryGetGraphicApparel(apparel, BodyTypeDefOf.Male, out apparelGraphicRecord);
                
                if (flag2)
                {
                    result = null;
                }
                else
                {
                    result = apparelGraphicRecord.graphic.MatAt(overrideRot ?? new Rot4(previewRotation), target)
                        .mainTexture as Texture2D;
                }
            }
            else
            {
                result = target.Graphic.MatSingle.mainTexture as Texture2D;
            }
            return result;
        }

        private IEnumerable<Texture2D> EnumerateMaskTextures
        {
            get
            {
                int num;
                for (int i = 0; i < comp.Props.maskCount; i = num + 1)
                {
                    MaskTextureStorage masks = comp.Masks;
                    TextureID textureID = default;
                    textureID.BodyType = NeedsBodyType() ? BodyTypeDefOf.Male : null;
                    textureID.Index = i;
                    textureID.Rotation = (byte)(NeedsRotation() ? Rot4.South.AsByte : 4);
                    Texture2D mask = masks.GetTexture(textureID);
                    
                    yield return
                        mask ? mask : BaseContent.BadTex;
                    num = i;
                }
            }
        }

        private void DrawMasks(Rect rect)
        {
            Widgets.DrawBox(rect);
            using (new TempAnchor(TextAnchor.UpperCenter))
            {
                Widgets.Label(rect, "<size=28><b>Patterns</b></size>");
            }

            rect.yMin += 40f;
            int num = (int)Mathf.Max(1f, (int)rect.width / 120f);
            Widgets.DrawWindowBackground(rect);
            Widgets.BeginScrollView(rect.ExpandedBy(-2f, -8f), ref ScrollRects.Get("Masks"), 
                new Rect(0f, 0f, rect.width - 20f, lastMaskHeight), true);
            Texture2D previewMainTexture = GetPreviewMainTexture(Rot4.South);
            float num2 = 0f;
            float num3 = rect.width - 20f - (num * 120f - 20f);
            int num4 = -1;
            
            foreach (Texture2D value in EnumerateMaskTextures)
            {
                num4++;
                int num5 = num4 % num;
                int num6 = num4 / num;
                float x = num3 * 0.5f + num5 * 120f;
                float num7 = num6 * 120f + 20f;
                Rect rect2 = new(x, num7, 100f, 100f);
                
                if (num4 == previewMaskIndex)
                {
                    GUI.color = Color.yellow;
                    Widgets.DrawBox(rect2.ExpandedBy(4f, 4f), 2);
                    GUI.color = Color.white;
                }

                Material material = MaterialPool.Get();
                tempMaterials.Add(material);
                material.SetColor("_Color", colors[0]);
                material.SetColor("_ColorTwo", colors[1]);
                material.SetColor("_ColorThree", colors[2]);
                material.SetTexture("_MaskTex", value);
                GUI.BeginGroup(rect2);
                {
                    Graphics.DrawTexture(new Rect(0f, 0f, rect2.width, rect2.height), previewMainTexture,
                        new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, material);
                }
                GUI.EndGroup();
                
                if (Mouse.IsOver(rect2))
                {
                    GUI.color = Color.white * 0.7f;
                    Widgets.DrawBox(rect2.ExpandedBy(4f, 4f));
                    GUI.color = Color.white;
                }

                if (Widgets.ButtonInvisible(rect2))
                {
                    previewMaskIndex = num4;
                }

                if (num7 + 100f > num2)
                {
                    num2 = num7 + 100f;
                }
            }
            
            foreach (Material mat in tempMaterials)
            {
                MaterialPool.Return(mat);
            }

            tempMaterials.Clear();
            Widgets.EndScrollView();
            lastMaskHeight = num2 + 20f;
        }

        private void DrawMainButtons(Rect rect)
        {
            GameFont font = Text.Font;
            Text.Font = GameFont.Small;

            Rect rect2 = rect.LeftHalf().ExpandedBy(-5f);
            Rect rect3 = rect.RightHalf().ExpandedBy(-5f);
            if (Widgets.ButtonText(rect2, "Save"))
            {
                comp.MaskIndex = previewMaskIndex;
                comp.ColorOne = colors[0];
                comp.ColorTwo = colors[1];
                comp.ColorThree = colors[2];
                comp.MarkDirty();
                Messages.Message("Applied successfully!", MessageTypeDefOf.NeutralEvent, false);
            }

            if (Widgets.ButtonText(rect3, "Revert"))
            {
                Apply(originalColors, originalMask);
                Messages.Message("Reverted to original colors & mask.", MessageTypeDefOf.NeutralEvent, false);
            }

            Text.Font = font;
        }

        private IEnumerable<Palette> CurrentPalettes
        {
            get
            {
                if (comp.Props.palettePresets != null)
                {
                    foreach (Palette p in comp.Props.palettePresets.palettes)
                    {
                        yield return p;
                    }
                }

                foreach (Palette p2 in PaletteStorage.Current.Palettes)
                {
                    yield return p2;
                }
            }
        }

        private void DrawPalettes(Rect rect)
        {
            using (new TempAnchor(TextAnchor.UpperCenter))
            {
                Widgets.Label(rect, "<size=28><b>Palettes</b></size>");
            }

            rect.yMin += 40f;
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
            rect.yMin += 10f;
            float num = 0f;
            Widgets.BeginScrollView(rect, ref ScrollRects.Get("Palette"),
                new Rect(0f, 0f, rect.width - 20f, lastPaletteHeight), true);
            Palette toDelete = null;
            
            foreach (Palette palette in CurrentPalettes)
            {
                float num2 = 40f;
                Rect rect2 = new Rect(0f, num, rect.width - 20f, num2);
                bool flag = palette.Draw(rect2, ref num2, delegate { toDelete = palette; });
                
                if (flag)
                {
                    EnablePalette(palette);
                }
                num += num2 + 20f;
            }

            Widgets.EndScrollView();
            lastPaletteHeight = num;
            
            if (toDelete != null)
            {
                PaletteStorage.Current.Palettes.Remove(toDelete);
            }
        }

        private void EnablePalette(Palette p)
        {
            Apply(new[]
            {
                p.colorA,
                p.colorB,
                p.colorC
            }, 
                previewMaskIndex);
            paletteName = p.name;
        }

        private void TrySavePalette()
        {
            paletteName = paletteName.Trim();
            bool flag = !string.IsNullOrWhiteSpace(paletteName);
            Palette palette = PaletteStorage.Current.Palettes.FirstOrDefault((Palette p) => p.name == paletteName);
           
            if (flag)
            {
                bool flag3 = palette == null;
                
                if (flag3)
                {
                    Palette item = new Palette
                    {
                        name = paletteName,
                        colorA = colors[0],
                        colorB = colors[1],
                        colorC = colors[2]
                    };
                    PaletteStorage.Current.Palettes.Add(item);
                    Messages.Message("Saved new palette '" + paletteName + "'", MessageTypeDefOf.PositiveEvent, false);
                }
                else
                {
                    palette.colorA = colors[0];
                    palette.colorB = colors[1];
                    palette.colorC = colors[2];
                    Messages.Message("Overwrote existing palette '" + paletteName + "'",
                        MessageTypeDefOf.PositiveEvent, false);
                }
            }
            else
            {
                Messages.Message("Must provide a name for this palette!", MessageTypeDefOf.RejectInput, false);
            }
        }

        private void Apply(IReadOnlyList<Color> col, int maskIndex)
        {
            previewMaskIndex = maskIndex;
            colors[0] = col[0];
            colors[1] = col[1];
            colors[2] = col[2];
            OnColorTabClicked(2);
            OnColorTabClicked(1);
            OnColorTabClicked(0);
        }
    }
}