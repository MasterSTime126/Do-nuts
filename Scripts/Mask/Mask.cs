using UnityEngine;

public class Mask
{
    public Sprite maskSprite;
    public maskLogic logicType; // Add this field

    public enum maskLogic
    {
        HAPPINESS,
        SADNESS,
        ANGER,
        FEAR,
        DISGUST
    };
}
