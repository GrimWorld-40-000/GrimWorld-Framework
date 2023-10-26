using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor;

public class Graphic_TriColorMask : Graphic_Multi
{
	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		var apparel = thing as Apparel;
		var flag = apparel != null;
		if (flag) rot = Rot4.South;

		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		var flag = false;
		var rotation = thing.Rotation;
		var apparel = thing as Apparel;
		var flag2 = apparel != null;
		if (flag2)
		{
			rotation = thing.Rotation;
			thing.Rotation = Rot4.South;
			flag = true;
		}

		base.Print(layer, thing, extraRotation);
		var flag3 = flag;
		if (flag3) thing.Rotation = rotation;
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
		var flag = array[0] == null;
		if (flag)
		{
			var flag2 = array[2] != null;
			if (flag2)
			{
				array[0] = array[2];
				drawRotatedExtraAngleOffset = 180f;
			}
			else
			{
				var flag3 = array[1] != null;
				if (flag3)
				{
					array[0] = array[1];
					drawRotatedExtraAngleOffset = -90f;
				}
				else
				{
					var flag4 = array[3] != null;
					if (flag4)
					{
						array[0] = array[3];
						drawRotatedExtraAngleOffset = 90f;
					}
					else
					{
						array[0] = ContentFinder<Texture2D>.Get(req.path, false);
					}
				}
			}
		}

		var flag5 = array[0] == null;
		if (flag5)
		{
			Log.Error("Failed to find any textures at " + req.path + " while constructing " +
			          this.ToStringSafe());
		}
		else
		{
			var flag6 = array[2] == null;
			if (flag6) array[2] = array[0];

			var flag7 = array[1] == null;
			if (flag7)
			{
				var flag8 = array[3] != null;
				if (flag8)
				{
					array[1] = array[3];
					eastFlipped = DataAllowsFlip;
				}
				else
				{
					array[1] = array[0];
				}
			}

			var flag9 = array[3] == null;
			if (flag9)
			{
				var flag10 = array[1] != null;
				if (flag10)
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
			var flag11 = array2[0] == null;
			if (flag11)
			{
				var flag12 = array2[2] != null;
				if (flag12)
				{
					array2[0] = array2[2];
				}
				else
				{
					var flag13 = array2[1] != null;
					if (flag13)
					{
						array2[0] = array2[1];
					}
					else
					{
						var flag14 = array2[3] != null;
						if (flag14) array2[0] = array2[3];
					}
				}
			}

			var flag15 = array2[2] == null;
			if (flag15) array2[2] = array2[0];

			var flag16 = array2[1] == null;
			if (flag16)
			{
				var flag17 = array2[3] != null;
				if (flag17)
					array2[1] = array2[3];
				else
					array2[1] = array2[0];
			}

			var flag18 = array2[3] == null;
			if (flag18)
			{
				var flag19 = array2[1] != null;
				if (flag19)
					array2[3] = array2[1];
				else
					array2[3] = array2[0];
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