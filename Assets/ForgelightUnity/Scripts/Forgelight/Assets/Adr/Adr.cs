namespace ForgelightUnity.Forgelight.Assets.Adr
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using UnityEngine;

    public class Adr : Asset
    {
        /// <summary>
        /// The name of this asset, with extension.
        /// </summary>
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }

        public string Base { get; private set; }
        public string MaterialType { get; private set; }

        public List<Lod> Lods { get; private set; }

        public bool IsPlaceable { get; private set; }

        public static Adr LoadFromStream(string name, string displayName, Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            XElement root;

            try
            {
                using (XmlReader xr = XmlReader.Create(stream, settings))
                {
                    if (!xr.Read())
                    {
                        return null;
                    }

                    root = XElement.Load(xr.ReadSubtree());
                }
            }
            catch
            {
                return null;
            }

            Adr adr = new Adr();

            adr.Name = name;
            adr.DisplayName = displayName;
            adr.IsPlaceable = true;
            adr.Lods = new List<Lod>();

            foreach (XElement child in root.Elements())
            {
                if (child.Name == "Base")
                {
                    XAttribute attribute = child.Attribute("fileName");

                    if (attribute == null)
                    {
                        Debug.LogWarning("Actor " + adr.Name + " has an invalid Base definition. This actor may not display correctly.");
                        continue;
                    }

                    adr.Base = attribute.Value;
                }

                else if (child.Name == "Lods")
                {
                    foreach (XElement lodElement in child.Elements())
                    {
                        Lod lod = new Lod();
                        //lod.Distance = Convert.ToInt32(lodElement.Attribute("distance").Value);

                        XAttribute fileName = lodElement.Attribute("fileName");
                        if (fileName != null)
                        {
                            lod.FileName = fileName.Value;
                        }

                        XAttribute paletteName = lodElement.Attribute("paletteName");
                        if (paletteName != null)
                        {
                            lod.PaletteName = paletteName.Value;
                        }

                        adr.Lods.Add(lod);
                    }
                }

                else if (child.Name == "Usage")
                {
                    if (child.Attribute("actorUsage").Value != "0" || child.Attribute("borrowSkeleton").Value != "0" && child.Attribute("validatePcNpc").Value != "0")
                    {
                        adr.IsPlaceable = false;
                    }
                }

                else if (child.Name == "MaterialType")
                {
                    adr.MaterialType = child.Attribute("type").Value;
                }
            }

            return adr;
        }
    }
}
