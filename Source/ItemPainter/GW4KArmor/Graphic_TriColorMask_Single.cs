using UnityEngine;
using Verse;

namespace GW4KArmor;

public class Graphic_TriColorMask_Single : Graphic_Single
{
    public override void Init(GraphicRequest req)
    {
        data = req.graphicData;
        path = req.path;
        maskPath = req.maskPath;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
        mat = MaterialPool.MatFrom(new MaterialRequest
        {
            mainTex = req.texture ?? ContentFinder<Texture2D>.Get(req.path),
            shader = req.shader,
            color = color,
            colorTwo = colorTwo,
            renderQueue = req.renderQueue,
            shaderParameters = req.shaderParameters,
            maskTex = ContentFinder<Texture2D>.Get(maskPath, false)
        });
    }
}