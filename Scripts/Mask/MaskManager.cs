using UnityEngine;

public class MaskManager : MonoBehaviour
{

    [SerializeField] private PlayerHP playerHP;

    private int maskState = 0;

    public int GetMaskState()
    {
        return maskState;
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
}
