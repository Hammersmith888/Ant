using System;
using Entitas;

namespace Ecs.Sources.Ui.Components
{
    [Ui]
    public class YesNoComponent : IComponent
    {
        public YesNoData Value;
    }

    public class YesNoData
    {
        public string Header;
        public string Description;
        public int? Price;
        public Action Action;
    }
}