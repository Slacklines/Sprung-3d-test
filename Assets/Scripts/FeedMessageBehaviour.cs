using System.Collections;
using TMPro;
using UnityEngine;

public class FeedMessageBehaviour : MonoBehaviour
{

    public TMP_Text text;
    private CanvasGroup cg;

    public float visibleTime = 3f;
    public float fadeTime = 1f;

    public void Initialize(string message)
    {
        text.text = message;
    }

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        yield return new WaitForSeconds(visibleTime);

        float timer = 0;

        while (timer < fadeTime)
        {
            timer+= Time.deltaTime;
            cg.alpha = 1 - (timer/fadeTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
