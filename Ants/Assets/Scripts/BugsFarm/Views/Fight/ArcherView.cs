using System.Collections.Generic;
using BugsFarm.Views.Core;
using UnityEngine;

namespace BugsFarm.Views.Fight
{
    public class ArcherView : AView
    {
        [SerializeField] private List<ArrowView> arrows = new List<ArrowView>();

        public List<ArrowView> Arrows => arrows;
    }
}