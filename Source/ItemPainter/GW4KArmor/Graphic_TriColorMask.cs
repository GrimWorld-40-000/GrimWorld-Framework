using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor
{
    public class Graphic_TriColorMask : Graphic_Multi
    {
        public Material[] mats = new Material[4];

        public float drawRotatedExtraAngleOffset;

        public bool westFlipped;

        public bool eastFlipped;

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            var apparel = thing as Apparel;
            if (apparel != null)
                rot = Rot4.South;

            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            var rotateBack = false;
            var rotation = thing.Rotation;

            var apparel = thing as Apparel;
            if (apparel != null)
            {
                rotation = thing.Rotation;
                thing.Rotation = Rot4.South;
                rotateBack = true;
            }

            base.Print(layer, thing, extraRotation);
            if (rotateBack)
                thing.Rotation = rotation;
        }

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            this.maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            var array = new Texture2D[mats.Length];
            array[0] = ContentFinder<Texture2D>.Get(req.path + "_north", false);
            array[1] = ContentFinder<Texture2D>.Get(req.path + "_east", false);
            array[2] = ContentFinder<Texture2D>.Get(req.path + "_south", false);
            array[3] = ContentFinder<Texture2D>.Get(req.path + "_west", false);

            if (array[0] == null)
            {
                if (array[2] != null)
                {
                    array[0] = array[2];
                    drawRotatedExtraAngleOffset = 180f;
                }
                else if (array[1] != null)
                {
                    array[0] = array[1];
                    drawRotatedExtraAngleOffset = -90f;
                }
                else if (array[3] != null)
                {
                    array[0] = array[3];
                    drawRotatedExtraAngleOffset = 90f;
                }
                else
                {
                    array[0] = ContentFinder<Texture2D>.Get(req.path, false);
                }
            }

            if (array[0] == null)
            {
                Log.Error("Failed to find any textures at " + req.path + " while constructing " +
                          this.ToStringSafe());
            }
            else
            {
                if (array[2] == null)
                {
                    array[2] = array[0];
                }

                if (array[1] == null)
                {
                    if (array[3] != null)
                    {
                        array[1] = array[3];
                        eastFlipped = DataAllowsFlip;
                    }
                    else
                    {
                        array[1] = array[0];
                    }
                }

                if (array[3] == null)
                {
                    if (array[1] != null)
                    {
                        array[3] = array[1];
                        westFlipped = DataAllowsFlip;
                    }
                    else
                    {
                        array[3] = array[0];
                    }
                }

                var array2 = new Texture2D[mats.Length];
                var maskPath = this.maskPath;
                array2[0] = ContentFinder<Texture2D>.Get(maskPath + "_north", false);
                array2[1] = ContentFinder<Texture2D>.Get(maskPath + "_east", false);
                array2[2] = ContentFinder<Texture2D>.Get(maskPath + "_south", false);
                array2[3] = ContentFinder<Texture2D>.Get(maskPath + "_west", false);

                if (array2[0] == null)
                {
                    if (array2[2] != null)
                    {
                        array2[0] = array2[2];
                    }
                    else if (array2[1] != null)
                    {
                        array2[0] = array2[1];
                    }
                    else if (array2[3] != null)
                    {
                        array2[0] = array2[3];
                    }
                }

                if (array2[2] == null)
                {
                    array2[2] = array2[0];
                }

                if (array2[1] == null)
                {
                    if (array2[3] != null)
                    {
                        array2[1] = array2[3];
                    }
                    else
                    {
                        array2[1] = array2[0];
                    }
                }

                if (array2[3] == null)
                {
                    if (array2[1] != null)
                    {
                        array2[3] = array2[1];
                    }
                    else
                    {
                        array2[3] = array2[0];
                    }
                }

                for (var i = 0; i < mats.Length; i++)
                {
                    var req2 = default(MaterialRequest);
                    req2.mainTex = array[i];
                    req2.shader = req.shader;
                    req2.color = color;
                    req2.colorTwo = colorTwo;
                    req2.maskTex = array2[i];
                    req2.shaderParameters = req.shaderParameters;
                    req2.renderQueue = req.renderQueue;
                    mats[i] = MaterialPool.MatFrom(req2);
                }
            }
        }
    }
}