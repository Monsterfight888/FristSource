using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class AimingRetical : MonoBehaviour
{
    
    public RectTransform RightRetical;
    public RectTransform LeftRetical;
    public RectTransform TopRetical;
    public RectTransform BottomRetical;
    public RectTransform topLeftTickMark;
    public RectTransform topRightTickMark;
    public RectTransform bottomLeftTickMark;
    public RectTransform bottomRightTickMark;
    public float tickX;
    public float tickY;
    [Range(0.0f, 1.0f)]
    public float scale;
    public bool tickMarkScaling = false;
    public Coroutine tickCor;
    public float scaleModdifier;
    void Start()
    {

        topLeftTickMark.gameObject.SetActive(false);
        topRightTickMark.gameObject.SetActive(false);
        bottomLeftTickMark.gameObject.SetActive(false);
        bottomRightTickMark.gameObject.SetActive(false);
    }
    public IEnumerator ReticleHit(float time)
    {
        topLeftTickMark.gameObject.SetActive(true);
        topRightTickMark.gameObject.SetActive(true);
        bottomLeftTickMark.gameObject.SetActive(true);
        bottomRightTickMark.gameObject.SetActive(true);

        yield return new WaitForSeconds(time);

        topLeftTickMark.gameObject.SetActive(false);
        topRightTickMark.gameObject.SetActive(false);
        bottomLeftTickMark.gameObject.SetActive(false);
        bottomRightTickMark.gameObject.SetActive(false);
        tickCor = null;
    }
    public void ReticleSet(bool set)
    {
        topLeftTickMark.gameObject.SetActive(set);
        topRightTickMark.gameObject.SetActive(set);
        bottomLeftTickMark.gameObject.SetActive(set);
        bottomRightTickMark.gameObject.SetActive(set);
    }

    // Update is called once per frame
    void Update()
    {

        float t_scale = scale * 4 + scaleModdifier;
        RightRetical.localPosition = new Vector2(t_scale, 0);
        LeftRetical.localPosition = new Vector2(-t_scale, 0);
        TopRetical.localPosition = new Vector2(0, t_scale);
        BottomRetical.localPosition = new Vector2(0, -t_scale);
        if (tickMarkScaling)
        {
            topLeftTickMark.localPosition = new Vector2(t_scale - tickX, t_scale - tickY);
            topRightTickMark.localPosition = new Vector2(-t_scale + tickX, t_scale - tickY);
            bottomLeftTickMark.localPosition = new Vector2(t_scale - tickX, -t_scale + tickY);
            bottomRightTickMark.localPosition = new Vector2(-t_scale + tickX, -t_scale + tickY);
        }
        //X.localScale = new Vector2(testScale, testScale);
    }
}
