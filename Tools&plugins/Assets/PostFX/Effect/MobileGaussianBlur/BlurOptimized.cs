using System;
using UnityEngine;

namespace PostFX
{
    [Serializable]
    public class BlurOptimized : PostEffectBase
    {

        [Range(0, 2)]
        public int downsample = 1;

        [Range(0.0f, 10.0f)]
        public float blurSize = 3.0f;

        [Range(1, 4)]
        public int blurIterations = 2;

        protected override void CreateMaterial()
        {
            if (base.material == null)
            {
                base.shader = Shader.Find("Hidden/FastBlur");
                if (base.shader != null)
                {
                    base.material = new Material(base.shader);
                }
            }
        }

        public override void Enable()
        {
            et = EffectType.BlurOptimized;
            CreateMaterial();
        }


        public override void Dispose()
        {
            if (material != null)
                GameObject.DestroyImmediate(material);

            base.Dispose();
        }

        public override void PreProcess(RenderTexture source, RenderTexture destination)
        {
            if (material == null)
                CreateMaterial();
            else
            {



                float widthMod = 1.0f / (1.0f * (1 << downsample));

                material.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
                source.filterMode = FilterMode.Bilinear;

                int rtW = source.width >> downsample;
                int rtH = source.height >> downsample;

                // downsample
                RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

                rt.filterMode = FilterMode.Bilinear;
                Graphics.Blit(source, rt, material, 0);

                var passOffs = 0;

                for (int i = 0; i < blurIterations; i++)
                {
                    float iterationOffs = (i * 1.0f);
                    material.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

                    // vertical blur
                    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                    rt2.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(rt, rt2, material, 1 + passOffs);
                    RenderTexture.ReleaseTemporary(rt);
                    rt = rt2;

                    // horizontal blur
                    rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                    rt2.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(rt, rt2, material, 2 + passOffs);
                    RenderTexture.ReleaseTemporary(rt);
                    rt = rt2;
                }

                Graphics.Blit(rt, destination);

                RenderTexture.ReleaseTemporary(rt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o">[blur��������,Blur��С��downsampler]</param>
        public override void ToParam(object[] o)
        {
            if (o[0] != null)
                blurIterations = Convert.ToInt32(o[0]);

            if (o[1] != null)
                blurSize = Convert.ToSingle(o[1]);

            if (o[2] != null)
                downsample = Convert.ToInt32(o[2]);

        }
    }
}
