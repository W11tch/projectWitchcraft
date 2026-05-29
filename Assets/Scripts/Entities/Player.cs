using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Entities
{
    // Player entity component — sits alongside PlayerMovement and PlayerController
    // on the Player GameObject. Handles entity identity and GameReferences registration.
    public class Player : Character
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}
