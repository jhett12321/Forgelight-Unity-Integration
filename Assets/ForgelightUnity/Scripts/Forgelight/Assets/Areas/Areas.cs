namespace ForgelightUnity.Forgelight.Assets.Areas
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using UnityEngine;

    public class Areas : Asset
    {
        public List<AreaDefinition> AreaDefinitions = new List<AreaDefinition>();
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }

        public static Areas LoadFromStream(string name, string displayName, Stream stream)
        {
            Areas areas = new Areas();

            areas.Name = name;
            areas.DisplayName = displayName;

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            XDocument definitionsXML = new XDocument(new XElement("root"));
            XElement root = definitionsXML.Descendants().First();

            using (XmlReader xr = XmlReader.Create(stream, settings))
            {
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        root.Add(XElement.Load(xr.ReadSubtree()));
                    }
                }
            }

            foreach (XElement areaDefTag in root.Elements())
            {
                AreaDefinition areaDefinition = new AreaDefinition();

                //Common
                areaDefinition.ID = areaDefTag.Attribute("id").Value;
                areaDefinition.Name = areaDefTag.Attribute("name").Value;
                areaDefinition.Shape = areaDefTag.Attribute("shape").Value;
                areaDefinition.Pos1 = new Vector3(float.Parse(areaDefTag.Attribute("x1").Value), float.Parse(areaDefTag.Attribute("y1").Value), float.Parse(areaDefTag.Attribute("z1").Value));

                //Shapes
                switch (areaDefinition.Shape)
                {
                    case "sphere":
                        areaDefinition.Radius = float.Parse(areaDefTag.Attribute("radius").Value);
                        break;
                    case "box":
                        areaDefinition.Pos2 = new Vector3(float.Parse(areaDefTag.Attribute("x2").Value), float.Parse(areaDefTag.Attribute("y2").Value), float.Parse(areaDefTag.Attribute("z2").Value));
                        areaDefinition.Rot = new Vector3(float.Parse(areaDefTag.Attribute("rotX").Value), float.Parse(areaDefTag.Attribute("rotY").Value), float.Parse(areaDefTag.Attribute("rotZ").Value));
                        break;
                    default:
                        Debug.LogWarning("Unknown Shape (PROBABLY A DOME): " + areaDefinition.Shape);
                        continue;
                }

                if (areaDefTag.HasElements)
                {
                    areaDefinition.Properties = new List<Property>();

                    //Properties
                    foreach (XElement childProperty in areaDefTag.Elements())
                    {
                        Property property = new Property();

                        //Common
                        property.ID = childProperty.Attribute("id").Value;
                        property.Type = childProperty.Attribute("type").Value;

                        property.Parameters = childProperty;

                        areaDefinition.Properties.Add(property);
                    }
                }

                areas.AreaDefinitions.Add(areaDefinition);
            }

            return areas;
        }

        public static void SerializeDefinitionsToStream(Areas areas, Stream stream)
        {

        }
    }
}
