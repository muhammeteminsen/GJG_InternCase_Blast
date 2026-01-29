using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public enum FeedbackType
{
    Shuffle,
    NotMatchable,
    GravityActive,
    GravityComplete,
    Matchable,
    BigMatch
}
public class FeedbacksManager : MonoBehaviour
{
    [Serializable]
    public class Feedbacks
    {
        public FeedbackType feedbackType;
        public MMFeedbacks feedbacks;
    }
   [SerializeField] private List<Feedbacks> feedbacks;

   private readonly Dictionary<FeedbackType, MMFeedbacks> _feedbackDict = new Dictionary<FeedbackType, MMFeedbacks>();
   private void Awake()
   {
       foreach (var feedback in feedbacks)
           if (!_feedbackDict.ContainsKey(feedback.feedbackType))
                _feedbackDict.Add(feedback.feedbackType, feedback.feedbacks);
   }

   public void PlayFeedbacks(FeedbackType feedbackType)
   {
       if (_feedbackDict.TryGetValue(feedbackType, out MMFeedbacks mmFeedbacks))
       {
           mmFeedbacks?.PlayFeedbacks();
       }
       else
       {
           Debug.LogWarning("Feedback Type: " + feedbackType + " does not exist");
       }
   }
   
}
