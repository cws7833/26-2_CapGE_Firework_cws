using System;
using System.Collections.Generic;
using UnityEngine;

public class FireworksController : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioSource music;
    [SerializeField] private Phase[] phases = Array.Empty<Phase>();
    private readonly List<ParticleSystem> effects = new();
    private double startTime;
    private int nextPhase;
    private bool finished;

    private void Start()
    {
        Array.Sort(phases, (a, b) => a.startSeconds.CompareTo(b.startSeconds));
        startTime = AudioSettings.dspTime + 0.2;
        if (musicClip != null)
        {
            if (music == null) music = gameObject.AddComponent<AudioSource>();
            music.clip = musicClip;
            music.loop = false;
            music.playOnAwake = false;
            music.spatialBlend = 0;
            music.PlayScheduled(startTime);
        }
    }

    private void Update()
    {
        double elapsed = AudioSettings.dspTime - startTime;
        if (!finished && musicClip != null && elapsed >= musicClip.length)
        {
            StopEmission();
            finished = true;
        }
        while (!finished && nextPhase < phases.Length && elapsed >= phases[nextPhase].startSeconds)
            PlayPhase(phases[nextPhase++]);
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i] != null && effects[i].IsAlive(true)) continue;
            if (effects[i] != null) Destroy(effects[i].gameObject);
            effects.RemoveAt(i);
        }
    }

    private void PlayPhase(Phase phase)
    {
        StopEmission();
        foreach (ParticleSystem prefab in phase.fireworks)
        {
            if (prefab == null) continue;
            var effect = Instantiate(prefab, phase.offset, Quaternion.identity, transform);
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.transform.localScale *= phase.scale;
            if (phase.oneShot)
                foreach (var system in effect.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var main = system.main;
                    main.loop = false;
                }
            effect.Play(true);
            effects.Add(effect);
        }
    }

    private void StopEmission()
    {
        foreach (var effect in effects)
            if (effect != null) effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void OnDisable()
    {
        if (music != null) music.Stop();
        foreach (var effect in effects)
            if (effect != null) Destroy(effect.gameObject);
        effects.Clear();
    }
}

[Serializable]
public class Phase
{
    public string name;
    [Min(0)] public float startSeconds;
    public ParticleSystem[] fireworks = Array.Empty<ParticleSystem>();
    public Vector3 offset;
    [Min(0.1f)] public float scale = 1;
    public bool oneShot;
}
