using System.Collections;
using UnityEngine;

public class BasicButton : MonoBehaviour
{
    Animator _animator;

    public delegate void ButtonHover();
    public ButtonHover onButtonHover;

    bool _isPointerOver = false;
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void OnPointerEnter()
    {
        _isPointerOver = true;
        onButtonHover?.Invoke();

        _animator.SetBool("PointerOver", true);
        // Play shine animation
        _animator.Play("ButtonShine");

        ScaleButtonUpCaller();
    }

    public void OnPointerExit()
    {
        _isPointerOver = false;

        _animator.SetBool("PointerOver", false);

        ScaleButtonDownCaller();
    }

    void ScaleButtonUpCaller()
    {
       // StopCoroutine(nameof(ScaleButtonDown));
        StartCoroutine(ScaleButtonUp(0.1f));
    }

    void ScaleButtonDownCaller()
    {
        //StopCoroutine(nameof(ScaleButtonUp));
        StartCoroutine(ScaleButtonDown(0.5f));
    }

    IEnumerator ScaleButtonUp(float time)
    {
        float totalTime = time;
        while (time > 0 && _isPointerOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 1.1f, (1 - (time / totalTime)));
            time -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator ScaleButtonDown(float time)
    {
        float totalTime = time;
        while (time > 0 && !_isPointerOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, (1 - (time / totalTime)));
            time -= Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
