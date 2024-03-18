﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{

    private bool mFaded = false;

    public float Duration = 0.4f;

    public void Fade()
    {
        var canvGroup = GetComponent<CanvasGroup>();

        StartCoroutine(DoFade(canvGroup, canvGroup.alpha, 0));

        //Toggling the faded state
        //mFaded = !mFaded;
    }

    public IEnumerator DoFade (CanvasGroup canvGroup, float start, float end)
    {
        float counter = 0f;

        while (counter < Duration)
        {
            counter += Time.deltaTime;
            canvGroup.alpha = Mathf.Lerp(start, end, counter / Duration);

            yield return null;
        }
    }

}