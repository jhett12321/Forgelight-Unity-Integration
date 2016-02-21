using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using Forgelight.Formats.Dme;

namespace Forgelight.Formats.Dma
{
    public class MaterialDefinitionManager
    {
        public Dictionary<UInt32, MaterialDefinition> MaterialDefinitions { get; private set; }
        public Dictionary<UInt32, VertexLayout> VertexLayouts { get; private set; }

        public MaterialDefinitionManager(ForgelightGame forgelightGame)
        {
            MaterialDefinitions = new Dictionary<UInt32, MaterialDefinition>();
            VertexLayouts = new Dictionary<UInt32, VertexLayout>();

            using (MemoryStream materialsXML = forgelightGame.CreateAssetMemoryStreamByName("materials_3.xml"))
            {
                materialsXML.Position = 0;

                using (StreamReader streamReader = new StreamReader(materialsXML))
                {
                    string xmlDoc = streamReader.ReadToEnd();

                    using (StringReader stringReader = new StringReader(xmlDoc))
                    {
                        LoadFromStringReader(stringReader);
                    }
                }
            }
        }

        private void LoadFromStringReader(StringReader stringReader)
        {
            if (stringReader == null)
                return;

            XPathDocument document = null;

            try
            {
                document = new XPathDocument(stringReader);
            }
            catch (Exception)
            {
                return;
            }

            XPathNavigator navigator = document.CreateNavigator();

            //vertex layouts
            LoadVertexLayoutsByXPathNavigator(navigator.Clone());

            //TODO: parameter groups

            //material definitions
            LoadMaterialDefinitionsByXPathNavigator(navigator.Clone());
        }

        private void LoadMaterialDefinitionsByXPathNavigator(XPathNavigator navigator)
        {
            XPathNodeIterator materialDefinitions = null;

            try
            {
                materialDefinitions =
                    navigator.Select("/Object/Array[@Name='MaterialDefinitions']/Object[@Class='MaterialDefinition']");
            }
            catch (Exception)
            {
                return;
            }

            while (materialDefinitions.MoveNext())
            {
                MaterialDefinition materialDefinition =
                    MaterialDefinition.LoadFromXPathNavigator(materialDefinitions.Current);

                if (materialDefinition != null && false == MaterialDefinitions.ContainsKey(materialDefinition.NameHash))
                {
                    MaterialDefinitions.Add(materialDefinition.NameHash, materialDefinition);
                }
            }
        }

        private void LoadVertexLayoutsByXPathNavigator(XPathNavigator navigator)
        {
            //material definitions
            XPathNodeIterator vertexLayouts = null;

            try
            {
                vertexLayouts = navigator.Select("/Object/Array[@Name='InputLayouts']/Object[@Class='InputLayout']");
            }
            catch (Exception)
            {
                return;
            }

            while (vertexLayouts.MoveNext())
            {
                VertexLayout vertexLayout = VertexLayout.LoadFromXPathNavigator(vertexLayouts.Current);

                if (vertexLayout != null && false == VertexLayouts.ContainsKey(vertexLayout.NameHash))
                {
                    VertexLayouts.Add(vertexLayout.NameHash, vertexLayout);
                }
            }
        }
    }
}