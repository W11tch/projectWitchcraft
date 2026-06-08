using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.UI
{
    // Debug-only readout of player damage-per-second. Attach inside the debug panel
    // (so it shows/hides with the backquote toggle) and assign a TMP label to _readout.
    // Counts damage dealt to any non-player target (player -> enemy) over a rolling
    // window. Every stat is windowed, so it self-decays to zero when hits stop.
    // Note: filtered by Target rather than Hit.Source (PlayerCombat) to keep the UI
    // assembly from referencing the Player assembly. Equivalent while only the player
    // deals damage; revisit if enemies start damaging each other.
    public class DpsMeterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _readout;
        [SerializeField] private float _windowSeconds = 3f;

        private readonly Queue<(float time, float damage)> _hits = new();

        private void OnEnable()  => EventManager.AddListener<DamagedEvent>(OnDamaged);
        private void OnDisable() => EventManager.RemoveListener<DamagedEvent>(OnDamaged);

        private void OnDamaged(DamagedEvent e)
        {
            Transform player = GameReferences.Instance?.PlayerTransform;
            if (e.Target != null && e.Target != player)
                _hits.Enqueue((Time.time, e.Hit.Damage));
        }

        private void Update()
        {
            float cutoff = Time.time - _windowSeconds;
            while (_hits.Count > 0 && _hits.Peek().time < cutoff)
                _hits.Dequeue();

            float sum = 0f;
            foreach (var hit in _hits)
                sum += hit.damage;

            int count = _hits.Count;
            float dps = sum / _windowSeconds;
            float aps = count / _windowSeconds;
            float avg = count > 0 ? sum / count : 0f;

            if (_readout != null)
                _readout.text = $"DPS {dps:0} | APS {aps:0.0} | avg {avg:0}";
        }
    }
}
