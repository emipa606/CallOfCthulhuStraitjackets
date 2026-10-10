# GitHub Copilot Instructions for "Call of Cthulhu - Straitjackets (Continued)"

## Mod Overview and Purpose
The "Call of Cthulhu - Straitjackets (Continued)" mod is an update to Jecrell's original mod, further refined by Santa. This mod introduces straitjackets to RimWorld, which can be used to manage colonists experiencing mental breaks. The main purpose of the mod is to prevent colonists from causing harm to themselves or others during these episodes by equipping them with a straitjacket.

## Key Features and Systems
- **Straitjackets:** When equipped, they significantly suppress the likelihood of mental breaks in colonists with a 95% chance of prevention.
- **Stat Modifiers:** Wearing a straitjacket decreases the global work speed by 98%, manipulation to 0%, and movement speed by 70%.
- **Crafting:** Straitjackets can be crafted at tailoring stations.
- **Language Support:** This mod supports both English and French translations.
- **Dependencies:** Requires JecsTools for proper functionality.
- **Powered by Harmony:** Utilizes the Harmony library to ensure compatibility with other mods.

## Coding Patterns and Conventions
- **C# Conventions:**
  - Follow standard C# coding practices such as PascalCase for methods and properties, camelCase for local variables, and ALL_CAPS for constant values.
  - Utilize regions to organize code sections meaningfully, such as `#region Fields` and `#region Methods`.

- **XML Conventions:**
  - Adhere to proper indentation for readability.
  - Define unique defNames for mod elements to avoid potential conflicts.
  - Include comprehensive comments in XML files to describe the purpose and usage of different elements.

## XML Integration
The mod makes extensive use of XML files to define game elements like apparel, jobs, thoughts, and hediffs. Key XML files include:
- **Straitjacket_Hediff.xml:** Defines the Hediff associated with wearing a straitjacket.
- **Straitjacket_Jobs.xml:** Configures job definitions for actions like equipping and unequipping straitjackets.
- **StraitJacket_Apparel.xml:** Details the properties and statistics of the straitjacket apparel.
- **Thoughts_Memory_Straitjacket.xml:** Manages the thoughts and memories linked to wearing a straitjacket.

## Harmony Patching
Harmony is leveraged to ensure the mod integrates seamlessly with the base game and other mods. Important considerations include:
- **Safe Code Injection:** Use Harmony patches for methods that control apparel equipping and mental state checks.
- **Patching Practices:** Employ prefix and postfix methods to adjust game behavior without overwriting core functionality.

## Suggestions for Copilot
- **Automate Repetitive Tasks:** Set up Copilot to generate boilerplate Harmony patches, XML def templates, and C# classes with common methods like `MakeNewToils`.
- **Error Handling Examples:** Request examples of error reporting and debugging methods to enhance mod stability.
- **Pattern Suggestions:** Encourage Copilot to suggest efficient coding patterns or refactoring opportunities, based on existing mod source files.
- **Language Localizations:** Utilize Copilot to suggest and autocomplete translations for new features added to the mod.

---

For further inquiries or support issues, remember to use the provided Discord channel for error reporting, and always share logs using the Log Uploader. Direct problem-solving suggestions to the GitHub repository where they benefit the community most.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

