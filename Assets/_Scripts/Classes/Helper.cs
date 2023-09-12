using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.Events;

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
public enum CardColor { None, Yellow, Red, Green, Blue }

[Serializable]
public enum CardCorners { None, Zero, Three, Four, Five }

[Serializable]
public enum CardSuit { None, Pentagone, Squre, Triangle, Circle }

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
public class CombinationBase
{
    public int[] cardIDs;
    public int CardNumber { get; set; }
    public CardSuit CardSuit { get; set; }

    public CombinationBase(int arrayCount)
    {
        cardIDs = new int[arrayCount];
        ClearIDs();
    }

    public void ClearIDs()
    {
        Array.Fill(cardIDs, -1);
    }
}

[Serializable]
public class Twin : CombinationBase
{
    public Twin() : base(2) { }
}

[Serializable]
public class Triplet : CombinationBase
{
    public Triplet() : base(3) { }
}

[Serializable]
public class Quadruplet : CombinationBase
{
    public Quadruplet() : base(4) { }
}

[Serializable]
public class Street : CombinationBase
{
    public Street() : base(8) { }
}

[Serializable]
public class Wengla : CombinationBase
{
    public Wengla() : base(12) { }
}

[Serializable]
public class CombinationContainer
{
    public List<CombinationBase> Combinations { get; set; }

    public CombinationContainer()
    {
        Combinations = new List<CombinationBase>();
    }
}

[Serializable]
public class EventAndResponse
{
    public string name;
    public GameEvent gameEvent;
    public UnityEvent genericResponse;
    public UnityEvent<int> sentIntResponse;
    public UnityEvent<bool> sentBoolResponse;
    public UnityEvent<float> sentFloatResponse;
    public UnityEvent<string> sentStringResponse;

    public void EventRaised()
    {
        if (genericResponse.GetPersistentEventCount() >= 1) { genericResponse.Invoke(); }

        if (sentIntResponse.GetPersistentEventCount() >= 1) { sentIntResponse.Invoke(gameEvent.sentInt); }

        if (sentBoolResponse.GetPersistentEventCount() >= 1) { sentBoolResponse.Invoke(gameEvent.sentBool); }

        if (sentFloatResponse.GetPersistentEventCount() >= 1) { sentFloatResponse.Invoke(gameEvent.sentFloat); }

        if (sentStringResponse.GetPersistentEventCount() >= 1) { sentStringResponse.Invoke(gameEvent.sentString); }
    }
}

//public int[] cardIDs = new int[2];
//public int CardNumber { get; set; }
//public CardSuit CardSuit { get; set; }

//public void ClearIDs()
//{
//    Array.Fill(cardIDs, -1);
//}

//public int[] cardIDs = new int[3];
//public int CardNumber { get; set; }
//public CardSuit CardSuit { get; set; }

//public void ClearIDs()
//{
//    Array.Fill(cardIDs, -1);
//}

//public int[] cardIDs = new int[4];
//public int CardNumber { get; set; }
//public CardSuit CardSuit { get; set; }

//public void ClearIDs()
//{
//    Array.Fill(cardIDs, -1);
//}

//public int[] cardIDs = new int[8];
//public CardSuit CardSuit { get; set; }

//public void ClearIDs()
//{
//    Array.Fill(cardIDs, -1);
//}

//public int[] cardIDs = new int[12];
//public int CardNumber { get; set; }

//public void ClearIDs()
//{
//    Array.Fill(cardIDs, -1);
//}
