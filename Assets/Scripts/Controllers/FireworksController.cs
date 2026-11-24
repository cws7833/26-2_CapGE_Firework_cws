using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FireworksController : MonoBehaviour
{
    [Serializable]
    private sealed class Phase
    {
        [Min(0)] public float startSeconds;
        public ParticleSystem[] fireworks = Array.Empty<ParticleSystem>();
    }

    [SerializeField] private AudioSource music;
    [SerializeField] private Phase[] phases = Array.Empty<Phase>();

    private readonly List<ParticleSystem> active = new();
    private int nextPhase;
    private double startTime;

    private void Start()
    {
        Array.Sort(phases, (a, b) => a.startSeconds.CompareTo(b.startSeconds));
        startTime = AudioSettings.dspTime + 0.1;
        if (music != null && music.clip != null)
            music.PlayScheduled(startTime);
    }

    private void Update()
    {
        double seconds = AudioSettings.dspTime - startTime;
        if (music != null && music.clip != null && seconds >= music.clip.length)
        {
            StopPhase();
            enabled = false;
            return;
        }

        while (nextPhase < phases.Length && phases[nextPhase].startSeconds <= seconds)
        {
            StopPhase();
            foreach (ParticleSystem prefab in phases[nextPhase].fireworks)
            {
                if (prefab == null) continue;
                ParticleSystem effect = Instantiate(prefab, transform.position, transform.rotation, transform);
                effect.Play();
                active.Add(effect);
            }
            nextPhase++;
        }
    }

    private void StopPhase()
    {
        foreach (ParticleSystem effect in active)
            if (effect != null) Destroy(effect.gameObject);
        active.Clear();
    }
}
