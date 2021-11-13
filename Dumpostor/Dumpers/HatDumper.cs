using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace Dumpostor.Dumpers
{
    public class Cosmetic
    {
        public Cosmetic(string type, string name, bool free, string storeName, int beanCost, int starCost, string bundleId,
            Vector2 chipOffset)
        {
            Type = type;
            Name = name;
            Free = free;
            StoreName = storeName;
            BeanCost = beanCost;
            StarCost = starCost;
            BundleId = bundleId;
            ChipOffset = chipOffset;
        }

        public string Type { get; }
        public string Name { get; }
        public bool Free { get; }
        public string StoreName { get; }
        public int BeanCost { get; }
        public int StarCost { get; }
        public string BundleId { get; }
        public Vector2 ChipOffset { get; }
    }
    public class HatDumper : IDumper
    {
        public string FileName => "cosmetics.json";

        public string Dump()
        {
            List<Cosmetic> cosmetics = new List<Cosmetic>();
            foreach (HatBehaviour hat in HatManager.Instance.AllHats)
            {
                cosmetics.Add(new Cosmetic("Hat", hat.ProdId, hat.Free, hat.StoreName, hat.beanCost, hat.starCost, hat.BundleId, hat.ChipOffset));
            }
            foreach (SkinData skin in HatManager.Instance.AllSkins)
            {
                cosmetics.Add(new Cosmetic("Skin", skin.ProdId, skin.Free, skin.StoreName, skin.beanCost, skin.starCost, skin.BundleId, skin.ChipOffset));
            }
            foreach (PetData pet in HatManager.Instance.AllPets)
            {
                cosmetics.Add(new Cosmetic("Pet", pet.ProdId, pet.Free, "", pet.beanCost, pet.starCost, pet.BundleId, pet.ChipOffset));
            }
            foreach (NamePlateData nameplate in HatManager.Instance.AllNamePlates)
            {
                cosmetics.Add(new Cosmetic("Nameplate", nameplate.ProdId, nameplate.Free, "", nameplate.beanCost, nameplate.starCost, nameplate.BundleId, nameplate.ChipOffset));
            }
            foreach (VisorData visor in HatManager.Instance.AllVisors)
            {
                cosmetics.Add(new Cosmetic("Visor", visor.ProdId, visor.Free, "", visor.beanCost, visor.starCost, visor.BundleId, visor.ChipOffset));
            }

            return JsonSerializer.Serialize(cosmetics,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new Vector2Converter()
                    }
                });
        }
    }
}
