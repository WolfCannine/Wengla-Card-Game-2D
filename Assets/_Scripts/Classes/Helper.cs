using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public static class Helper
{
    public static void Shuffle<T>(List<T> list)
    {
        System.Random rand = new();
        int n = list.Count;
        while (n > 1)
        {
            int k = rand.Next(n);
            n--;
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}

[Serializable]
public enum Mode { Online, Offline }

[Serializable]
public enum CardSuit { None, Pentagone, Squre, Triangle, Circle }

[Serializable]
public enum CardCorners { None, Zero, Three, Four, Five }

[Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    public AudioMixerGroup output;

    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;

    public bool playOnAwake;

    public bool loop;
}

[Serializable]
public class TransformSet
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformSet(Vector3 position = new(), Quaternion rotation = new(), Vector3 scale = new())
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}

[Serializable]
public class TwinCombination
{
    //public int[] cardIDs = new int[2];
    public int CardNumber { get; set; }
    public CardSuit CardSuit { get; set; }
}

[Serializable]
public class TripletCombination
{
    //public int[] cardIDs = new int[3];
    public int CardNumber { get; set; }
    public CardSuit CardSuit { get; set; }
}

[Serializable]
public class QuadrupletCombination
{
    //public int[] cardIDs = new int[4];
    public int CardNumber { get; set; }
    public CardSuit CardSuit { get; set; }
}

[Serializable]
public class StreetCombination
{
    //public int[] cardIDs = new int[6];
    public CardSuit CardSuit { get; set; }
}

[Serializable]
public class WenglaCombination
{
    //public int[] cardIDs = new int[12];
    public int CardNumber { get; set; }
}
