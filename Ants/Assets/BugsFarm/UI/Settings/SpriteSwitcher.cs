namespace BugsFarm.UI
{
    public class SpriteSwitcher : Switcher<SpriteSwitchData>
    {
        public override void Init(){}

        protected override void ToSwitch(SpriteSwitchData data, int state)
        {
            var sprite = data.SwitchList[state];
            data.TargetObject.sprite = sprite;
        }
    }
}
