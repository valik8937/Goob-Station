using System;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._Pirate.UserInterface.Controls;

/// <summary>
/// A reusable collapsible section with framed background and styled heading.
/// Designed for menus that need card-like grouped lists.
/// </summary>
public sealed class FramedCollapsible : PanelContainer
{
    private const string ChevronCollapsedIconPath = "/Textures/Interface/Nano/triangle_right.png";
    private const string ChevronExpandedIconPath = "/Textures/Interface/Nano/inverted_triangle.svg.png";
    private static readonly System.Numerics.Vector2 ChevronSize = new(12, 12);

    public event Action<bool>? OnToggled;

    public CollapsibleBody Body { get; }

    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
                return;
            _expanded = value;
            ApplyExpandedState();
        }
    }

    public bool IsEmptySection
    {
        get => _isEmptySection;
        set
        {
            if (_isEmptySection == value)
                return;
            _isEmptySection = value;
            ApplyExpandedState();
        }
    }

    private readonly Collapsible _collapsible;
    private readonly CollapsibleHeading _heading;
    private readonly TextureRect? _chevronIcon;
    private bool _expanded;
    private bool _isEmptySection;

    public FramedCollapsible(string title)
    {
        HorizontalExpand = true;
        PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = new Color(0.11f, 0.11f, 0.13f, 0.92f),
            ContentMarginLeftOverride = 5,
            ContentMarginRightOverride = 5,
            ContentMarginTopOverride = 5,
            ContentMarginBottomOverride = 5,
        };

        _heading = new CollapsibleHeading(title)
        {
            HorizontalExpand = true,
            StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = new Color(0.17f, 0.17f, 0.20f, 0.95f),
                ContentMarginLeftOverride = 5,
                ContentMarginRightOverride = 5,
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 4,
            }
        };
        _heading.Label.HorizontalExpand = true;
        _heading.Label.ClipText = true;
        _chevronIcon = FindChevronIcon(_heading);
        if (_chevronIcon != null)
        {
            _chevronIcon.MinSize = ChevronSize;
            _chevronIcon.SetSize = ChevronSize;
            _chevronIcon.Stretch = TextureRect.StretchMode.KeepCentered;
        }

        Body = new CollapsibleBody
        {
            Margin = new Thickness(0, 5, 0, 0),
        };

        _collapsible = new Collapsible(_heading, Body)
        {
            HorizontalExpand = true,
        };
        AddChild(_collapsible);

        _heading.OnToggled += args =>
        {
            _expanded = args.Pressed;
            ApplyExpandedState();
            OnToggled?.Invoke(Expanded);
        };

        ApplyExpandedState();
    }

    private void ApplyExpandedState()
    {
        // Keep chevron state in sync with expanded/collapsed regardless of section contents.
        _collapsible.BodyVisible = _expanded;

        if (_chevronIcon != null)
            _chevronIcon.TexturePath = _expanded ? ChevronExpandedIconPath : ChevronCollapsedIconPath;

        // For empty sections we keep body hidden to avoid vertical twitching on toggle.
        Body.Visible = !_isEmptySection && _expanded;
    }

    private static TextureRect? FindChevronIcon(Control control)
    {
        foreach (var child in control.Children)
        {
            if (child is TextureRect textureRect)
                return textureRect;

            var nested = FindChevronIcon(child);
            if (nested != null)
                return nested;
        }

        return null;
    }
}
