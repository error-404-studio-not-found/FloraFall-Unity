using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int nuts = 0;
    private TextMeshProUGUI nutText;


    private void Start()
    {
        nutText = GetComponent<TextMeshProUGUI>();
        if (nutText != null)
        {
            nutText.text = nuts.ToString();
        }   
    }

    public void gainBolts(int amount)
    {
        int start = nuts;
        nuts += amount;

        StopAllCoroutines();
        StartCoroutine(counter(start, nuts));
    }
    public void looseBolts(int amount)
    {
        int start = nuts;
        nuts -= amount;

        StopAllCoroutines();
        StartCoroutine(counter(start, nuts));
    }
    public void setBolts(int amount)
    {
        nuts = amount;
    }

    private IEnumerator counter(int currentNuts, int endNuts)
    {
      int step = currentNuts < endNuts ? 1 : -1;

        for (int i = currentNuts; step > 0 ? i < endNuts : i > endNuts; i += step)
        {
            nutText.text = i.ToString();
            yield return new WaitForSeconds(0.04f);
        }
       
        nutText.text = nuts.ToString();
    }

}
