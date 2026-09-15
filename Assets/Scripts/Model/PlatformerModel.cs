using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Model
{
    
    [System.Serializable]
    public class PlatformerModel
    {
        public Unity.Cinemachine.CinemachineCamera virtualCamera;

              public PlayerController player;

     
        public Transform spawnPoint;

       
        public float jumpModifier = 1.5f;

        
        public float jumpDeceleration = 0.5f;

    }
}