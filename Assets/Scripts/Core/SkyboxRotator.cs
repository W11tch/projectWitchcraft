using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public class SkyboxRotator : MonoBehaviour
    {
        // Speed of the skybox rotation
        public float rotationSpeed = 1.0f;

        void Update()
        {
            // Rotate the skybox by modifying its "_Rotation" property
            // This is a material property specific to some skybox shaders
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
            }
        }
    }
}