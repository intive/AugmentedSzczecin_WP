using System;
using System.Globalization;

namespace Microsoft.Maps.SpatialToolbox
{
    public partial struct StyleColor
    {
        #region Public Properties

        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            if (obj is StyleColor)
            {
                var s = (StyleColor) obj;

                return s.A == A && s.B == B && s.G == G && s.R == R;
            }

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return string.Format("rgba({1},{2},{3},{0})", ((double) A)/255, R, G, B);
        }

        public string ToKmlColor()
        {
            //char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            //byte[] bytes = new byte[4];
            //bytes[0] = R;
            //bytes[1] = G;
            //bytes[2] = B;
            //bytes[3] = A;

            //char[] chars = new char[bytes.Length * 2];
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    int b = bytes[i];
            //    chars[i * 2] = hexDigits[b >> 4];
            //    chars[i * 2 + 1] = hexDigits[b & 0xF];
            //}

            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", A, B, G, R);
        }

        public string ToHex(bool includeHash, bool includeAlpha)
        {
            //char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            //byte[] bytes = new byte[4];
            //bytes[0] = R;
            //bytes[1] = G;
            //bytes[2] = B;
            //bytes[3] = A;

            //char[] chars = new char[bytes.Length * 2];
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    int b = bytes[i];
            //    chars[i * 2] = hexDigits[b >> 4];
            //    chars[i * 2 + 1] = hexDigits[b & 0xF];
            //}

            string chars = string.Empty;

            if (includeAlpha)
            {
                chars = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", A, R, G, B);
            }
            else
            {
                chars = string.Format("{0:X2}{1:X2}{2:X2}", R, G, B);
            }

            return (includeHash) ? "#" + chars : chars;
        }

        #endregion

        #region Public Static Methods

        public static StyleColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new StyleColor() {A = a, R = r, G = g, B = b};
        }

        public static StyleColor? FromString(string colorName)
        {
            return FromString(colorName, 255);
        }

        public static StyleColor? FromString(string colorName, byte defualtAlpha)
        {
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                switch (colorName)
                {
                    case "black":
                        return new StyleColor() {R = 0, G = 0, B = 0, A = defualtAlpha};
                    case "darkred":
                        return new StyleColor() {R = 139, G = 0, B = 0, A = defualtAlpha};
                    case "darkgreen":
                        return new StyleColor() {R = 0, G = 139, B = 0, A = defualtAlpha};
                    case "darkyellow":
                        return new StyleColor() {R = 204, G = 204, B = 0, A = defualtAlpha};
                    case "darkblue":
                        return new StyleColor() {R = 0, G = 0, B = 139, A = defualtAlpha};
                    case "darkmagenta":
                        return new StyleColor() {R = 139, G = 0, B = 139, A = defualtAlpha};
                    case "darkcyan":
                        return new StyleColor() {R = 0, G = 139, B = 139, A = defualtAlpha};
                    case "lightgray":
                        return new StyleColor() {R = 211, G = 211, B = 211, A = defualtAlpha};
                    case "darkgray":
                        return new StyleColor() {R = 169, G = 169, B = 169, A = defualtAlpha};
                    case "red":
                        return new StyleColor() {R = 255, G = 0, B = 0, A = defualtAlpha};
                    case "green":
                        return new StyleColor() {R = 0, G = 255, B = 0, A = defualtAlpha};
                    case "yellow":
                        return new StyleColor() {R = 255, G = 255, B = 0, A = defualtAlpha};
                    case "blue":
                        return new StyleColor() {R = 0, G = 0, B = 255, A = defualtAlpha};
                    case "magenta":
                        return new StyleColor() {R = 255, G = 0, B = 255, A = defualtAlpha};
                    case "cyan":
                        return new StyleColor() {R = 0, G = 255, B = 255, A = defualtAlpha};
                    case "white":
                        return new StyleColor() {R = 255, G = 255, B = 255, A = defualtAlpha};
                    case "transparent":
                        return new StyleColor() {R = 0, G = 0, B = 0, A = 0};
                    default:
                        return FromHex(colorName);
                }
            }

            return null;
        }

        public static StyleColor? FromHex(string hex)
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                uint abgr = 0;
                int max = Math.Min(hex.Length, 8); // We consider only the first eight characters significant.
                for (int i = 0; i < max; ++i)
                {
                    // Always increase the color, even if the char isn't a valid number
                    abgr <<= 4; // Move along one hex - 2^4
                    string letter = hex[i].ToString();
                    uint number;
                    if (uint.TryParse(letter, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
                    {
                        abgr += number;
                    }
                }

                if (max == 6)
                {
                    return new StyleColor()
                    {
                        A = 255,
                        B = (byte) (abgr >> 16),
                        G = (byte) (abgr >> 8),
                        R = (byte) abgr
                    };
                }
                else if (max > 6)
                {
                    return new StyleColor()
                    {
                        A = (byte) (abgr >> 24),
                        B = (byte) (abgr >> 16),
                        G = (byte) (abgr >> 8),
                        R = (byte) abgr
                    };
                }
            }

            return null;
        }

        #endregion
    }
}