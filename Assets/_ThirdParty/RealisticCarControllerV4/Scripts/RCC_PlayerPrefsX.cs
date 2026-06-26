//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


// ArrayPrefs2 v 1.4

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Extended PlayerPrefs utility supporting additional types: bool, long, Vector2, Vector3, Quaternion, Color,
/// and arrays of float, int, bool, string, Vector2, Vector3, Quaternion, and Color. Based on ArrayPrefs2 v1.4.
/// </summary>
public class RCC_PlayerPrefsX {

    static private int endianDiff1;
    static private int endianDiff2;
    static private int idx;
    static private byte[] byteBlock;

    enum ArrayType { Float, Int32, Bool, String, Vector2, Vector3, Quaternion, Color }

    /// <summary>
    /// Stores a bool value in PlayerPrefs under the given key (encoded as int 1/0).
    /// </summary>
    /// <param name="name">PlayerPrefs key.</param>
    /// <param name="value">Bool value to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetBool(String name, bool value) {
        try {
            PlayerPrefs.SetInt(name, value ? 1 : 0);
        } catch {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Retrieves a previously-stored bool from PlayerPrefs.
    /// </summary>
    /// <param name="name">PlayerPrefs key.</param>
    /// <returns>True when the stored int equals 1; false otherwise (including when the key is missing).</returns>
    public static bool GetBool(String name) {
        return PlayerPrefs.GetInt(name) == 1;
    }

    /// <summary>
    /// Retrieves a previously-stored bool from PlayerPrefs.
    /// </summary>
    /// <param name="name">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static bool GetBool(String name, bool defaultValue) {
        return (1 == PlayerPrefs.GetInt(name, defaultValue ? 1 : 0));
    }

    /// <summary>
    /// Retrieves a previously-stored long from PlayerPrefs (split across two int entries: key_lowBits and key_highBits).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static long GetLong(string key, long defaultValue) {
        int lowBits, highBits;
        SplitLong(defaultValue, out lowBits, out highBits);
        lowBits = PlayerPrefs.GetInt(key + "_lowBits", lowBits);
        highBits = PlayerPrefs.GetInt(key + "_highBits", highBits);

        // unsigned, to prevent loss of sign bit.
        ulong ret = (uint)highBits;
        ret = (ret << 32);
        return (long)(ret | (ulong)(uint)lowBits);
    }

    /// <summary>
    /// Retrieves a previously-stored long from PlayerPrefs (split across two int entries: key_lowBits and key_highBits).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored value, or 0 if the key is missing.</returns>
    public static long GetLong(string key) {
        int lowBits = PlayerPrefs.GetInt(key + "_lowBits");
        int highBits = PlayerPrefs.GetInt(key + "_highBits");

        // unsigned, to prevent loss of sign bit.
        ulong ret = (uint)highBits;
        ret = (ret << 32);
        return (long)(ret | (ulong)(uint)lowBits);
    }

    private static void SplitLong(long input, out int lowBits, out int highBits) {
        // unsigned everything, to prevent loss of sign bit.
        lowBits = (int)(uint)(ulong)input;
        highBits = (int)(uint)(input >> 32);
    }

    /// <summary>
    /// Stores a long value in PlayerPrefs by splitting it into two int entries: key_lowBits and key_highBits.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="value">Long value to store.</param>
    public static void SetLong(string key, long value) {
        int lowBits, highBits;
        SplitLong(value, out lowBits, out highBits);
        PlayerPrefs.SetInt(key + "_lowBits", lowBits);
        PlayerPrefs.SetInt(key + "_highBits", highBits);
    }

    /// <summary>
    /// Stores a Vector2 value in PlayerPrefs (encoded as a 2-element float array).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="vector">Vector2 value to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetVector2(String key, Vector2 vector) {
        return SetFloatArray(key, new float[] { vector.x, vector.y });
    }

    static Vector2 GetVector2(String key) {
        var floatArray = GetFloatArray(key);
        if (floatArray.Length < 2) {
            return Vector2.zero;
        }
        return new Vector2(floatArray[0], floatArray[1]);
    }

    /// <summary>
    /// Retrieves a previously-stored Vector2 from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Vector2 GetVector2(String key, Vector2 defaultValue) {
        if (PlayerPrefs.HasKey(key)) {
            return GetVector2(key);
        }
        return defaultValue;
    }

    /// <summary>
    /// Stores a Vector3 value in PlayerPrefs (encoded as a 3-element float array).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="vector">Vector3 value to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetVector3(String key, Vector3 vector) {
        return SetFloatArray(key, new float[] { vector.x, vector.y, vector.z });
    }

    /// <summary>
    /// Retrieves a previously-stored Vector3 from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored value, or <see cref="Vector3.zero"/> if the key is missing.</returns>
    public static Vector3 GetVector3(String key) {
        var floatArray = GetFloatArray(key);
        if (floatArray.Length < 3) {
            return Vector3.zero;
        }
        return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
    }

    /// <summary>
    /// Retrieves a previously-stored Vector3 from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Vector3 GetVector3(String key, Vector3 defaultValue) {
        if (PlayerPrefs.HasKey(key)) {
            return GetVector3(key);
        }
        return defaultValue;
    }

    /// <summary>
    /// Stores a Quaternion value in PlayerPrefs (encoded as a 4-element float array).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="vector">Quaternion value to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetQuaternion(String key, Quaternion vector) {
        return SetFloatArray(key, new float[] { vector.x, vector.y, vector.z, vector.w });
    }

    /// <summary>
    /// Retrieves a previously-stored Quaternion from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored value, or <see cref="Quaternion.identity"/> if the key is missing.</returns>
    public static Quaternion GetQuaternion(String key) {
        var floatArray = GetFloatArray(key);
        if (floatArray.Length < 4) {
            return Quaternion.identity;
        }
        return new Quaternion(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
    }

    /// <summary>
    /// Retrieves a previously-stored Quaternion from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Quaternion GetQuaternion(String key, Quaternion defaultValue) {
        if (PlayerPrefs.HasKey(key)) {
            return GetQuaternion(key);
        }
        return defaultValue;
    }

    /// <summary>
    /// Stores a Color value in PlayerPrefs (encoded as a 4-element float array: r, g, b, a).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="color">Color value to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetColor(String key, Color color) {
        return SetFloatArray(key, new float[] { color.r, color.g, color.b, color.a });
    }

    /// <summary>
    /// Retrieves a previously-stored Color from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored value, or transparent black (0,0,0,0) if the key is missing.</returns>
    public static Color GetColor(String key) {
        var floatArray = GetFloatArray(key);
        if (floatArray.Length < 4) {
            return new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        return new Color(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
    }

    /// <summary>
    /// Retrieves a previously-stored Color from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Value returned when the key is missing.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Color GetColor(String key, Color defaultValue) {
        if (PlayerPrefs.HasKey(key)) {
            return GetColor(key);
        }
        return defaultValue;
    }

    /// <summary>
    /// Stores an array of bool values in PlayerPrefs (packed as bits with a 5-byte header for type and entry count).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="boolArray">Bool array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetBoolArray(String key, bool[] boolArray) {
        // Make a byte array that's a multiple of 8 in length, plus 5 bytes to store the number of entries as an int32 (+ identifier)
        // We have to store the number of entries, since the boolArray length might not be a multiple of 8, so there could be some padded zeroes
        var bytes = new byte[(boolArray.Length + 7) / 8 + 5];
        bytes[0] = System.Convert.ToByte(ArrayType.Bool);   // Identifier
        var bits = new BitArray(boolArray);
        bits.CopyTo(bytes, 5);
        Initialize();
        ConvertInt32ToBytes(boolArray.Length, bytes); // The number of entries in the boolArray goes in the first 4 bytes

        return SaveBytes(key, bytes);
    }

    /// <summary>
    /// Retrieves a previously-stored array of bool values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static bool[] GetBoolArray(String key) {
        if (PlayerPrefs.HasKey(key)) {
            var bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(key));
            if (bytes.Length < 5) {
#if UNITY_EDITOR
                Debug.LogError("Corrupt preference file for " + key);
#endif
                return new bool[0];
            }
            if ((ArrayType)bytes[0] != ArrayType.Bool) {
#if UNITY_EDITOR
                Debug.LogError(key + " is not a boolean array");
#endif
                return new bool[0];
            }
            Initialize();

            // Make a new bytes array that doesn't include the number of entries + identifier (first 5 bytes) and turn that into a BitArray
            var bytes2 = new byte[bytes.Length - 5];
            System.Array.Copy(bytes, 5, bytes2, 0, bytes2.Length);
            var bits = new BitArray(bytes2);
            // Get the number of entries from the first 4 bytes after the identifier and resize the BitArray to that length, then convert it to a boolean array
            bits.Length = ConvertBytesToInt32(bytes);
            var boolArray = new bool[bits.Count];
            bits.CopyTo(boolArray, 0);

            return boolArray;
        }
        return new bool[0];
    }

    /// <summary>
    /// Retrieves a previously-stored array of bool values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static bool[] GetBoolArray(String key, bool defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetBoolArray(key);
        }
        var boolArray = new bool[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            boolArray[i] = defaultValue;
        }
        return boolArray;
    }

    /// <summary>
    /// Stores an array of strings in PlayerPrefs. Individual strings must be non-null and 255 characters or fewer.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="stringArray">String array to store.</param>
    /// <returns>True on success, false if the key cannot be written or any element is null / longer than 255 characters.</returns>
    public static bool SetStringArray(String key, String[] stringArray) {
        var bytes = new byte[stringArray.Length + 1];
        bytes[0] = System.Convert.ToByte(ArrayType.String); // Identifier
        Initialize();

        // Store the length of each string that's in stringArray, so we can extract the correct strings in GetStringArray
        for (var i = 0; i < stringArray.Length; i++) {
            if (stringArray[i] == null) {
#if UNITY_EDITOR
                Debug.LogError("Can't save null entries in the string array when setting " + key);
#endif
                return false;
            }
            if (stringArray[i].Length > 255) {
#if UNITY_EDITOR
                Debug.LogError("Strings cannot be longer than 255 characters when setting " + key);
#endif
                return false;
            }
            bytes[idx++] = (byte)stringArray[i].Length;
        }

        try {
            PlayerPrefs.SetString(key, System.Convert.ToBase64String(bytes) + "|" + String.Join("", stringArray));
        } catch {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Retrieves a previously-stored array of strings from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static String[] GetStringArray(String key) {
        if (PlayerPrefs.HasKey(key)) {
            var completeString = PlayerPrefs.GetString(key);
            var separatorIndex = completeString.IndexOf("|"[0]);
            if (separatorIndex < 4) {
#if UNITY_EDITOR
                Debug.LogError("Corrupt preference file for " + key);
#endif
                return new String[0];
            }
            var bytes = System.Convert.FromBase64String(completeString.Substring(0, separatorIndex));
            if ((ArrayType)bytes[0] != ArrayType.String) {
#if UNITY_EDITOR
                Debug.LogError(key + " is not a string array");
#endif
                return new String[0];
            }
            Initialize();

            var numberOfEntries = bytes.Length - 1;
            var stringArray = new String[numberOfEntries];
            var stringIndex = separatorIndex + 1;
            for (var i = 0; i < numberOfEntries; i++) {
                int stringLength = bytes[idx++];
                if (stringIndex + stringLength > completeString.Length) {
#if UNITY_EDITOR
                    Debug.LogError("Corrupt preference file for " + key);
#endif
                    return new String[0];
                }
                stringArray[i] = completeString.Substring(stringIndex, stringLength);
                stringIndex += stringLength;
            }

            return stringArray;
        }
        return new String[0];
    }

    /// <summary>
    /// Retrieves a previously-stored array of strings from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static String[] GetStringArray(String key, String defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetStringArray(key);
        }
        var stringArray = new String[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            stringArray[i] = defaultValue;
        }
        return stringArray;
    }

    /// <summary>
    /// Stores an array of int values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="intArray">Int array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetIntArray(String key, int[] intArray) {
        return SetValue(key, intArray, ArrayType.Int32, 1, ConvertFromInt);
    }

    /// <summary>
    /// Stores an array of float values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="floatArray">Float array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetFloatArray(String key, float[] floatArray) {
        return SetValue(key, floatArray, ArrayType.Float, 1, ConvertFromFloat);
    }

    /// <summary>
    /// Stores an array of Vector2 values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="vector2Array">Vector2 array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetVector2Array(String key, Vector2[] vector2Array) {
        return SetValue(key, vector2Array, ArrayType.Vector2, 2, ConvertFromVector2);
    }

    /// <summary>
    /// Stores an array of Vector3 values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="vector3Array">Vector3 array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetVector3Array(String key, Vector3[] vector3Array) {
        return SetValue(key, vector3Array, ArrayType.Vector3, 3, ConvertFromVector3);
    }

    /// <summary>
    /// Stores an array of Quaternion values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="quaternionArray">Quaternion array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetQuaternionArray(String key, Quaternion[] quaternionArray) {
        return SetValue(key, quaternionArray, ArrayType.Quaternion, 4, ConvertFromQuaternion);
    }

    /// <summary>
    /// Stores an array of Color values in PlayerPrefs (Base64-encoded byte stream prefixed with an array-type identifier).
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="colorArray">Color array to store.</param>
    /// <returns>True on success, false if the key cannot be written.</returns>
    public static bool SetColorArray(String key, Color[] colorArray) {
        return SetValue(key, colorArray, ArrayType.Color, 4, ConvertFromColor);
    }

    private static bool SetValue<T>(String key, T array, ArrayType arrayType, int vectorNumber, Action<T, byte[], int> convert) where T : IList {
        var bytes = new byte[(4 * array.Count) * vectorNumber + 1];
        bytes[0] = System.Convert.ToByte(arrayType);    // Identifier
        Initialize();

        for (var i = 0; i < array.Count; i++) {
            convert(array, bytes, i);
        }
        return SaveBytes(key, bytes);
    }

    private static void ConvertFromInt(int[] array, byte[] bytes, int i) {
        ConvertInt32ToBytes(array[i], bytes);
    }

    private static void ConvertFromFloat(float[] array, byte[] bytes, int i) {
        ConvertFloatToBytes(array[i], bytes);
    }

    private static void ConvertFromVector2(Vector2[] array, byte[] bytes, int i) {
        ConvertFloatToBytes(array[i].x, bytes);
        ConvertFloatToBytes(array[i].y, bytes);
    }

    private static void ConvertFromVector3(Vector3[] array, byte[] bytes, int i) {
        ConvertFloatToBytes(array[i].x, bytes);
        ConvertFloatToBytes(array[i].y, bytes);
        ConvertFloatToBytes(array[i].z, bytes);
    }

    private static void ConvertFromQuaternion(Quaternion[] array, byte[] bytes, int i) {
        ConvertFloatToBytes(array[i].x, bytes);
        ConvertFloatToBytes(array[i].y, bytes);
        ConvertFloatToBytes(array[i].z, bytes);
        ConvertFloatToBytes(array[i].w, bytes);
    }

    private static void ConvertFromColor(Color[] array, byte[] bytes, int i) {
        ConvertFloatToBytes(array[i].r, bytes);
        ConvertFloatToBytes(array[i].g, bytes);
        ConvertFloatToBytes(array[i].b, bytes);
        ConvertFloatToBytes(array[i].a, bytes);
    }

    /// <summary>
    /// Retrieves a previously-stored array of int values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static int[] GetIntArray(String key) {
        var intList = new List<int>();
        GetValue(key, intList, ArrayType.Int32, 1, ConvertToInt);
        return intList.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of int values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static int[] GetIntArray(String key, int defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetIntArray(key);
        }
        var intArray = new int[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            intArray[i] = defaultValue;
        }
        return intArray;
    }

    /// <summary>
    /// Retrieves a previously-stored array of float values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static float[] GetFloatArray(String key) {
        var floatList = new List<float>();
        GetValue(key, floatList, ArrayType.Float, 1, ConvertToFloat);
        return floatList.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of float values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static float[] GetFloatArray(String key, float defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetFloatArray(key);
        }
        var floatArray = new float[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            floatArray[i] = defaultValue;
        }
        return floatArray;
    }

    /// <summary>
    /// Retrieves a previously-stored array of Vector2 values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static Vector2[] GetVector2Array(String key) {
        var vector2List = new List<Vector2>();
        GetValue(key, vector2List, ArrayType.Vector2, 2, ConvertToVector2);
        return vector2List.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of Vector2 values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Vector2[] GetVector2Array(String key, Vector2 defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetVector2Array(key);
        }
        var vector2Array = new Vector2[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            vector2Array[i] = defaultValue;
        }
        return vector2Array;
    }

    /// <summary>
    /// Retrieves a previously-stored array of Vector3 values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static Vector3[] GetVector3Array(String key) {
        var vector3List = new List<Vector3>();
        GetValue(key, vector3List, ArrayType.Vector3, 3, ConvertToVector3);
        return vector3List.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of Vector3 values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Vector3[] GetVector3Array(String key, Vector3 defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetVector3Array(key);
        }
        var vector3Array = new Vector3[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            vector3Array[i] = defaultValue;
        }
        return vector3Array;
    }

    /// <summary>
    /// Retrieves a previously-stored array of Quaternion values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static Quaternion[] GetQuaternionArray(String key) {
        var quaternionList = new List<Quaternion>();
        GetValue(key, quaternionList, ArrayType.Quaternion, 4, ConvertToQuaternion);
        return quaternionList.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of Quaternion values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Quaternion[] GetQuaternionArray(String key, Quaternion defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetQuaternionArray(key);
        }
        var quaternionArray = new Quaternion[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            quaternionArray[i] = defaultValue;
        }
        return quaternionArray;
    }

    /// <summary>
    /// Retrieves a previously-stored array of Color values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <returns>The stored array, or an empty array if the key is missing, corrupt, or of the wrong array type.</returns>
    public static Color[] GetColorArray(String key) {
        var colorList = new List<Color>();
        GetValue(key, colorList, ArrayType.Color, 4, ConvertToColor);
        return colorList.ToArray();
    }

    /// <summary>
    /// Retrieves a previously-stored array of Color values from PlayerPrefs.
    /// </summary>
    /// <param name="key">PlayerPrefs key.</param>
    /// <param name="defaultValue">Element used to fill the array when the key is missing.</param>
    /// <param name="defaultSize">Length of the fallback array when the key is missing.</param>
    /// <returns>The stored array, or a <paramref name="defaultSize"/>-length array of <paramref name="defaultValue"/> if the key is missing.</returns>
    public static Color[] GetColorArray(String key, Color defaultValue, int defaultSize) {
        if (PlayerPrefs.HasKey(key)) {
            return GetColorArray(key);
        }
        var colorArray = new Color[defaultSize];
        for (int i = 0; i < defaultSize; i++) {
            colorArray[i] = defaultValue;
        }
        return colorArray;
    }

    private static void GetValue<T>(String key, T list, ArrayType arrayType, int vectorNumber, Action<T, byte[]> convert) where T : IList {
        if (PlayerPrefs.HasKey(key)) {
            var bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(key));
            if ((bytes.Length - 1) % (vectorNumber * 4) != 0) {
#if UNITY_EDITOR
                Debug.LogError("Corrupt preference file for " + key);
#endif
                return;
            }
            if ((ArrayType)bytes[0] != arrayType) {
#if UNITY_EDITOR
                Debug.LogError(key + " is not a " + arrayType.ToString() + " array");
#endif
                return;
            }
            Initialize();

            var end = (bytes.Length - 1) / (vectorNumber * 4);
            for (var i = 0; i < end; i++) {
                convert(list, bytes);
            }
        }
    }

    private static void ConvertToInt(List<int> list, byte[] bytes) {
        list.Add(ConvertBytesToInt32(bytes));
    }

    private static void ConvertToFloat(List<float> list, byte[] bytes) {
        list.Add(ConvertBytesToFloat(bytes));
    }

    private static void ConvertToVector2(List<Vector2> list, byte[] bytes) {
        list.Add(new Vector2(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
    }

    private static void ConvertToVector3(List<Vector3> list, byte[] bytes) {
        list.Add(new Vector3(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
    }

    private static void ConvertToQuaternion(List<Quaternion> list, byte[] bytes) {
        list.Add(new Quaternion(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
    }

    private static void ConvertToColor(List<Color> list, byte[] bytes) {
        list.Add(new Color(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
    }

    /// <summary>
    /// Editor-only debug helper. Logs the array type stored under the given key (decoded from the first byte of the
    /// PlayerPrefs payload). No-op in player builds.
    /// </summary>
    /// <param name="key">PlayerPrefs key to inspect.</param>
    public static void ShowArrayType(String key) {
#if UNITY_EDITOR
        var bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(key));
        if (bytes.Length > 0) {
            ArrayType arrayType = (ArrayType)bytes[0];
            Debug.Log(key + " is a " + arrayType.ToString() + " array");
        }
#endif
    }

    private static void Initialize() {
        if (System.BitConverter.IsLittleEndian) {
            endianDiff1 = 0;
            endianDiff2 = 0;
        } else {
            endianDiff1 = 3;
            endianDiff2 = 1;
        }
        if (byteBlock == null) {
            byteBlock = new byte[4];
        }
        idx = 1;
    }

    private static bool SaveBytes(String key, byte[] bytes) {
        try {
            PlayerPrefs.SetString(key, System.Convert.ToBase64String(bytes));
        } catch {
            return false;
        }
        return true;
    }

    private static void ConvertFloatToBytes(float f, byte[] bytes) {
        byteBlock = System.BitConverter.GetBytes(f);
        ConvertTo4Bytes(bytes);
    }

    private static float ConvertBytesToFloat(byte[] bytes) {
        ConvertFrom4Bytes(bytes);
        return System.BitConverter.ToSingle(byteBlock, 0);
    }

    private static void ConvertInt32ToBytes(int i, byte[] bytes) {
        byteBlock = System.BitConverter.GetBytes(i);
        ConvertTo4Bytes(bytes);
    }

    private static int ConvertBytesToInt32(byte[] bytes) {
        ConvertFrom4Bytes(bytes);
        return System.BitConverter.ToInt32(byteBlock, 0);
    }

    private static void ConvertTo4Bytes(byte[] bytes) {
        bytes[idx] = byteBlock[endianDiff1];
        bytes[idx + 1] = byteBlock[1 + endianDiff2];
        bytes[idx + 2] = byteBlock[2 - endianDiff2];
        bytes[idx + 3] = byteBlock[3 - endianDiff1];
        idx += 4;
    }

    private static void ConvertFrom4Bytes(byte[] bytes) {
        if (bytes == null || idx + 3 >= bytes.Length)
            return;
        byteBlock[endianDiff1] = bytes[idx];
        byteBlock[1 + endianDiff2] = bytes[idx + 1];
        byteBlock[2 - endianDiff2] = bytes[idx + 2];
        byteBlock[3 - endianDiff1] = bytes[idx + 3];
        idx += 4;
    }
}