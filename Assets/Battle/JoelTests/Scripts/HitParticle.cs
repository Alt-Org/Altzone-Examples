using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticle : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    public AudioSource audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag(UnityConstants.Tags.Ball))
        {
            audioSource.PlayOneShot(audioSource.clip);
            Debug.Log("HIT!");
            foreach (ContactPoint2D hitPos in collision.contacts)
            {
                Vector2 hitPoint = hitPos.point;
                Instantiate(ps, new Vector3(hitPoint.x, hitPoint.y, 0), Quaternion.identity);
                ps.Play();
                Debug.Log(" x POS " + hitPoint.x.ToString() + " y POS " + hitPoint.y.ToString());
            }
        }
    }
}
