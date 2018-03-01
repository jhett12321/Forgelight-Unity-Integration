namespace ForgelightUnity.Forgelight.Assets
{
    using System;

    public abstract class Asset : IComparable
    {
        /// <summary>
        /// The base name of this asset, as referenced in pack files.
        /// </summary>
        public abstract string Name { get; protected set; }

        /// <summary>
        /// The display name of this asset. Does not include extension, and appends the asset's origin pack.
        /// </summary>
        public abstract string DisplayName { get; protected set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            Asset otherAsset = (Asset) obj;

            return string.Compare(DisplayName, otherAsset.DisplayName, StringComparison.Ordinal);
        }
    }
}
