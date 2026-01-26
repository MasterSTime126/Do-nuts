using System.Collections;
using UnityEngine;

public class MaskManager : MonoBehaviour
{

    [SerializeField] private PlayerHP playerHP;

    private int maskState = 0;

    private void Start()
    {
        StartCoroutine(MaskEnumerator());
    }

    private void ChangeTheMask()
    {
        // Example check before changing the mask state (adjust condition as needed)
        if (maskState >= 0)
        {
            maskState++;
            playerHP.UpdateTheMaskState(maskState);
        }
    }

    public int GetMaskState()
    {
        return maskState;
    }
    
    private IEnumerator MaskEnumerator()
    {
        while (maskState < 5)
        {
            yield return new WaitForSeconds(15f);
            ChangeTheMask();
        }
    }
}
