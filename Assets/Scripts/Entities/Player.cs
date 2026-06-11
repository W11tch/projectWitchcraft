using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    // Player-identity marker — sits alongside PlayerMovement and PlayerController on the Player
    // GameObject, and is the seam for future remote/multiplayer player identity. The player is its
    // OWN category, deliberately NOT an Entity: it is driven by input/networking and is not tracked
    // or cleared by EntityManager. (GameReferences registration is done by PlayerMovement.)
    public class Player : MonoBehaviour
    {
    }
}
