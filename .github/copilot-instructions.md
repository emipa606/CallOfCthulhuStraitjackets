# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

**Mod Name:** Call of Cthulhu - Straitjackets (Continued)  
**Description:** This mod is an update to the original project by Jecrell and follows an enhancement by Santa. It introduces straitjackets to RimWorld, an item that dramatically reduces the risk of mental breaks in colonists. While providing a mostly fail-safe method to prevent colonists from self-harm or harming others, straitjackets come with significant movement and work penalties. This mod is part of the Call of Cthulhu series and requires JecsTools for optimal functionality.

## Key Features and Systems

- **Straitjackets:** An apparel item crafted at tailoring stations. Once equipped, it:
  - Reduces Global Work Speed by 98%
  - Reduces Manipulation to 0%
  - Decreases Move Speed by 70%
  - Increases Mental Break Threshold
  - Provides a 95% chance to disable mental breaks entirely

- **Localization:** Available in English and French (translation contributed by byorh2).

## Coding Patterns and Conventions

- **C# Coding Style:** The project follows standard C# conventions, including PascalCase naming for classes and methods, and camelCase for method parameters.
- **Class Structure:** The project uses a mix of public and internal classes to control accessibility. Key game-related functionalities are encapsulated in appropriately named classes.

## XML Integration

- **Def Files:** The mod uses XML def files to define item attributes and behaviors. Ensure all items are correctly referenced in the XML files to align with C# code logic.

## Harmony Patching

- **Harmony Library:** The mod leverages the Harmony library for runtime modification of game code.
- **Patching Classes:** 
  - `HarmonyStraitJacket` and other classes maintain patches to adjust or extend base game behaviors.
  - Detour patterns are utilized via attributes to modify game logic safely and efficiently.

## Suggestions for Copilot

- **Method Extension:** When adding new features, ensure methods and class extensions follow the existing structure. Place logic in appropriate files like `Utility.cs` or related feature classes.
- **Error Handling:** Implement robust error handling by checking null references and ensuring proper exception reporting.
- **Harmony Patches:** Use Harmony for future patches and adjustments to game behavior. Follow existing examples in `HarmonyStraitJacket.cs`.
- **Cross-File References:** When Copilot generates code, ensure that any cross-file references (e.g., to XML def names) are accurate and existent.

Ensure integration with any third-party tools, such as JecsTools, is reflected both in code and external documentation to maintain compatibility and prevent run-time errors.
