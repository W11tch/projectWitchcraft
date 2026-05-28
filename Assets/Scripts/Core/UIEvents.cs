namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// A static class to hold UI-related events.
    /// </summary>
    public static class UIEvents
    {
        /// <summary>
        /// Event fired when an interaction panel (like a chest) needs to be displayed.
        /// The InventoryPanelController listens to this to hide the stats/equipment panel.
        /// The bool indicates whether to show (true) or hide (false) the interaction panel.
        /// </summary>
        public delegate void InteractionPanelToggle(bool show);
        public static event InteractionPanelToggle OnInteractionPanelToggle;

        public static void ToggleInteractionPanel(bool show)
        {
            OnInteractionPanelToggle?.Invoke(show);
        }
    }
}