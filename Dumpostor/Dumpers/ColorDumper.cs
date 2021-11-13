using System.Linq;
using System.Text.Json;
using Il2CppSystem;
using UnhollowerBaseLib;

namespace Dumpostor.Dumpers
{
    public class ColorDumper : IDumper
    {
        public string FileName => "colors.json";

        public string Dump()
        {
            TranslationController.Instance.Awake();

            var id = 0;

            return JsonSerializer.Serialize(
                Palette.ColorNames.ToDictionary(k => TranslationController.Instance.GetString(k, new Il2CppReferenceArray<Object>(0)).ToLower().Capitalize(), _ => id++)
            );
        }
    }
}
