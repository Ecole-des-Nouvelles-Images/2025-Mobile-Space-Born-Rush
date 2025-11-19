using UnityEngine;
public class PlayerPowerUpCollector : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectParticles;
    // [SerializeField] private AudioClip collectSound;
    
    void OnTriggerEnter(Collider other)
    {
        BasePowerUps powerUp = other.GetComponent<BasePowerUps>();
        
        if (powerUp != null)
        {
            // // Play player-specific collection effects
            // if (collectParticles != null)
            //     Instantiate(collectParticles, transform.position, Quaternion.identity);
            //     
            // if (collectSound != null)
            //     AudioSource.PlayClipAtPoint(collectSound, transform.position);
            
            // Apply the power-up effect
            powerUp.ApplyEffect(this.gameObject);
        }
    }
}