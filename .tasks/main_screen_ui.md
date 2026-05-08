# Task: Main Screen UI Generation
**Agent:** `@mobile-developer`
**Status:** ⏳ Blocked on Socratic Gate (Waiting for User Clarification)

## Specifiations
- **Style:** Mobile Portrait UI
- **Root Canvas:** Screen Space - Overlay, Scale With Screen Size (1080x1920), Match: 0.5
- **Anchors & Layouts:** Detailed presets for Top Bar, Background, Castle, Characters, Bottom Bar, Sidebars, and Bottom Nav.
- **Advanced:** Safe Area, Content Size Fitter, Pulse Animation for Action Buttons.

## Implementation Plan
1.  **Clear Socratic Gate:** Ask user edge-case questions.
2.  **Editor Script:** Generate an Editor script `Assets/Editor/MainScreenGenerator.cs` that automates building the hierarchy exactly to spec.
3.  **Components:** Attach standard Unity `RectTransform`, `Image`, `VerticalLayoutGroup`, and `TextMeshProUGUI` components automatically.
4.  **SafeArea Script:** Create and attach a `SafeArea.cs` behaviour to adjust the layout group inner-padding or parent container for iPhone notch support.
