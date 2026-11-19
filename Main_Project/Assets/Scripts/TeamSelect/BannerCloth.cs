using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BannerCloth : BaseMeshEffect
{
    private const float Duration = 0.82f;
    private const int Rows = 32;
    private static readonly AnimationCurve HemMotion = CreateHemMotion();
    private float bend;

    private static AnimationCurve CreateHemMotion()
    {
        // Horizontal hem offset as a fraction of the banner width.
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.082f, 0.108f),
            new Keyframe(0.174f, 0.193f), new Keyframe(0.275f, 0.129f),
            new Keyframe(0.344f, 0.044f), new Keyframe(0.442f, -0.053f),
            new Keyframe(0.580f, -0.018f), new Keyframe(0.676f, 0.009f),
            new Keyframe(Duration, 0f));
        for (int i = 1; i < curve.length - 1; i++)
            curve.SmoothTangents(i, 0f);
        return curve;
    }

    public void PlayArrival(float direction)
    {
        StopArrival();
        DOVirtual.Float(0f, Duration, Duration, time => SetBend(direction * HemMotion.Evaluate(time)))
            .SetEase(Ease.Linear).SetTarget(this);
    }

    public void StopArrival()
    {
        this.DOKill();
        SetBend(0f);
    }

    protected override void OnDisable()
    {
        StopArrival();
        base.OnDisable();
    }

    private void SetBend(float value)
    {
        if (bend == value) return;
        bend = value;
        graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || bend == 0f || mesh.currentVertCount != 4) return;

        // Start from the fitted Image quad so sprite padding, UVs and tint are preserved.
        UIVertex lowerLeft = default, upperRight = default;
        mesh.PopulateUIVertex(ref lowerLeft, 0);
        mesh.PopulateUIVertex(ref upperRight, 2);
        Vector3 size = upperRight.position - lowerLeft.position;
        mesh.Clear();
        for (int row = 0; row <= Rows; row++)
        {
            float v = (float)row / Rows;
            // The upper 7.5% contains the rigid rod. The cloth bends most near the hem.
            float weight = Mathf.Pow(Mathf.Clamp01((0.925f - v) / 0.925f), 3.1f);
            for (int side = 0; side <= 1; side++)
            {
                var vertex = lowerLeft;
                vertex.position.x += size.x * (side + bend * weight);
                vertex.position.y += size.y * (v + weight * (bend * bend * 0.30f - bend * (side - 0.5f) * 0.22f));
                vertex.uv0.x = Mathf.Lerp(lowerLeft.uv0.x, upperRight.uv0.x, side);
                vertex.uv0.y = Mathf.Lerp(lowerLeft.uv0.y, upperRight.uv0.y, v);
                mesh.AddVert(vertex);
            }
            if (row == 0) continue;
            int bottom = (row - 1) * 2;
            mesh.AddTriangle(bottom, bottom + 2, bottom + 3);
            mesh.AddTriangle(bottom + 3, bottom + 1, bottom);
        }
    }
}
