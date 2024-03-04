using System;

public class ColorOperations
{
    // Method to perform XOR operation on two colors
    public static string XOR(string color1, string color2)
    {
        int result = Convert.ToInt32(color1, 2) ^ Convert.ToInt32(color2, 2);
        return Convert.ToString(result, 2).PadLeft(3, '0');
    }

    // Method to perform XNOR operation on two colors
    public static string XNOR(string color1, string color2)
    {
        int result = ~(Convert.ToInt32(color1, 2) ^ Convert.ToInt32(color2, 2));
        string binaryResult = Convert.ToString(result & 7, 2).PadLeft(3, '0');
        return binaryResult;
    }

    // Method to perform OR operation on two colors
    public static string OR(string color1, string color2)
    {
        int result = Convert.ToInt32(color1, 2) | Convert.ToInt32(color2, 2);
        return Convert.ToString(result, 2).PadLeft(3, '0');
    }

    // Method to perform AND operation on two colors
    public static string AND(string color1, string color2)
    {
        int result = Convert.ToInt32(color1, 2) & Convert.ToInt32(color2, 2);
        return Convert.ToString(result, 2).PadLeft(3, '0');
    }
}