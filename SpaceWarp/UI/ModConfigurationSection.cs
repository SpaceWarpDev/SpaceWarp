using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
namespace SpaceWarp.UI;

public class ModConfigurationSection
{
    public bool Open = false;
        
    public readonly List<(string name, FieldInfo info, object confAttribute, string currentStringValue)> Properties = new List<(string name, FieldInfo info, object confAttribute, string currentStringValue)>();
    public readonly List<(string path, ModConfigurationSection section)> SubSections = new List<(string path, ModConfigurationSection section)>();

    private ModConfigurationSection TouchSubSection(string subsection)
    {
        (string path, ModConfigurationSection section) sub1 = SubSections.FirstOrDefault(sub => sub.path == subsection);

        if (sub1 != default)
        {
            return sub1.section;
        }
            
        ModConfigurationSection sub2 = new ModConfigurationSection();
        SubSections.Add((subsection, sub2));
        return sub2;

    }
    public void Insert(string[] path, (string name, FieldInfo info, object confAttribute, string currentStringValue) property)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string t in path)
        {
            sb.Append(t + "/");
        }
        if (path.Length > 0)
        {
            List<string> subPath = new List<string>();
            for (int i = 1; i < path.Length; i++)
            {
                subPath.Add(path[i]);
            }

            ModConfigurationSection receivedSub = TouchSubSection(path[0]);
                
            receivedSub.Insert(subPath.ToArray(),property);
        }
        else
        {
            Properties.Add(property);
        }
    }
}