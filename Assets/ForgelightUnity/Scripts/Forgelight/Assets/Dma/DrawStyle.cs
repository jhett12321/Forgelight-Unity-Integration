namespace ForgelightUnity.Forgelight.Assets.Dma
{
    using System.Xml.XPath;
    using Utils.Cryptography;

    public class DrawStyle
    {
        #region Structure
        public string Name { get; private set; }
        public uint NameHash { get; private set; }
        public string Effect { get; private set; }
        public uint VertexLayoutNameHash { get; private set; }
        #endregion

        private DrawStyle()
        {
            Name = string.Empty;
            NameHash = 0;
            Effect = string.Empty;
            VertexLayoutNameHash = 0;
        }

        public static DrawStyle LoadFromXPathNavigator(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return null;
            }

            DrawStyle drawStyle = new DrawStyle();

            //name
            drawStyle.Name = navigator.GetAttribute("Name", string.Empty);
            drawStyle.NameHash = Jenkins.OneAtATime(drawStyle.Name);

            //effect
            drawStyle.Effect = navigator.GetAttribute("Effect", string.Empty);

            //input layout
            string vertexLayout = navigator.GetAttribute("InputLayout", string.Empty);
            drawStyle.VertexLayoutNameHash = Jenkins.OneAtATime(vertexLayout);

            return drawStyle;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}