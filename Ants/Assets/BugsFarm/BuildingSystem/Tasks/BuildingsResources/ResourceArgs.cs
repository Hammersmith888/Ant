using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class ResourceArgs
    {
        public AnimKey[] ActionAnimKeys { get; private set;}
        public AnimKey WalkAnimKey { get; private set;}
        public string[] ActionSoundKeys { get; private set;}
        public int RepeatCount { get; private set;}
        private ResourceArgs(){ }

        public static ResourceArgs Default()
        {
            return new ResourceArgs
            {
                RepeatCount = 1, 
                ActionAnimKeys = new AnimKey[0], 
                WalkAnimKey = AnimKey.Walk, 
                ActionSoundKeys = new string[0]
            };
        }
        public ResourceArgs SetActionAnimKeys(params AnimKey[] anims)
        {
            ActionAnimKeys = anims;
            return this;
        }
        public ResourceArgs SetWalkAnimKeys(AnimKey anim)
        {
            WalkAnimKey = anim;
            return this;
        }
        public ResourceArgs SetActionSoundKeys(params string[] sounds)
        {
            ActionSoundKeys = sounds;
            return this;
        }
        public ResourceArgs SetRepeatCounts(int count)
        {
            RepeatCount = count;
            return this;
        }
    }
}