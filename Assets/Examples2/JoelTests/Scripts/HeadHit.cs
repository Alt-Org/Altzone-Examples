using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadHit : MonoBehaviour
{
    private int _currentCount;
    public GameObject[] backGround;

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
            backGround[0].SetActive(false);
            backGround[1].SetActive(true);
        }
        if (_currentCount >= 2)
        {
            backGround[1].SetActive(false);
            backGround[2].SetActive(true);
        }
        if (_currentCount >= 3)
        {
            backGround[2].SetActive(false);
            backGround[3].SetActive(true);
        }
        if (_currentCount >= 4)
        {
            backGround[3].SetActive(false);
            backGround[4].SetActive(true);
        }
    }
}
