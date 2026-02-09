using TMPro;
using UnityEngine;

public class UITextCopier : MonoBehaviour
{
   [SerializeField] private TMP_Text sourceText;
   private TMP_Text targetText;

   private void Start()
   {
       targetText = GetComponent<TMP_Text>();
   }

   private void Update()
   {
       CopyText();
   }

   private void CopyText()
   {
       if (sourceText != null && targetText != null)
       {
           targetText.text = sourceText.text;
       }
   }
}
