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
public enum CardSuit { None, Red, Green, Blue, Brown }

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

public class CombinationRule
{
    public string CombinationType { get; }

    public CombinationRule(string combinationType)
    {
        CombinationType = combinationType;
    }
}

[Serializable]
public class CombinationRule1
{
    public string CombinationType { get; }
    public int NumberOfCardsRequired { get; }
    public Func<List<Card>, bool> CheckCondition { get; }
    public Action<List<Card>> Action { get; }

    public CombinationRule1 (string combinationType, int numberOfCardsRequired, Func<List<Card>, bool> checkCondition, Action<List<Card>> action)
    {
        CombinationType = combinationType;
        NumberOfCardsRequired = numberOfCardsRequired;
        CheckCondition = checkCondition;
        Action = action;
    }

    //#region Define Rule
    //List<CombinationRule> pairRules = new()
    //{
    //    new CombinationRule("Pair", 2, (hand) => HasPair(hand), (hand) => CollectPair(hand)),
    //    new CombinationRule("Triplet", 3, (hand) => HasTriplet(hand), (hand) => CollectTriplet(hand))
    //};

    //private void AILogic(List<Card> aiHand)
    //{
    //    foreach (CombinationRule rule in pairRules)
    //    {
    //        if (rule.CheckCondition(aiHand))
    //        {
    //            rule.Action(aiHand);
    //            break;
    //        }
    //    }
    //}

    //private static bool HasPair(List<Card> hand)
    //{
    //    var rankCounts = new Dictionary<int, int>();
    //    foreach (var card in hand)
    //    {
    //        if (!rankCounts.ContainsKey(card.Rank))
    //        {
    //            rankCounts[card.Rank] = 1;
    //        }
    //        else
    //        {
    //            rankCounts[card.Rank]++;
    //            if (rankCounts[card.Rank] == 2)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //private static bool HasTriplet(List<Card> hand)
    //{
    //    var rankCounts = new Dictionary<int, int>();
    //    foreach (var card in hand)
    //    {
    //        if (!rankCounts.ContainsKey(card.Rank))
    //        {
    //            rankCounts[card.Rank] = 1;
    //        }
    //        else
    //        {
    //            rankCounts[card.Rank]++;
    //            if (rankCounts[card.Rank] == 2)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //private static void CollectPair(List<Card> hand)
    //{

    //}

    //private static void CollectTriplet(List<Card> hand)
    //{

    //}
    //#endregion
}
