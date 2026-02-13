using System.Collections;
using UnityEngine;

public class ShaderToggle : MonoBehaviour
{

    [SerializeField] private float dissolveTime = 0.75f;

    private SpriteRenderer spriteRenderer;
    private Material[] materials;

    private int _dissolveAmount = Shader.PropertyToID("_Dissolve");
    private int _vertical = Shader.PropertyToID("_Vertical");

    [SerializeField] bool useDissolve = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        materials = new Material[spriteRenderer.materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = spriteRenderer.materials[i];
            //Debug.Log("Material " + i + ": " + materials[i].name);
        }

        StartCoroutine(Starter());
    }

    IEnumerator Starter()
    {
        yield return StartCoroutine(Appear(!useDissolve, useDissolve));
        //yield return StartCoroutine(Disappear(false, true));
        //yield return StartCoroutine(Appear(true, false));
        //yield return StartCoroutine(Disappear(true, false));
    }

    public IEnumerator Appear(bool useDissolve, bool useVertical)
    {
        if(useDissolve)
            materials[0].SetFloat(_vertical, -2f);
        if(useVertical)
            materials[0].SetFloat(_dissolveAmount, 1.1f);
        
        float elapsedTime = 0f;
        //yield return new WaitForSeconds(0.25f);
        Debug.Log("Starting Appear Coroutine");
        
        while(elapsedTime < dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(-.2f, 1f, elapsedTime / dissolveTime);
            float verticalValue = Mathf.Lerp(1.1f, 0f, elapsedTime / dissolveTime);
            //Debug.Log($"Elapsed Time: {elapsedTime}, Dissolve Value: {dissolveValue}, Vertical Value: {verticalValue}");
            foreach (var mat in materials)
            {
                if(useDissolve)
                    mat.SetFloat(_dissolveAmount, dissolveValue);
                if(useVertical)
                    mat.SetFloat(_vertical, verticalValue);
            }
            yield return null;
        }
        materials[0].SetFloat(_dissolveAmount, 1.1f);
        materials[0].SetFloat(_vertical, 0f);
    }

    public IEnumerator Disappear(bool useDissolve, bool useVertical)
    {
        if(useDissolve)
            materials[0].SetFloat(_vertical, -2f);
        if(useVertical)
            materials[0].SetFloat(_dissolveAmount, 1.1f);
        float elapsedTime = 0f;
        //yield return new WaitForSeconds(0.25f);
        Debug.Log("Starting Disappear Coroutine");

        while(elapsedTime < dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(1f, -.2f, elapsedTime / dissolveTime);
            float verticalValue = Mathf.Lerp(0f, 1.1f, elapsedTime / dissolveTime);
            //Debug.Log($"Elapsed Time: {elapsedTime}, Dissolve Value: {dissolveValue}, Vertical Value: {verticalValue}");
            foreach (var mat in materials)
            {
                if(useDissolve)
                    mat.SetFloat(_dissolveAmount, dissolveValue);
                if(useVertical)
                    mat.SetFloat(_vertical, verticalValue);
            }
            yield return null;
        }
        materials[0].SetFloat(_dissolveAmount, -0.2f);
        materials[0].SetFloat(_vertical, 1.1f);
    }


}
