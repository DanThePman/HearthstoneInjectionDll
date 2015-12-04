using System;

public interface ISelectableTouchListItem : ITouchListItem
{
    bool IsSelected();
    void Selected();
    void Unselected();

    bool Selectable { get; }
}

