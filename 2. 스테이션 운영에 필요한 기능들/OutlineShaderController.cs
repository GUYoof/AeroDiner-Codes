using UnityEngine;

public class OutlineShaderController : MonoBehaviour
{
    private SpriteRenderer sr;
    private MaterialPropertyBlock block;
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
    }

    public void EnableOutline()
    {
        sr.GetPropertyBlock(block);
        block.SetFloat(OutlineEnabled, 1);
        sr.SetPropertyBlock(block);
    }

    public void DisableOutline()
    {
        sr.GetPropertyBlock(block);
        block.SetFloat(OutlineEnabled, 0);
        sr.SetPropertyBlock(block);
    }
}
