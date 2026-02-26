using UnityEngine;

public class DonutAnimator : MonoBehaviour
{
    [Header("Happiness Sprites")]
    [SerializeField] private Sprite happinessIdle;

    [Header("Sadness Sprites")]
    [SerializeField] private Sprite[] sadnessWalk = new Sprite[2];
    [SerializeField] private Sprite sadnessPause;

    [Header("Fear Sprites")]
    [SerializeField] private Sprite fearIdle;

    [Header("Anger Sprites")]
    
    [SerializeField] private Sprite[] angerWalk = new Sprite[2];
    [SerializeField] private Sprite[] angerAttack = new Sprite[3];

    [Header("Disgust Sprites")]
    [SerializeField] private Sprite[] disgustWalk = new Sprite[2];
    [SerializeField] private Sprite disgustExplosion;

    [Header("Animation Settings")]
    [SerializeField] private float walkFrameRate = 0.2f;
    [SerializeField] private float attackFrameRate = 0.15f;

    private SpriteRenderer spriteRenderer;
    private MaskManager maskManager;
    private DonutLogic donutLogic;

    private float animationTimer = 0f;
    private int currentFrame = 0;
    private bool isAttacking = false;
    private bool isPaused = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maskManager = MaskManager.Instance;
        donutLogic = GetComponent<DonutLogic>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        SetInitialSprite();
    }

    private void Update()
    {
        if (maskManager == null || spriteRenderer == null) return;

        animationTimer += Time.deltaTime;

        switch (maskManager.GetMaskState())
        {
            case MaskManager.MaskState.Happiness:
                AnimateHappiness();
                break;
            case MaskManager.MaskState.Sadness:
                AnimateSadness();
                break;
            case MaskManager.MaskState.Fear:
                AnimateFear();
                break;
            case MaskManager.MaskState.Anger:
                AnimateAnger();
                break;
            case MaskManager.MaskState.Disgust:
                AnimateDisgust();
                break;
        }
    }

    private void SetInitialSprite()
    {
        if (maskManager == null || spriteRenderer == null) return;

        switch (maskManager.GetMaskState())
        {
            case MaskManager.MaskState.Happiness:
                if (happinessIdle != null) spriteRenderer.sprite = happinessIdle;
                break;
            case MaskManager.MaskState.Sadness:
                if (sadnessWalk.Length > 0 && sadnessWalk[0] != null) spriteRenderer.sprite = sadnessWalk[0];
                break;
            case MaskManager.MaskState.Fear:
                if (fearIdle != null) spriteRenderer.sprite = fearIdle;
                break;
            case MaskManager.MaskState.Anger:
                if (angerWalk.Length > 0 && angerWalk[0] != null) spriteRenderer.sprite = angerWalk[0];
                break;
            case MaskManager.MaskState.Disgust:
                if (disgustWalk.Length > 0 && disgustWalk[0] != null) spriteRenderer.sprite = disgustWalk[0];
                break;
        }
    }

    #region Animation Methods
    private void AnimateHappiness()
    {
        if (happinessIdle != null)
        {
            spriteRenderer.sprite = happinessIdle;
        }
    }

    private void AnimateSadness()
    {
        if (isPaused)
        {
            if (sadnessPause != null)
            {
                spriteRenderer.sprite = sadnessPause;
            }
        }
        else
        {
            PlayWalkAnimation(sadnessWalk);
        }
    }

    private void AnimateFear()
    {
        if (fearIdle != null)
        {
            spriteRenderer.sprite = fearIdle;
        }
    }

    private void AnimateAnger()
    {
        if (isAttacking)
        {
            PlayAttackAnimation(angerAttack);
        }
        else
        {
            PlayWalkAnimation(angerWalk);
        }
    }

    private void AnimateDisgust()
    {
        PlayWalkAnimation(disgustWalk);
    }

    private void PlayWalkAnimation(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;

        if (animationTimer >= walkFrameRate)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % sprites.Length;
            
            if (sprites[currentFrame] != null)
            {
                spriteRenderer.sprite = sprites[currentFrame];
            }
        }
    }

    private void PlayAttackAnimation(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;

        if (animationTimer >= attackFrameRate)
        {
            animationTimer = 0f;
            currentFrame++;
            
            if (currentFrame >= sprites.Length)
            {
                currentFrame = 0;
                isAttacking = false;
                return;
            }
            
            if (sprites[currentFrame] != null)
            {
                spriteRenderer.sprite = sprites[currentFrame];
            }
        }
    }
    #endregion

    #region Public Methods
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (paused)
        {
            currentFrame = 0;
            animationTimer = 0f;
        }
    }

    public void TriggerAttack()
    {
        if (maskManager != null && maskManager.GetMaskState() == MaskManager.MaskState.Anger)
        {
            isAttacking = true;
            currentFrame = 0;
            animationTimer = 0f;
        }
    }

    public void TriggerExplosion()
    {
        if (disgustExplosion != null)
        {
            spriteRenderer.sprite = disgustExplosion;
        }
    }
    #endregion
}
