#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ArtAssignmentTool
{
    [MenuItem("Ants of War/Assign Art Assets")]
    static void Assign()
    {
        var bullet = AssetDatabase.LoadAssetAtPath<SpeciesData>(
            "Assets/Data/Species/Bullet Ants.asset");
        var fire = AssetDatabase.LoadAssetAtPath<SpeciesData>(
            "Assets/Data/Species/Fire Ants.asset");
        var army = AssetDatabase.LoadAssetAtPath<SpeciesData>(
            "Assets/Data/Species/Army Ants.asset");
        var weaver = AssetDatabase.LoadAssetAtPath<SpeciesData>(
            "Assets/Data/Species/Weaver Ants.asset");

        AssignSpecies(bullet, "bulletant.aseprite", "anthill 1.aseprite");
        AssignSpecies(fire, "trapjawant.aseprite", "anthill2.aseprite");
        AssignSpecies(army, "turtleant.aseprite", "anthill2.aseprite");
        AssignSpecies(weaver, "weaverant.aseprite", "anthill 1.aseprite");

        AssetDatabase.SaveAssets();
        Debug.Log("Ants of War art assets assigned.");
    }

    static void AssignSpecies(SpeciesData species, string antName, string hillName)
    {
        if (species == null) return;
        species.antSprite = LoadSprite(antName);
        species.hillSprite = LoadSprite(hillName);
        EditorUtility.SetDirty(species);
    }

    static Sprite LoadSprite(string fileName)
    {
        return AssetDatabase.LoadAllAssetsAtPath("Assets/Art/" + fileName)
            .OfType<Sprite>()
            .FirstOrDefault();
    }
}
#endif
