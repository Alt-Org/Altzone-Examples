using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public GameObject[] shield;

    private int _currentCount;

    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ball")
        {
            yield return new WaitForSeconds(.5f);
            _currentCount++;
            Debug.Log("Current hit count " + _currentCount.ToString());
        }
        if (_currentCount >= 1)
        {
            shield[0].SetActive(false);
            shield[1].SetActive(true);
            _currentCount = 0;
        }
    }
}
