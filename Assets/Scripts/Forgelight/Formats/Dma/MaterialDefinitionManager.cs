using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using Forgelight.Formats.Dme;

namespace Forgelight.Formats.Dma
{
    public class MaterialDefinitionManager
    {
        #region Singleton

        private static MaterialDefinitionManager instance = null;

        public static void CreateInstance()
        {
            instance = new MaterialDefinitionManager();

            using (MemoryStream materialsXML = AssetManager.Instance.CreateAssetMemoryStreamByName("materials_3.xml"))
            {
                materialsXML.Position = 0;

                using (StreamReader streamReader = new StreamReader(materialsXML))
                {
                    string xmlDoc = streamReader.ReadToEnd();

                    using (StringReader stringReader = new StringReader(xmlDoc))
                    {
                        instance.loadFromStringReader(stringReader);
                    }
                }
            }
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static MaterialDefinitionManager Instance
        {
            get { return instance; }
        }

        #endregion

        public Dictionary<UInt32, MaterialDefinition> MaterialDefinitions { get; private set; }
        public Dictionary<UInt32, VertexLayout> VertexLayouts { get; private set; }

        MaterialDefinitionManager()
        {
            MaterialDefinitions = new Dictionary<UInt32, MaterialDefinition>();
            VertexLayouts = new Dictionary<UInt32, VertexLayout>();
        }

        private void loadFromStringReader(StringReader stringReader)
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
            loadVertexLayoutsByXPathNavigator(navigator.Clone());

            //TODO: parameter groups

            //material definitions
            loadMaterialDefinitionsByXPathNavigator(navigator.Clone());
        }

        private void loadMaterialDefinitionsByXPathNavigator(XPathNavigator navigator)
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

        private void loadVertexLayoutsByXPathNavigator(XPathNavigator navigator)
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