namespace BugsFarm.AnimationsSystem
{
    public struct CustomAnimationClip
    {
        public int Iteration;
        public AnimKey AnimKey;
        public bool Initialized => AnimKey != AnimKey.None;
        public CustomAnimationClip(AnimKey animKey, int iteration)
        {
            Iteration = iteration;
            AnimKey = animKey;
        }
    }
}