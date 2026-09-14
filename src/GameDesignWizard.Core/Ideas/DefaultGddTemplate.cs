namespace GameDesignWizard.Core.Ideas;

public static class DefaultGddTemplate
{
    public static IReadOnlyList<GddTemplateSection> Sections { get; } =
    [
        new("Design Pillars", "Define the few principles that every major design decision should support.", 0),
        new("Target Platform and Audience", "Describe the intended players, play context, and platform-specific constraints.", 1),
        new("Genre and Themes", "Explain the genre promise, tone, themes, and emotional goals.", 2),
        new("Core Gameplay Loop", "Describe what the player repeatedly does and why the loop remains engaging.", 3),
        new("Mechanics and Systems", "Explain the rules, interactions, resources, and connected systems.", 4),
        new("Progression", "Describe how challenge, abilities, content, and player mastery develop over time.", 5),
        new("Characters", "Record important characters, roles, motivations, and relationships.", 6),
        new("World and Lore", "Describe the setting, locations, history, factions, and world rules.", 7),
        new("Art Direction", "Define the visual language, mood, readability goals, and key references.", 8),
        new("Technical Features", "Record important technical requirements, tools, integrations, and constraints.", 9),
        new("Production Scope", "Set boundaries, milestones, priorities, and assumptions for production.", 10),
        new("Risks and Open Questions", "Track unresolved decisions, design risks, and validation work.", 11)
    ];
}

public sealed record GddTemplateSection(string TitleEnglish, string GuidanceEnglish, int SortOrder);
