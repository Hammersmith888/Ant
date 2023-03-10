using BugsFarm.Game;
using BugsFarm.Views.Fight.Ui;
using UnityEngine;

public static class SquadSelectSlots
{
    static SquadSelectSlot HoveredSlot => SquadSelectSlot.HoveredSlot;


    public static readonly BiDict<SquadSelectSlot, SquadSelectItemView> Slots =
        new BiDict<SquadSelectSlot, SquadSelectItemView>();


    public static void Clear()
    {
        foreach (var pair in Slots)
            GameObject.Destroy(pair.Value.gameObject);

        Slots.Clear();
    }


    public static bool MoveTo(SquadSelectItemView itemView)
    {
        if (SquadSelectSlot.HoveredSlot)
        {
            MoveToSlot(itemView);
            return true;
        }

        ReturnBack(itemView);
        return false;
    }


    static void MoveToSlot(SquadSelectItemView itemView)
    {
        Slots.TryGetValue(HoveredSlot, out SquadSelectItemView occupant);
        Slots.TryGetValue(itemView, out SquadSelectSlot source);

        // Dragged item
        {
            SetSlot(HoveredSlot, itemView);

            GameEvents.OnUnitSelected?.Invoke();
        }

        // Occupant
        if (occupant && occupant != itemView)
        {
            Set(source, occupant, source);
        }
    }


    static void ReturnBack(SquadSelectItemView itemView)
    {
        Slots.TryGetValue(itemView, out SquadSelectSlot source);
        Set(source, itemView, source && !SquadSelectViewPort.Hovered);
    }


    static void Set(SquadSelectSlot slot, SquadSelectItemView itemView, bool condition)
    {
        if (condition)
            SetSlot(slot, itemView);
        else
            SetContent(itemView);
    }


    public static void SetSlot(SquadSelectSlot slot, SquadSelectItemView itemView)
    {
        Slots[slot] = itemView;

        itemView.transform.SetParent(slot.transform);

        itemView.transform.localPosition = Vector3.zero;
    }


    static void SetContent(SquadSelectItemView itemView)
    {
        Slots.Remove(itemView);

        //item.transform.SetParent(UiSquadSelectView.Instance.Content);
    }
}