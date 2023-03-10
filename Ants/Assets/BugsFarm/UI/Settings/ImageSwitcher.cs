namespace BugsFarm.UI
{
    public class ImageSwitcher : Switcher<ImageSwitchData>
    {
        public override void Init(){}

        protected override void ToSwitch(ImageSwitchData data, int state)
        {
            var sprite = data.SwitchList[state];
            data.TargetObject.sprite = sprite;
        }
    }
}
