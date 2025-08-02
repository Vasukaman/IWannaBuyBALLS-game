// Filename: LiquidGateView.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace VFX.Gadgets
{
    [RequireComponent(typeof(Renderer))]
    public class LiquidGateView : MonoBehaviour
    {

        private LiquidGateProfile _profile;
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;

        private static readonly int BulgeSizeID = Shader.PropertyToID("_BulgeSize");
        private static readonly int BulgePosID = Shader.PropertyToID("_BulgePos");
        private static readonly int BulgeAngleID = Shader.PropertyToID("_BulgeAngle");
        private static readonly int NoiseAmpID = Shader.PropertyToID("_NoiseAmp");
        private static readonly int XRayTopEnableID = Shader.PropertyToID("_XRayTopEnable");
        private static readonly int XRayColorTopID = Shader.PropertyToID("_XRayColorTop");
        private static readonly int HighlightColorTopID = Shader.PropertyToID("_HighlightColorTop");
        private static readonly int XRayBottomEnableID = Shader.PropertyToID("_XRayBottomEnable");
        private static readonly int XRayColorBottomID = Shader.PropertyToID("_XRayColorBottom");
        private static readonly int HighlightColorBottomID = Shader.PropertyToID("_HighlightColorBottom");
        private static readonly int BallCountID = Shader.PropertyToID("_BallCount");
        private static readonly int BallDataID = Shader.PropertyToID("_BallData");
        private static readonly int BallColorsID = Shader.PropertyToID("_BallColors");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }


        public void SetProfile(LiquidGateProfile profile)
        {
            _profile = profile;
        }

        public void UpdateShaderProperties(float bulgeSize, float bulgePos, float bulgeAngle, float noiseAmp,
            int ballCount, Vector4[] ballData, Vector4[] ballColors)
        {
            _renderer.GetPropertyBlock(_mpb);

            _mpb.SetFloat(BulgeSizeID, bulgeSize);
            _mpb.SetFloat(BulgePosID, bulgePos);
            _mpb.SetFloat(BulgeAngleID, bulgeAngle);
            _mpb.SetFloat(NoiseAmpID, noiseAmp);
            _mpb.SetInt(BallCountID, ballCount);

            if (ballCount > 0)
            {
                _mpb.SetVectorArray(BallDataID, ballData);
                _mpb.SetVectorArray(BallColorsID, ballColors);
            }

            _mpb.SetFloat(XRayTopEnableID,  _profile.XrayForTopZone ? 1.0f : 0.0f);
            _mpb.SetColor(XRayColorTopID, _profile.XrayColorTop);
            _mpb.SetColor(HighlightColorTopID, _profile.HighlightColorTop);
            _mpb.SetFloat(XRayBottomEnableID, _profile.XrayForBottomZone ? 1.0f : 0.0f);
            _mpb.SetColor(XRayColorBottomID, _profile.XrayColorBottom);
            _mpb.SetColor(HighlightColorBottomID, _profile.HighlightColorBottom);

            _renderer.SetPropertyBlock(_mpb);
        }
    }
}